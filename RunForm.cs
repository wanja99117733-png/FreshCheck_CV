using FreshCheck_CV.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public RunForm()
        {
            InitializeComponent();
            ApplyDarkTheme();

            this.FormClosed += RunForm_FormClosed;
        }

        private void RunForm_FormClosed(object sender, FormClosedEventArgs e)
        {
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

            // RUN 표시
            Global.Inst?.InspStage?.Hub?.SetRunning(true);

            
            // 이벤트 중복 구독 방지
            if (MainForm.Instance != null)
            {
                MainForm.Instance.ImageChanged -= MainForm_ImageChanged;
                MainForm.Instance.ImageChanged += MainForm_ImageChanged;

                // 이미지 사이클링 시작(이미 구현된 private StartImageCycle를 public wrapper로 노출한 상태라고 가정)
                MainForm.Instance.TryStartImageCycle();
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;

            Global.Inst?.InspStage?.Hub?.SetRunning(false);

            MainForm.Instance?.StopImageCyclePublic();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _isInspectEnabled = false;

            Global.Inst?.InspStage?.Hub?.SetRunning(false);

            if (MainForm.Instance != null)
            {
                MainForm.Instance.StopImageCyclePublic();
                MainForm.Instance.ImageChanged -= MainForm_ImageChanged;
            }
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
            if (!_isInspectEnabled)
                return;

            // 같은 UI 스레드에서 연속 호출되거나, 검사가 오래 걸릴 경우 재진입 방지
            if (_isInspectBusy)
                return;

            try
            {
                _isInspectBusy = true;

                // 지금 FC는 Mold 임시 검사
                Global.Inst?.InspStage?.RunMoldInspectionTemp();
            }
            finally
            {
                _isInspectBusy = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.Grab(0);
        }
    }
}