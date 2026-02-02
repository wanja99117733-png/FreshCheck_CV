using FreshCheck_CV.Defect;
using FreshCheck_CV.Grab;
using FreshCheck_CV.Inspect;
using FreshCheck_CV.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;


namespace FreshCheck_CV.Core
{
    
    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage : IDisposable
    {
        private BinaryOptions _lastBinaryOptions = new BinaryOptions();
        public BinaryOptions LastBinaryOptions
        {
            get
            {
                return _lastBinaryOptions;
            }
            set
            {
                _lastBinaryOptions = value ?? new BinaryOptions();
            }
        }

        // 원본 이미지
        private Bitmap _sourceBitmap = null;
        private GrabModel _grabManager = null;
        SaigeAI _saigeAI; // SaigeAI 인스턴스
        private CameraType _camType = CameraType.HikRobotCam;

        public string LastFinalResult { get; private set; } = "OK";

        public InspStage() { }

        public SaigeAI AIModule
        {
            get
            {
                if (_saigeAI is null)
                    _saigeAI = new SaigeAI();
                return _saigeAI;
            }
        }


        public InspectionHub Hub { get; } = new InspectionHub();

        public bool Initialize()
        {
            return true;
        }


        // ImageViewCtrl 화면 이미지 업데이트 함수
        public void UpdateDisplay(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return;
            }

            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDisplay(bitmap);
            }
        }

        // ImageViewCtrl 프리뷰 이미지 업데이트 함수
        public void UpdatePreview(Bitmap bitmap)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdatePreview(bitmap);
            }
        }


        // (Bitmap) ImageViewCtrl 현재 이미지 가져오기 함수
        public Bitmap GetCurrentImage()
        {
            Bitmap bitmap = null;
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                bitmap = cameraForm.GetDisplayImage();
            }

            return bitmap;
        }


        // (Bitmap) ImageViewCtrl 현재 이미지 가져오기 함수
        public Bitmap GetPreviewImage()
        {
            Bitmap bitmap = null;
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                bitmap = cameraForm.GetPreviewImage();
            }

            return bitmap;
        }

        public void LoadImage(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
                return;

            using (var temp = (Bitmap)Image.FromFile(filePath))
            {
                SetSourceImage(new Bitmap(temp));
            }
        }

        public void SetSourceImage(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            // 기존 원본 존재하면 메모리 해제
            if (_sourceBitmap != null)
            {
                _sourceBitmap.Dispose();
                _sourceBitmap = null;
            }

            // 원본 저장
            _sourceBitmap = new Bitmap(bitmap);
            // 화면에 원본 표시
            UpdateDisplay(new Bitmap(_sourceBitmap));

        }
        public void ShowSource()
        {
            if (_sourceBitmap == null)
                return;

            UpdateDisplay(new Bitmap(_sourceBitmap));
        }

        public void ApplyBinary(BinaryOptions options)
        {
            // 마지막 옵션 저장
            _lastBinaryOptions = options ?? new BinaryOptions();

            if (options == null)
                options = new BinaryOptions();

            // 원본이 없으면 현재 화면에서라도 가져와서 원본으로 설정
            if (_sourceBitmap == null)
            {
                var cur = GetCurrentImage();
                if (cur == null) return;

                SetSourceImage(cur); // 여기서 화면 원본 표시까지 됨
            }

            // 항상 원본 기준으로 처리
            Bitmap result = BinaryProcessor.ApplyPreview(_sourceBitmap, options);
            if (result == null)
                return;

            // 처리 결과를 화면에 표시
            UpdateDisplay(result);
        }

        public void Grab(int bufferIndex)
        {
            if (_grabManager == null)
                return;

            _grabManager.Grab(bufferIndex, true);
        }
        public void RunMoldInspectionTemp()
        {

            var swTotal = System.Diagnostics.Stopwatch.StartNew();

            if (_sourceBitmap == null)
                return;

            DateTime now = DateTime.Now;

            // 1) 검사용 입력은 무조건 "원본" 복사본
            Bitmap detectSource = new Bitmap(_sourceBitmap);

            var detector = new MoldDetector(() => _lastBinaryOptions)
            {
                AreaRatioThreshold = 0.005
            };

            DefectResult result = detector.Detect(detectSource);

            // detectSource는 더 이상 필요 없으니 해제(메모리 누수 방지)
            detectSource.Dispose();

            bool isMold = result != null && result.IsDefect && result.Type == DefectType.Mold;

            string resultText = isMold ? "Mold" : "OK";
            string label = isMold ? "Mold" : "OK";
            DefectType saveType = isMold ? DefectType.Mold : DefectType.OK;
            // 2) 저장도 원본으로 저장하는 게 맞음 (원하면 여기서도 _sourceBitmap 쓰기)
            string savedPath = DefectImageSaver.Save(new Bitmap(_sourceBitmap), saveType, now, label);

            swTotal.Stop();

            // Inspection Monitor용 집계 이벤트 푸시
            var dto = new InspectionResultDto
            {
                Timestamp = now,
                IsOk = !isMold,
                Type = isMold ? MonitorDefectType.Mold : MonitorDefectType.None,
                Ratio = result?.AreaRatio,
                SavedPath = savedPath,
                Message = result?.Message
            };


            Hub.Push(dto);

            // 유진형(스크래치 검사 시간)
            Util.SLogger.Write($"Result : {label} | " + $"Mold ratio={(result?.AreaRatio ?? 0.0):0.0000} | " + $"MoldDetect={(result?.ElapsedMs ?? 0)}ms | Total={swTotal.ElapsedMilliseconds}ms");


            // ResultForm: 항상 1건 기록
            var resultForm = MainForm.GetDockForm<ResultForm>();
            if (resultForm != null)
            {
                var record = new Models.ResultRecord
                {
                    Time = now,
                    Result = resultText,
                    DefectType = isMold ? "Mold" : "None",
                    Ratio = result?.AreaRatio ?? 0.0,
                    SavedPath = savedPath,
                    Message = result?.Message ?? string.Empty
                };

                resultForm.AddRecord(record);
            }

            // DefectForm: NG일 때만 마스킹 이미지
            if (isMold)
            {
                var defectForm = MainForm.GetDockForm<DefectForm>();
                defectForm?.AddDefectImage(
                    result?.OverlayBitmap,
                    $"{now:HH:mm:ss.fff}  {label}  ratio={result?.AreaRatio:0.0000}",
                    savedPath);
            }
            else
            {
                // OK면 OverlayBitmap이 null일 가능성이 높음(정상)
                if (result?.OverlayBitmap != null)
                {
                    result.OverlayBitmap.Dispose();
                }
            }
        }


        // Global.Inst.InspStage 또는 적절한 검사 관리 클래스 내에 구현
        public bool RunFullInspectionCycle()
        {
            bool isNG = true;

            var swTotal = System.Diagnostics.Stopwatch.StartNew();

            Bitmap original = null;
            Bitmap noBgImage = null;
            DefectResult moldResult = null;
            Bitmap displayBase = null;

            // ✅ Scratch를 Mold 결과에 따라 실행/스킵해야 하므로 미리 선언
            SegmentationResult scratchResult = null;
            bool isScratch = false;
            bool isMold = false;

            try
            {
                var saigeAI = this.AIModule;
                if (saigeAI == null)
                {
                    return true; // 검사 불가 => NG로 처리(안전)
                }

                // 1) 원본 이미지
                original = GetCurrentImage();
                if (original == null)
                {
                    return true;
                }

                DateTime now = DateTime.Now;

                // STEP 1) 배경 제거
                if (!saigeAI.InspAIModule(original, AIEngineType.Segmentation))
                {
                    return true;
                }

                noBgImage = saigeAI.GetResultImage();
                if (noBgImage == null)
                {
                    return true;
                }

                // STEP 2) Mold
                var moldDetector = new MoldDetector(() => _lastBinaryOptions)
                {
                    AreaRatioThreshold = 0.01
                };

                moldResult = moldDetector.Detect(noBgImage, original);
                isMold = moldResult != null && moldResult.IsDefect && moldResult.Type == DefectType.Mold;

                // ✅ STEP 3) Scratch (Mold가 아닐 때만 실행)
                if (!isMold)
                {
                    if (!saigeAI.InspAIModule(noBgImage, AIEngineType.ScratchSegmentation))
                    {
                        FreshCheck_CV.Dialogs.CustomMessageBoxForm.Show("Scratch 검출이 실패하였습니다.", "시스템 오류");
                        return true;
                    }

                    scratchResult = saigeAI.GetScratchResult();
                    isScratch = scratchResult != null &&
                                scratchResult.SegmentedObjects != null &&
                                scratchResult.SegmentedObjects.Length > 0;
                }
                else
                {
                    // Mold면 Scratch는 스킵 (명시적으로)
                    scratchResult = null;
                    isScratch = false;
                }

                // STEP 4) 최종 판정
                isNG = isMold || isScratch;

                // ✅ Mold면 결과는 Mold로 확정(= Scratch를 아예 안 돌았으므로 둘 다 표기 불가)
                string finalResult =
                    isMold ? "Mold" :
                    (isScratch ? "Scratch" : "OK");

                LastFinalResult = finalResult;

                // STEP 5) 화면 출력용 베이스 이미지
                if (isMold && moldResult?.OverlayBitmap != null)
                {
                    displayBase = (Bitmap)moldResult.OverlayBitmap.Clone();
                }
                else
                {
                    displayBase = (Bitmap)original.Clone();
                }

                // ✅ Mold면 scratchResult가 null일 수 있음 → UpdatePreviewWithScratch가 null-safe여야 함(아래 2번 패치 참고)
                UpdatePreviewWithScratch(displayBase, scratchResult);

                // STEP 6) 저장/기록
                DefectType saveType = isMold ? DefectType.Mold : (isScratch ? DefectType.Scratch : DefectType.OK);

                string savedPath;
                using (var originalCopy = new Bitmap(original)) // ★ new Bitmap(original) 누수 방지
                {
                    savedPath = DefectImageSaver.Save(originalCopy, saveType, now, finalResult);
                }

                int scratchCount = (scratchResult?.SegmentedObjects?.Length) ?? 0;

                // 1) Hub 업데이트
                var dto = new InspectionResultDto
                {
                    Timestamp = now,
                    IsOk = !isNG,
                    Type = isMold ? MonitorDefectType.Mold : (isScratch ? MonitorDefectType.Scratch : MonitorDefectType.None),
                    SavedPath = savedPath,
                    Message = $"[Result: {finalResult}] " +
                              (isMold ? $"Ratio: {moldResult.AreaRatio:F4} " : "") +
                              (!isMold ? (isScratch ? $"Scratch: {scratchCount}ea" : "Scratch: 0ea") : "Scratch: skipped")
                };
                Hub.Push(dto);

                // 2) ResultForm 기록
                var resultForm = MainForm.GetDockForm<ResultForm>();
                resultForm?.AddRecord(new Models.ResultRecord
                {
                    Time = now,
                    Result = finalResult,
                    DefectType = isNG ? finalResult : "None",
                    Ratio = moldResult?.AreaRatio ?? 0.0,
                    SavedPath = savedPath,
                    Message = dto.Message
                });

                // 3) 로그
                swTotal.Stop();
                Util.SLogger.Write(
                    $"[FullCycle] Result: {finalResult} | " +
                    $"Mold: {isMold}({moldResult?.ElapsedMs}ms) | " +
                    (isMold ? "Scratch: skipped | " : $"Scratch: {isScratch}({scratchCount}ea) | ") +
                    $"Total: {swTotal.ElapsedMilliseconds}ms");

                // 4) DefectForm (NG일 때만)
                if (isNG)
                {
                    MainForm.GetDockForm<DefectForm>()?.AddDefectImage(
                        displayBase,
                        $"{now:HH:mm:ss} [{finalResult}]",
                        savedPath);

                    // DefectForm이 displayBase.Dispose()를 내부에서 함 → 여기서는 중복 Dispose 방지
                    displayBase = null;
                }
                else
                {
                    // OK일 때는 누수 방지 위해 직접 해제 가능 (ImageViewCtrl은 Clone해서 내부 보관함)
                    displayBase.Dispose();
                    displayBase = null;
                }

                return isNG;
            }
            finally
            {
                // swTotal이 Stop 안 되었으면 정리
                if (swTotal.IsRunning)
                {
                    swTotal.Stop();
                }

                // noBgImage는 항상 우리가 Dispose
                if (noBgImage != null)
                {
                    noBgImage.Dispose();
                    noBgImage = null;
                }

                // MoldDetector가 가진 OverlayBitmap은 우리가 더 이상 쓰지 않으니 정리
                if (moldResult?.OverlayBitmap != null)
                {
                    moldResult.OverlayBitmap.Dispose();
                }

                // displayBase는 위에서 NG/OK 분기에서 처리했지만, 혹시 남아있으면 정리
                if (displayBase != null)
                {
                    displayBase.Dispose();
                }
            }
        }

        public void UpdatePreviewWithScratch(Bitmap displayBase, SegmentationResult scratchResult)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm == null)
                return;

            // ✅ Scratch 결과가 없으면 base 이미지만 표시
            if (scratchResult == null ||
                scratchResult.SegmentedObjects == null ||
                scratchResult.SegmentedObjects.Length == 0)
            {
                cameraForm.UpdatePreview(displayBase);
                return;
            }

            cameraForm.UpdatePreviewWithScratch(displayBase, scratchResult);
        }




        //#17_WORKING_STATE#2 작업 상태 설정
        public void SetWorkingState(WorkingState workingState)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.SetWorkingState(workingState);
            }
        }

        #region Disposable

        private bool disposed = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }


                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion //Disposable
    }
}
