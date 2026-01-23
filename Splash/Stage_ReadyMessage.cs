using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreshCheck_CV.InitProgress;

namespace FreshCheck_CV.Splash
{
    namespace FreshCheck_CV
    {
        internal sealed class Stage_ReadyMessage : IStartupStage
        {
            public string Name { get { return "마무리"; } }
            public int StageWeightPercent { get { return 10; } } // 예시: 전체 합 100 맞게 조정

            public async Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
            {
                int minMs = 800;

                var cps = new StartupProgressHelper.Checkpoint[]
                {
                new StartupProgressHelper.Checkpoint(  0, 20, "UI 준비 중..."),
                new StartupProgressHelper.Checkpoint(200, 60, "마지막 점검 중..."),
                new StartupProgressHelper.Checkpoint(800, 90, "시작 준비 완료"),
                };

                await StartupProgressHelper.RunWithMinimumDurationAsync(
                    work: () => Task.CompletedTask,
                    progress: progress,
                    ct: ct,
                    minDurationMs: minMs,
                    checkpoints: cps).ConfigureAwait(true);
            }
        }
    }
}