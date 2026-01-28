
using FreshCheck_CV.Defect;
using FreshCheck_CV.Models;
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

        public InspStage() { }

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
