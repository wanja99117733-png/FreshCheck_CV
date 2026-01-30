using FreshCheck_CV.Defect;
using FreshCheck_CV.Grab;
using FreshCheck_CV.Inspect;
using FreshCheck_CV.Models;
using FreshCheck_CV.Sequence;
using FreshCheck_CV.Setting;
using FreshCheck_CV.Util;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
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
        // 원본 이미지
        private Bitmap _sourceBitmap = null;
        private GrabModel _grabManager = null;
        SaigeAI _saigeAI; // SaigeAI 인스턴스
        private CameraType _camType = CameraType.HikRobotCam;



        private Model _model = null;
        public Model CurModel
        {
            get => _model;
        }
        private string _capturePath = "";
        private bool _isInspectMode = false;
        public bool UseCamera { get; set; } = false;

        public bool SaveCamImage { get; set; } = false;
        public int SaveImageIndex { get; set; } = 0;
        public bool LiveMode { get; set; } = false;


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
            _model = new Model();

            //#19_VISION_SEQUENCE#3 VisionSequence 초기화
            VisionSequence.Inst.InitSequence();
            VisionSequence.Inst.SeqCommand += SeqCommand;

            return true;
        }
        public bool LoadModel(string filePath)
        {
            SLogger.Write($"모델 로딩:{filePath}");

            _model = _model.Load(filePath);

            if (_model is null)
            {
                SLogger.Write($"모델 로딩 실패:{filePath}");
                return false;
            }

            return true;
        }


        public void SaveModel(string filePath)
        {
            SLogger.Write($"모델 저장:{filePath}");

            //입력 경로가 없으면 현재 모델 저장
            if (string.IsNullOrEmpty(filePath))
                Global.Inst.InspStage.CurModel.Save();
            else
                Global.Inst.InspStage.CurModel.SaveAs(filePath);
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

        public bool Grab(int bufferIndex)
        {
            if (_grabManager == null)
                return false;

            if (!_grabManager.Grab(bufferIndex, true))
                return false;

            return true;
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
                AreaRatioThreshold = 0.01
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
        public void RunFullInspectionCycle()
        {
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            var saigeAI = this.AIModule;
            if (saigeAI == null) return;


            // 1. 원본 이미지 가져오기
            Bitmap original = GetCurrentImage();
            if (original == null) return;
            DateTime now = DateTime.Now;

            // --- [STEP 1: 배경 제거] ---
            if (!saigeAI.InspAIModule(original, AIEngineType.Segmentation)) return;
            Bitmap noBgImage = saigeAI.GetResultImage(); // 배경이 제거된 검정 바탕 이미지

            // --- [STEP 2: Mold 검사] ---
            // 유저 요청에 따라 배경이 제거된 이미지(noBgImage)로 Mold 검사 수행
            var moldDetector = new MoldDetector(() => _lastBinaryOptions) { AreaRatioThreshold = 0.01 };

            // Mold 검사 수행
            DefectResult moldResult = moldDetector.Detect(noBgImage, original);
            bool isMold = moldResult != null && moldResult.IsDefect && moldResult.Type == DefectType.Mold;

            // --- [STEP 3: Scratch 검사] ---
            if (!saigeAI.InspAIModule(noBgImage, AIEngineType.ScratchSegmentation))
            {
                MessageBox.Show("Scratch 검출 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SegmentationResult scratchResult = saigeAI.GetScratchResult();
            bool isScratch = scratchResult != null && scratchResult.SegmentedObjects.Length > 0;


            // --- [STEP 4: 다중 결함 판정 로직] ---
            List<string> defectList = new List<string>();

            if (isMold) defectList.Add("Mold");
            if (isScratch) defectList.Add("Scratch");

            bool isNG = isMold || isScratch;
            string finalResult = isNG ? string.Join(", ", defectList) : "OK";

            swTotal.Stop();

            // --- [STEP 5: 최종 화면 출력] ---
            // Mold 하이라이트 이미지(resultBaseImage) 위에 Scratch 박스(scratchResult)를 겹쳐서 그림
            Bitmap displayBase = null;

            if (isMold && moldResult.OverlayBitmap != null)
                // Mold 결함이 있을 때 Overlay 이미지를 복제
                displayBase = (Bitmap)moldResult.OverlayBitmap.Clone();
            else if (original != null)
                // 결함이 없거나 Overlay가 없을 때 원본 복제
                displayBase = (Bitmap)original.Clone();

            UpdatePreviewWithScratch(displayBase, scratchResult);

            // --- [STEP 6: 데이터 기록 및 저장] ---

            // 최종 판정 로직 (Mold나 Scratch 중 하나라도 있으면 NG)
            string resultText = isNG ? (isMold ? "Mold" : "Scratch") : "OK";
            string label = resultText;

            // 저장용 타입 결정 (우선순위: Mold > Scratch)
            DefectType saveType = isMold ? DefectType.Mold : (isScratch ? DefectType.Scratch : DefectType.OK);

            // 이미지 저장 (원본 기준으로 저장)
            string savedPath = DefectImageSaver.Save(new Bitmap(original), saveType, now, finalResult);

            // 1) Inspection Monitor(Hub) 업데이트
            var dto = new InspectionResultDto
            {
                Timestamp = now,
                IsOk = !isNG,
                Type = isMold ? MonitorDefectType.Mold : (isScratch ? MonitorDefectType.Scratch : MonitorDefectType.None),
                SavedPath = savedPath,
                Message = $"[Result: {finalResult}] " +
                  (isMold ? $"Ratio: {moldResult.AreaRatio:F4} " : "") +
                  (isScratch ? $"Scratch: {scratchResult.SegmentedObjects.Length}ea" : "")
            };
            Hub.Push(dto);

            // 2) ResultForm 기록 (최종 결과 기준)
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

            // 3) SLogger 기록
            Util.SLogger.Write($"[FullCycle] Result: {label} | Mold: {isMold}({moldResult?.ElapsedMs}ms) | Scratch: {isScratch} | Total: {swTotal.ElapsedMilliseconds}ms");


            // 4) DefectForm 업데이트 (NG일 때)
            if (isNG)
            {
                MainForm.GetDockForm<DefectForm>()?.AddDefectImage(
                    displayBase,
                    $"{now:HH:mm:ss} [{finalResult}]",
                    savedPath);
            }

            // 메모리 정리
            noBgImage.Dispose();
            if (moldResult?.OverlayBitmap != null) moldResult.OverlayBitmap.Dispose();
            // resultBaseImage는 UpdatePreviewWithScratch 내부에서 관리되므로 여기서 Dispose하지 않음 (ImageViewCtrl 내부 로직 확인 필요)
        }




        public void UpdatePreviewWithScratch(Bitmap bitmap, SegmentationResult scratchResult)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            // CameraForm 호출 (기존 패턴)
            cameraForm.UpdatePreviewWithScratch(bitmap, scratchResult);
        }



        public void StopCycle()
        {
            //#19_VISION_SEQUENCE#4 시퀀스 정지
            VisionSequence.Inst.StopAutoRun();
            _isInspectMode = false;

            SetWorkingState(WorkingState.NONE);
        }



        public bool StartAutoRun()
        {
            SLogger.Write("Action : StartAutoRun");

            if (SaveCamImage && _model != null)
            {
                SaveImageIndex = 0;

                _capturePath = Path.Combine(Path.GetDirectoryName(_model.ModelPath), "Capture");
                if (!Directory.Exists(_capturePath))
                {
                    Directory.CreateDirectory(_capturePath);
                }
                else
                {
                    string[] files = Directory.GetFiles(_capturePath);
                    foreach (string file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            SLogger.Write($"Failed to delete file: {file}. Exception: {ex.Message}", SLogger.LogType.Error);
                        }
                    }
                }
            }

            string modelPath = CurModel.ModelPath;
            if (modelPath == "")
            {
                SLogger.Write("열려진 모델이 없습니다!", SLogger.LogType.Error);
                MessageBox.Show("열려진 모델이 없습니다!");
                return false;
            }

            LiveMode = false;
            UseCamera = SettingXml.Inst.CamType != CameraType.None ? true : false;

            SetWorkingState(WorkingState.INSPECT);

            //#19_VISION_SEQUENCE#5 자동검사 시작
            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            VisionSequence.Inst.StartAutoRun(modelName);
            _isInspectMode = true;
            return true;
        }


        private void SeqCommand(object sender, SeqCmd seqCmd, object Param)
        {
            switch (seqCmd)
            {
                case SeqCmd.InspStart:
                    {
                        //#WCF_FSM#5 카메라 촬상 후, 검사 진행
                        SLogger.Write("MMI : InspStart", SLogger.LogType.Info);

                        //검사 시작
                        string errMsg;

                        if (UseCamera)
                        {
                            if (!Grab(0))
                            {
                                errMsg = string.Format("Failed to grab");
                                SLogger.Write(errMsg, SLogger.LogType.Error);
                            }
                        }
                    }
                    break;
                case SeqCmd.InspEnd:
                    {
                        SLogger.Write("MMI : InspEnd", SLogger.LogType.Info);

                        //모든 검사 종료
                        string errMsg = "";

                        //검사 완료에 대한 처리
                        SLogger.Write("검사 종료");

                        VisionSequence.Inst.VisionCommand(Vision2Mmi.InspEnd, errMsg);
                    }
                    break;
            }
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

        public void SetExposure(long exposureTime)
        {
            if (_grabManager != null)
            {
                _grabManager.SetExposureTime(exposureTime);
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
