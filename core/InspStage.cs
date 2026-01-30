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
using FreshCheck_CV.Inspect;
using SaigeVision.Net.V2.Segmentation;
using System.Threading;
using System.ServiceModel;
using FreshCheck_CV.Sequence;


namespace FreshCheck_CV.Core
{
    
    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage : IDisposable


    {
        private long _inspectionSeq = 0;

        private BinaryOptions _lastBinaryOptions = new BinaryOptions();
        // 원본 이미지
        private Bitmap _sourceBitmap = null;
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

            TrySendWcf_MoldOnly(now, isMold, result, detector.AreaRatioThreshold, savedPath);

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

        private void TrySendWcf_MoldOnly(DateTime now, bool isMold, DefectResult result, double moldThreshold, string savedPath)
        {
            try
            {
                var comm = FreshCheck_CV.Core.Global.Inst.Communicator;
                if (comm == null)
                    return;

                // 1) 검사 번호
                long inspNo = System.Threading.Interlocked.Increment(ref _inspectionSeq);
                string productNo = inspNo.ToString(); // TODO: 나중에 서버/라인에서 받은 제품번호로 교체

                // 2) 누적 카운트(서버 UI 우측 Item/Value용)
                var snap = Hub.GetSnapshot();

                var counters = new FreshCheck_CV.Sequence.FcCounters
                {
                    Total = snap.Total,
                    Good = snap.Ok,
                    Ng = snap.Total - snap.Ok,
                    Mold = snap.Mold,
                    Scratch = snap.Scratch,
                    Both = snap.Both
                };

                // 3) 시간(지금 RunMoldInspectionTemp는 stopwatch가 없으니 0으로)
                //    원하면 swTotal 추가해서 넘기면 됨.
                var timing = new FreshCheck_CV.Sequence.FcTiming
                {
                    InspectStartTime = now.ToString("HH:mm:ss.fff"),
                    InspectEndTime = DateTime.Now.ToString("HH:mm:ss.fff"),
                    ProcessMsTotal = 0,
                    MoldMs = 0,
                    ScratchMs = 0
                };

                // 4) 메시지 구성
                var msg = new FreshCheck_CV.Sequence.FcInspectionResult
                {
                    InspectionNo = inspNo,
                    ProductNo = productNo,
                    Time = now.ToString("HH:mm:ss.fff"),

                    Judge = isMold ? FreshCheck_CV.Sequence.FcJudge.Ng : FreshCheck_CV.Sequence.FcJudge.Good,
                    NgReason = isMold ? FreshCheck_CV.Sequence.FcNgReason.Mold : FreshCheck_CV.Sequence.FcNgReason.None,

                    MoldRatio = result?.AreaRatio ?? 0.0,
                    MoldThreshold = moldThreshold,

                    // Scratch 아직 미연동
                    ScratchCount = 0,
                    ScratchScore = 0,

                    SavedPath = savedPath,
                    Message = result?.Message,

                    Counters = counters,
                    Timing = timing,

                    MachineName = comm.MachineName,
                    ModelName = comm.ModelName,
                    SerialId = comm.SerialId
                };

                // 5) 연결/전송
                if (comm.State != System.ServiceModel.CommunicationState.Opened)
                    comm.Connect();

                if (comm.State == System.ServiceModel.CommunicationState.Opened)
                {
                    var ack = comm.SendInspection(msg);
                    FreshCheck_CV.Util.SLogger.Write($"[WCF] InspDone inspNo={inspNo} ack={ack?.Ok} msg={ack?.Message}", FreshCheck_CV.Util.SLogger.LogType.Info);
                }
                else
                {
                    FreshCheck_CV.Util.SLogger.Write("[WCF] Not connected. Skip send.", FreshCheck_CV.Util.SLogger.LogType.Error);
                }
            }
            catch (Exception ex)
            {
                FreshCheck_CV.Util.SLogger.Write($"[WCF] SendInspection error : {ex.Message}", FreshCheck_CV.Util.SLogger.LogType.Error);
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
