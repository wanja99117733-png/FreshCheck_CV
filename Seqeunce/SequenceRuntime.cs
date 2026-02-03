using FreshCheck_CV.Core;
using FreshCheck_CV.Sequence;
using FreshCheck_CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FreshCheck_CV.Sequence.VisionSequence;

namespace FreshCheck_CV.Seqeunce
{
    internal sealed class Vision4SequenceRuntime : IDisposable
    {
        private readonly object _sync = new object();
        private ISynchronizeInvoke _ui;
        private bool _started;
        private bool _disposed;

        // 서버 트리거가 연속으로 들어올 때 중복 검사 방지
        private readonly System.Threading.SemaphoreSlim _inspGate
    = new System.Threading.SemaphoreSlim(1, 1);

        /// <summary>
        /// MainForm(=UI 스레드 보장 객체)을 바인딩하고 통신 시퀀스를 시작합니다.
        /// 반드시 도킹 윈도우가 생성된 이후(MainForm.LoadDockingWindows 이후) 호출하세요.
        /// </summary>
        public void BindUiAndStart(ISynchronizeInvoke uiInvoker)
        {
            if (uiInvoker == null) throw new ArgumentNullException(nameof(uiInvoker));

            lock (_sync)
            {
                if (_started)
                    return;

                _ui = uiInvoker;

                // 1) 서버 -> 비전 InspStart 이벤트 받기
                VisionSequence.Inst.SeqCommand += OnSeqCommand;

                // 2) 통신 시작 (InitSequence 내부에서 Communicator.Create + Thread 시작)
                VisionSequence.Inst.InitSequence();

                _started = true;
            }

            SLogger.Write("[Vision4] Runtime started");
        }

        /// <summary>비전->제어: OpenRecipe -> MmiStart 요청</summary>
        public void StartAutoRun()
        {
            string recipeName = Setting.SettingXml.Inst.RecipeName;

            if (string.IsNullOrWhiteSpace(recipeName))
                recipeName = Setting.SettingXml.Inst.MachineName; // fallback

            VisionSequence.Inst.StartAutoRun(recipeName);
        }

        /// <summary>비전->제어: MmiStop 요청</summary>
        public void StopAutoRun()
        {
            VisionSequence.Inst.StopAutoRun();
        }

        private void OnSeqCommand(object sender, SeqCmd cmd, object param)
        {
            if (cmd == SeqCmd.OpenRecipe)
            {
                VisionSequence.Inst.ReplyOpenRecipe(true);
                return;
            }

            // InspStart만 검사 트리거로 사용 (InspEnd 등은 무시)
            if (cmd != SeqCmd.InspStart)
                return;

            // ★ UI 스레드 절대 건드리지 않고 백그라운드에서 검사 실행
            _ = Task.Run(async () =>
            {
                await _inspGate.WaitAsync().ConfigureAwait(false);

                bool isNg = true;
                string reason = "NG";

                try
                {
                    // 여기서 검사 1회 실행 (UI가 아니라 백그라운드)
                    isNg = Global.Inst.InspStage.RunFullInspectionCycle();
                    reason = Global.Inst.InspStage.LastFinalResult ?? (isNg ? "NG" : "OK");
                }
                catch (Exception ex)
                {
                    isNg = true;
                    reason = "Exception";
                    SLogger.Write("[Vision4] Inspection exception: " + ex, SLogger.LogType.Error);
                }
                finally
                {
                    _inspGate.Release();
                }

                // ★ 결과 회신
                VisionSequence.Inst.VisionCommand(
                    Vision2Mmi.InspDone,
                    new InspDonePayload
                    {
                        IsNg = isNg,
                        Reason = reason
                    });
            });
        }

        private void InvokeOnUi(Action action)
        {
            if (action == null)
                return;

            var ui = _ui;
            if (ui != null && ui.InvokeRequired)
                ui.Invoke(action, new object[0]);
            else
                action();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try { VisionSequence.Inst.SeqCommand -= OnSeqCommand; } catch { }
            try { VisionSequence.Inst.Dispose(); } catch { }
        }
    }
}
