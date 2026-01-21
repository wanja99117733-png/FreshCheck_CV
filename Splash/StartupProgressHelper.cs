using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreshCheck_CV.Splash
{
    // 강제로 로딩창 띄우는 걸 보여주는 클래스 -> 프로그램 구현 후 삭제해도 됨.

    internal static class StartupProgressHelper
    {
        internal sealed class Checkpoint
        {
            public int AtMs { get; private set; }      // 이 시간(ms)이 되면
            public int Percent { get; private set; }   // 이 퍼센트로
            public string Message { get; private set; }// 이 문구 표시

            public Checkpoint(int atMs, int percent, string message)
            {
                AtMs = atMs;
                Percent = percent;
                Message = message ?? string.Empty;
            }
        }

        public static async Task RunWithMinimumDurationAsync(
            Func<Task> work,
            IProgress<InitProgress> progress,
            CancellationToken ct,
            int minDurationMs,
            Checkpoint[] checkpoints)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (checkpoints == null) checkpoints = new Checkpoint[0];

            int startTick = Environment.TickCount;

            // 실제 작업은 동시에 시작(빨리 끝나도 상관 없음)
            Task workTask = work();

            // 체크포인트를 시간 순서대로 실행하면서 강제 업데이트
            for (int i = 0; i < checkpoints.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                Checkpoint cp = checkpoints[i];

                int elapsed = Environment.TickCount - startTick;
                int wait = cp.AtMs - elapsed;

                if (wait > 0)
                    await Task.Delay(wait, ct).ConfigureAwait(true);

                progress.Report(new InitProgress(Clamp(cp.Percent, 0, 100), cp.Message));
            }

            // 실제 작업이 끝날 때까지 대기
            await workTask.ConfigureAwait(true);

            // 최소 로딩 시간 강제
            int elapsedTotal = Environment.TickCount - startTick;
            int remain = minDurationMs - elapsedTotal;
            if (remain > 0)
                await Task.Delay(remain, ct).ConfigureAwait(true);

            // 끝은 100%로 보정(문구는 Stage가 원하면 넣고, 아니면 공백)
            progress.Report(new InitProgress(100, string.Empty));
        }

        private static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}