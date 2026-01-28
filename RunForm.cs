using FreshCheck_CV.Core;
using FreshCheck_CV.Grab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private int _currentBufferIdx = 0;

        public RunForm()
        {
            InitializeComponent();
            ApplyDarkTheme();

            this.FormClosed += RunForm_FormClosed;

            CheckCameraConnection();  // 🔑 초기 카메라 연결 확인
        }

        // ===== 1. 카메라 연결 확인 (생성자 후 바로 실행) =====
        private void CheckCameraConnection()
        {
            try
            {
                _hikCam = new HikRobotCam();
                // IP는 실제 카메라 IP로 변경! (Global 설정에서 가져올 수 있음)
                if (!_hikCam.Create("192.168.1.100") || !_hikCam.InitGrab())
                {
                    _isCameraConnected = false;
                    return;
                }

                _hikCam.InitBuffer(2);
                _imageBuffers = _hikCam._userImageBuffer;

                int w, h, s, bpp;
                _hikCam.GetResolution(out w, out h, out s);
                _hikCam.GetPixelBpp(out bpp);

                // 버퍼 할당
                for (int i = 0; i < 2; i++)
                {
                    byte[] buf = new byte[s * h];
                    GCHandle hndl = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    _hikCam.SetBuffer(buf, hndl.AddrOfPinnedObject(), hndl, i);
                }

                _hikCam.SetTriggerMode(false);  // 소프트 트리거
                _isCameraConnected = true;
            }
            catch
            {
                _isCameraConnected = false;
            }
        }

        private void RunForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _captureTimer?.Stop();
            if (_imageBuffers != null)
            {
                foreach (var buf in _imageBuffers)
                    buf.ImageHandle?.Free();
            }
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
            Global.Inst?.InspStage?.Hub?.SetRunning(true);

            if (_isCameraConnected)
            {
                // 실시간 캡처 모드
                _captureTimer = new System.Windows.Forms.Timer();
                _captureTimer.Interval = 1000;  // 1초 간격
                _captureTimer.Tick += CaptureTimer_Tick;
                _captureTimer.Start();
            }
            else
            {
                // 사이클링 모드 (기존 동작)
            }

            // 공통: 이벤트 + 사이클링 (카메라 있어도 ImageChanged는 유지)
            if (MainForm.Instance != null)
            {
                MainForm.Instance.ImageChanged -= MainForm_ImageChanged;
                MainForm.Instance.ImageChanged += MainForm_ImageChanged;
                MainForm.Instance.TryStartImageCycle();
            }

            StartInspectionLoop();  // 기존 루프 시작
        }

        // ===== 3. 1000ms마다 캡처 =====
        private void CaptureTimer_Tick(object sender, EventArgs e)
        {
            if (_hikCam != null)
            {
                int idx = _currentBufferIdx;
                _hikCam.Grab(idx, true);  // 소프트 트리거 발사
                _currentBufferIdx = 1 - idx;  // 0↔1 순환
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;
            Global.Inst?.InspStage?.Hub?.SetRunning(false);

            _captureTimer?.Stop();  // 카메라 타이머 정지
            MainForm.Instance?.StopImageCyclePublic();  // 사이클링도 정지

            PauseInspectionLoop();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;
            Global.Inst?.InspStage?.Hub?.SetRunning(false);

            _captureTimer?.Stop();
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

            // 시작 시 이미지 사이클링
            MainForm.Instance?.TryStartImageCycle();

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
                // 일시정지면 여기서 대기
                _pauseGate.Wait(token);

                // 1) UI 스레드에서 검사 1회 실행 (Cross-thread 방지)
                InvokeOnUiThread(() =>
                {
                    // (Scratch 붙으면 여기서 1 Cycle로 묶어 호출)
                    Global.Inst?.InspStage?.RunMoldInspectionTemp();
                });

                // 2) 목표 주기 대기
                Task.Delay(LoopIntervalMs, token).Wait(token);
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
            if (!_isInspectEnabled || _isInspectBusy) return;

            try
            {
                _isInspectBusy = true;

                if (_isCameraConnected)
                {
                    // 카메라 이미지 우선 검사
                    byte[] camImage = _imageBuffers[_hikCam.BufferIndex].ImageBuffer;
                    Global.Inst?.InspStage?.RunMoldInspectionWithImage(camImage);  // 이미지 버퍼 전달
                }
                else
                {
                    // 사이클링 이미지 검사 (기존)
                    Global.Inst?.InspStage?.RunMoldInspectionTemp();
                }
            }
            finally
            {
                _isInspectBusy = false;
            }
        }
    }
}