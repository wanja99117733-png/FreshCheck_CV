using FreshCheck_CV.Defect;
using FreshCheck_CV.Grab;
using FreshCheck_CV.Inspect;
using FreshCheck_CV.Models;
using SaigeVision.Net.V2.Segmentation;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
            Bitmap source = GetCurrentImage();
            if (source == null)
                return;

            DateTime now = DateTime.Now;

            var detector = new MoldDetector(() => _lastBinaryOptions)
            {
                AreaRatioThreshold = 0.01
            };

            DefectResult result = detector.Detect(source);

            bool isMold = result != null && result.IsDefect && result.Type == DefectType.Mold;

            string resultText = isMold ? "NG" : "OK";
            string label = isMold ? "NG - Mold" : "OK";
            DefectType saveType = isMold ? DefectType.Mold : DefectType.None;

            string savedPath = DefectImageSaver.Save(source, saveType, now, label);

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

            // 로그(기록용)
            Util.SLogger.Write($"Mold Inspection: {label} | {result?.Message} | saved={savedPath}");

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

        
        public void UpdatePreviewWithScratch(Bitmap bitmap, SegmentationResult scratchResult)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            // CameraForm 호출 (기존 패턴)
            cameraForm.UpdatePreviewWithScratch(bitmap, scratchResult);
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
