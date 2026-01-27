    using FreshCheck_CV.Splash;
    using FreshCheck_CV.Splash.FreshCheck_CV;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using static FreshCheck_CV.InitProgress;

    namespace FreshCheck_CV
    {
        internal static class AppInitializer
        {
            public static async Task<StartupContext> InitializeAsync(
                IProgress<InitProgress> progress,
                CancellationToken ct)
            {
                if (progress is null)
                    throw new ArgumentNullException(nameof(progress));

                StartupContext context = StartupContextFactory.Create();

                // 단계 구성(필요 시 단계 추가/삭제만 하면 됨)
                var stages = new List<IStartupStage>
                {
                    new Stage_RegisterExceptionHandlers(),
                    new Stage_PrepareDirectories(),
                    new Stage_LoadConfig(),
                    new Stage_InitGlobal(),
                    new Stage_InitInspStage(),
                    new Stage_ReadyMessage()
                    //new Stage_InitCameraOrGrab(), // 정책에 따라 optional 처리

                    
                };

                ValidateStageWeights(stages);

                int accumulated = 0;

                foreach (var stage in stages)
                {
                    ct.ThrowIfCancellationRequested();

                    int startPercent = accumulated;
                    int endPercent = accumulated + stage.StageWeightPercent;

                    Report(progress, startPercent, $"{stage.Name}...");

                    var sw = Stopwatch.StartNew();
                    await stage.ExecuteAsync(context, new Progress<InitProgress>(p =>
                    {
                        // stage 내부에서 0~100을 보내면, 전체 퍼센트로 맵핑
                        int clamped = Clamp(p.Percent, 0, 100);
                        int mapped = startPercent + (int)((endPercent - startPercent) * (clamped / 100.0));
                        Report(progress, mapped, p.Message);
                    }), ct).ConfigureAwait(true);
                    sw.Stop();

                    // 필요하면 여기서 stage 수행시간 로그
                    // Slogger.Write($"{stage.Name} done: {sw.ElapsedMilliseconds}ms");

                    accumulated = endPercent;
                    Report(progress, accumulated, $"{stage.Name} 완료");
                }

                Report(progress, 100, "완료");
                return context;
            }

            private static void ValidateStageWeights(List<IStartupStage> stages)
            {
                int sum = 0;
                foreach (var s in stages)
                    sum += s.StageWeightPercent;

                if (sum != 100)
                    throw new InvalidOperationException($"Startup stages weight sum must be 100. Current: {sum}");
            }
            private static int MapPercent(int start, int end, int percent)
            {
                if (percent < 0) percent = 0;
                if (percent > 100) percent = 100;

                double t = percent / 100.0;
                int mapped = start + (int)((end - start) * t);

                if (mapped < 0) mapped = 0;
                if (mapped > 100) mapped = 100;

                return mapped;
            }


            private static void Report(IProgress<InitProgress> progress, int percent, string message)
                => progress.Report(new InitProgress(Clamp(percent, 0, 100), message));

            private static int Clamp(int value, int min, int max)
            {
                if (value < min) return min;
                if (value > max) return max;
                return value;
            }
        }
    }
