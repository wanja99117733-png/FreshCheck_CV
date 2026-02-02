using FreshCheck_CV.Splash;
using FreshCheck_CV.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FreshCheck_CV.InitProgress;

namespace FreshCheck_CV
{
    internal sealed class Stage_LoadConfig : IStartupStage
    {
        public string Name { get { return "설정 로드"; } }
        public int StageWeightPercent { get { return 20; } }

        public async Task ExecuteAsync(StartupContext context, IProgress<InitProgress> progress, CancellationToken ct)
        {
            int minMs = 1200;

            var cps = new StartupProgressHelper.Checkpoint[]
            {
                new StartupProgressHelper.Checkpoint(   0,  5, "설정 파일 읽는 중..."),
                new StartupProgressHelper.Checkpoint( 200, 30, "옵션 값 파싱 중..."),
                new StartupProgressHelper.Checkpoint( 500, 65, "기본값/검증 적용 중..."),
                new StartupProgressHelper.Checkpoint(700, 90, "설정 적용 마무리 중..."),
            };

            await StartupProgressHelper.RunWithMinimumDurationAsync(
                work: () =>
                {
                    // TODO: 실제 설정 로드 코드로 교체
                    // 지금은 예시: context.Config = ...
                    context.Config = new AppConfig();

                    return Task.CompletedTask;
                },
                progress: progress,
                ct: ct,
                minDurationMs: minMs,
                checkpoints: cps).ConfigureAwait(true);
        }
    }
}