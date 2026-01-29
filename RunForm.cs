using FreshCheck_CV.Core;
using FreshCheck_CV.Grab;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace FreshCheck_CV
{
    public partial class RunForm : DockContent
    {
        private volatile bool _isInspectEnabled;
        private bool _liveMode = false;  // Live 모드 플래그
        private volatile bool _isInspectBusy;

        private CancellationTokenSource _cts;
        private Task _loopTask;
        private ManualResetEventSlim _pauseGate = new ManualResetEventSlim(true); // true=진행, false=대기
        private volatile bool _isLoopRunning;

        private const int LoopIntervalMs = 1000;

        private HikRobotCam _hikCam;
        private System.Windows.Forms.Timer _captureTimer;
        private GrabUserBuffer[] _imageBuffers;
        private bool _isCameraConnected = false;
        private int _width, _height, _stride;
        private PictureBox pictureBox;  // UI PictureBox

        public RunForm()
        {
            InitializeComponent();
            ApplyDarkTheme();
            CheckCameraConnection();  // 🔑 초기 카메라 연결 확인

            // 🔑 키보드 단축키 설정
            this.KeyPreview = true;
            this.KeyDown += RunForm_KeyDown;

            this.FormClosed += RunForm_FormClosed;
        }

        // 🔑 키보드 단축키 처리 (생성자에서 등록)
        

        // ===== 1. 카메라 연결 확인 (생성자 후 바로 실행) =====
        private void CheckCameraConnection()
        {
            try
            {
                _hikCam = new HikRobotCam();
                // IP는 실제 카메라 IP로 변경! (Global 설정에서 가져올 수 있음)
                if (!_hikCam.Create("169.254.90.253") || !_hikCam.InitGrab())
                {
                    _isCameraConnected = false;
                    return;
                }

                _hikCam.InitBuffer(2);
                _imageBuffers = _hikCam._userImageBuffer;

                int w, h, s, bpp;
                _hikCam.GetResolution(out w, out h, out s);
                _hikCam.GetPixelBpp(out bpp);
                _width = w; _height = h; _stride = s;
                Console.WriteLine($"카메라 스펙 - W:{w} H:{h} Stride:{s} Bpp:{bpp}");  // MVS BayerGR8 확인용

                // 버퍼 할당
                for (int i = 0; i < 2; i++)
                {
                    byte[] buf = new byte[s * h];
                    GCHandle hndl = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    _hikCam.SetBuffer(buf, hndl.AddrOfPinnedObject(), hndl, i);
                }

                // 🔑 TransferCompleted 콜백 등록 (GrabCompleted 대신!)
                _hikCam.TransferCompleted += MultiGrab_TransferCompleted;

                _hikCam.SetTriggerMode(false);

                _hikCam.Open();
                _isCameraConnected = true;
                _hikCam.SetWhiteBalance(true);
            }
            catch
            {
                _isCameraConnected = false;
            }
        }
        private readonly object _bufferLock = new object();

        private async void MultiGrab_TransferCompleted(object sender, object e)
        {
            if (!_isInspectEnabled) return;

            // 1. 인덱스 유효성 검사
            int bufferIndex = _hikCam.BufferIndex;
            if (bufferIndex < 0 || _imageBuffers == null || _imageBuffers[bufferIndex].ImageBuffer == null) return;

            try
            {
                // 2. 이미지 처리 (UI 업데이트 포함)
                // 비동기 상황에서 버퍼가 바뀌지 않도록 복사본 전달을 권장합니다.
                UpdateImageViewCtrl(bufferIndex);

                if (_liveMode)
                {
                    await Task.Delay(800); // 800ms는 너무 길 수 있으니 적절히 조절
                    if (_liveMode) _hikCam.Grab(bufferIndex, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Grab Error: {ex.Message}");
            }
        }

        private void RunForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // _captureTimer?.Stop();  ← 삭제!
            if (_imageBuffers != null)
            {
                foreach (var buf in _imageBuffers)
                    if (buf.ImageHandle.IsAllocated)
                        buf.ImageHandle.Free();
            }
            _hikCam?.Close();
            _hikCam?.Dispose();
            StopInspectionLoop();
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(28, 32, 38);
            this.ForeColor = Color.White;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = true;
            _liveMode = true;  // 🔑 Live 모드 시작!
            Global.Inst?.InspStage?.Hub?.SetRunning(true);

            if (_isCameraConnected)
            {
                // 첫 Grab으로 연쇄 시작!
                _hikCam.Grab(0, false);  // 비동기 → TransferCompleted 콜백
            }
            else
            {
                // 공통: 이벤트 + 사이클링 (카메라 있어도 ImageChanged는 유지)
                if (MainForm.Instance != null)
                {
                    MainForm.Instance.ImageChanged -= MainForm_ImageChanged;
                    MainForm.Instance.ImageChanged += MainForm_ImageChanged;
                    StartInspectionLoop();
                }
            }

            // 검사 시작시 모니터 탭으로 변경
            var propForm = MainForm.GetDockForm<PropertiesForm>();
            propForm.SelectMonitorTab();
        }
        private Bitmap ByteArrayToBitmap(byte[] buffer, int width, int height, int srcStride)
        {
            if (buffer == null || buffer.Length == 0) return null;

            try
            {
                // 1. RGB24 stride 패딩 제거 (7776=2592*3 완벽, padding 거의 없음)
                int expectedLineBytes = width * 3;
                byte[] cleanBuffer = new byte[expectedLineBytes * height];
                for (int y = 0; y < height; y++)
                {
                    Buffer.BlockCopy(buffer, y * srcStride, cleanBuffer, y * expectedLineBytes, expectedLineBytes);
                }

                Console.WriteLine($"RGB24 변환 - Stride:{srcStride} → Line:{expectedLineBytes} Total:{cleanBuffer.Length}");

                // 2. OpenCV Mat으로 RGB24 → BGR24 변환 (Bitmap 호환)
                using (Mat rgbMat = new Mat(height, width, MatType.CV_8UC3, cleanBuffer))
                using (Mat bgrMat = new Mat())
                {
                    Cv2.CvtColor(rgbMat, bgrMat, ColorConversionCodes.RGB2BGR);  // RGB→BGR [web:46]

                    // 화질 보정 (원본 가까이)
                    Cv2.ConvertScaleAbs(bgrMat, bgrMat, 1.05, 2);

                    Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bgrMat);
                    Console.WriteLine("RGB24 → 컬러 비트맵 성공!");
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RGB24 오류: {ex.Message}\nBuffer[0-10]: {BitConverter.ToString(buffer?.Take(10).ToArray() ?? new byte[0])}");
                return null;
            }
        }



        private void UpdateImageViewCtrl(int bufferIndex)
        {
            try
            {
                var cameraForm = MainForm.GetDockForm<CameraForm>();
                if (cameraForm == null) return;

                byte[] rawCopy;
                lock (_bufferLock)
                {
                    rawCopy = (byte[])_imageBuffers[bufferIndex].ImageBuffer.Clone();
                }

                // stride 명시적 전달로 밀림 완전 방지
                Bitmap bmp = ByteArrayToBitmap(rawCopy, _width, _height, _stride);

                if (bmp != null)
                {
                    cameraForm.Invoke(new Action(() => cameraForm.UpdateDisplay(bmp)));
                }
                else
                {
                    Console.WriteLine("비트맵 생성 실패 - stride 또는 Bayer 확인");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update UI Error: {ex.Message}");
            }
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;  // 🔑 콜백 제어!
            _liveMode = false;  // 🔑 Live 중지!

            MainForm.Instance?.StopImageCyclePublic();
            PauseInspectionLoop();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;
            _liveMode = false;  // 🔑 Live 중지!


            if (MainForm.Instance != null)
            {
                MainForm.Instance.StopImageCyclePublic();
                MainForm.Instance.ImageChanged -= MainForm_ImageChanged;
            }
            StopInspectionLoop();
        }

        private void StartInspectionLoop()
        {
            if (_isLoopRunning)
            {
                _pauseGate.Set();
                SetRunningFlag(true);

                // 재개 시에도 이미지 사이클링 재시작
                MainForm.Instance?.TryStartImageCycle();
                return;
            }

            _cts = new CancellationTokenSource();
            _pauseGate.Set();
            _isLoopRunning = true;
            SetRunningFlag(true);

            _loopTask = Task.Run(() => InspectionLoopWorker(_cts.Token));
        }

        // 루프 일시정지
        private void PauseInspectionLoop()
        {
            if (!_isLoopRunning)
                return;

            // 게이트를 닫아 루프 대기 상태로 전환
            _pauseGate.Reset();
            SetRunningFlag(false);

            MainForm.Instance?.StopImageCyclePublic();
        }

        // 루프 종료

        private void StopInspectionLoop()
        {
            if (!_isLoopRunning)
            {
                SetRunningFlag(false);
                MainForm.Instance?.StopImageCyclePublic();
                return;
            }

            try
            {
                _cts?.Cancel();
            }
            catch
            {
                // ignore
            }

            // 대기 중이면 빠져나오게 열어줌
            _pauseGate.Set();

            try
            {
                _loopTask?.Wait(500);
            }
            catch
            {
                // ignore
            }

            _cts?.Dispose();
            _cts = null;
            _loopTask = null;

            _isLoopRunning = false;
            SetRunningFlag(false);
        }

        private void InspectionLoopWorker(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _pauseGate.Wait(token);

                    // 1) UI 스레드에서 검사 1회 실행 (Cross-thread 방지)
                    InvokeOnUiThread(() =>
                    {
                        // (Scratch 붙으면 여기서 1 Cycle로 묶어 호출)
                        Global.Inst?.InspStage?.RunFullInspectionCycle();
                    });


                    var delayTask = Task.Delay(LoopIntervalMs, token);
                    delayTask.Wait(token);  // 이미 token 있음
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }

        // UI 스레드 실행 헬퍼
        private void InvokeOnUiThread(Action action)
        {
            if (action == null) return;

            try
            {
                if (IsDisposed) return;

                if (InvokeRequired)
                {
                    Invoke(action); // 동기 실행: 검사 호출이 겹치지 않게 보장
                }
                else
                {
                    action();
                }
            }
            catch
            {
                // 폼 종료 중 등 예외는 무시(안전 종료 목적)
            }
        }

        private void RunForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:      // 검사 시작
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    btnStart_Click(sender, e);
                    break;

                case Keys.F8:      // 일시 중지
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    btnPause_Click(sender, e);
                    break;

                case Keys.F12:     // 검사 중지
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    btnStop_Click(sender, e);
                    break;
            }
        }


        private void SetRunningFlag(bool isRunning)
        {
            try
            {
                Global.Inst?.InspStage?.Hub?.SetRunning(isRunning);
            }
            catch
            {
                // Hub가 아직 없거나 연결 전이면 무시
            }
        }
        private void MainForm_ImageChanged(object sender, MainForm.ImageChangedEventArgs e)
        {
            if (!_isInspectEnabled)
                return;

            // 같은 UI 스레드에서 연속 호출되거나, 검사가 오래 걸릴 경우 재진입 방지
            if (_isInspectBusy)
                return;

            try
            {
                _isInspectBusy = true;

                // 사이클링 이미지 검사 (기존)
                Global.Inst?.InspStage?.RunMoldInspectionTemp();
            }
            finally
            {
                _isInspectBusy = false;
            }
        }
    }
}