using FreshCheck_CV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FreshCheck_CV.Core
{
    public sealed class InspectionHub
    {
        private readonly object _sync = new object();

        private int _total;
        private int _ok;
        private int _mold;
        private int _scratch;
        private int _both;

        private bool _isRunning;

        // 최근 50개 기반
        private const int RecentN = 50;
        private const int TrendWindow = 10;
        private readonly Queue<InspectionResultDto> _lastN = new Queue<InspectionResultDto>(RecentN);

        // 속도 계산용
        private readonly Queue<DateTime> _speed = new Queue<DateTime>();
        private const int SpeedWindowMax = 120;

        public void SetRunning(bool isRunning)
        {
            lock (_sync)
            {
                _isRunning = isRunning;
            }
        }

        public void Push(InspectionResultDto r)
        {
            if (r == null) return;

            lock (_sync)
            {
                // 누적
                _total++;

                if (r.IsOk) _ok++;
                else
                {
                    if (r.Type == MonitorDefectType.Mold) _mold++;
                    else if (r.Type == MonitorDefectType.Scratch) _scratch++;
                    else if (r.Type == MonitorDefectType.Both) _both++;
                }

                // 최근 50개 큐
                _lastN.Enqueue(r);
                while (_lastN.Count > RecentN)
                    _lastN.Dequeue();

                // 속도 큐
                _speed.Enqueue(r.Timestamp);
                while (_speed.Count > SpeedWindowMax)
                    _speed.Dequeue();
            }
        }

        public InspectionSnapshot GetSnapshot()
        {
            lock (_sync)
            {
                return BuildSnapshot_NoLock();
            }
        }

        private InspectionSnapshot BuildSnapshot_NoLock()
        {
            var arr = _lastN.ToArray();
            int recentCount = arr.Length;

            int recentOk = arr.Count(x => x.IsOk);
            int recentMold = arr.Count(x => !x.IsOk && x.Type == MonitorDefectType.Mold);
            int recentScratch = arr.Count(x => !x.IsOk && x.Type == MonitorDefectType.Scratch);

            double okRate = recentCount > 0 ? (double)recentOk / recentCount : 0.0;
            double moldRate = recentCount > 0 ? (double)recentMold / recentCount : 0.0;
            double scratchRate = recentCount > 0 ? (double)recentScratch / recentCount : 0.0;

            // Items/min
            double itemsPerMin = 0.0;
            if (_speed.Count >= 2)
            {
                var first = _speed.Peek();
                var last = _speed.Last();
                var sec = Math.Max(1.0, (last - first).TotalSeconds);
                itemsPerMin = (_speed.Count / sec) * 60.0;
            }

            // 경고(최근 10개 중 NG 7개 이상)
            string alert = null;
            if (arr.Length >= 10)
            {
                var last10 = arr.Skip(arr.Length - 10).ToArray();
                int ng10 = last10.Count(x => !x.IsOk);
                if (ng10 >= 7)
                    alert = $"최근 10개 중 NG {ng10}회";
            }

            // 차트 포인트 생성: 최근 50개 기준, 이동평균(10개)으로 OK/Mold/Scratch 라인
            var points = BuildTrendPoints(arr, TrendWindow);

            return new InspectionSnapshot
            {
                IsRunning = _isRunning,
                ItemsPerMin = itemsPerMin,

                Total = _total,
                Ok = _ok,
                Mold = _mold,
                Scratch = _scratch,
                Both = _both,

                RecentCount = recentCount,
                RecentOkRate = okRate,
                RecentMoldRate = moldRate,
                RecentScratchRate = scratchRate,

                TrendPoints = points,
                AlertText = alert
            };
        }

        private static TrendPoint[] BuildTrendPoints(InspectionResultDto[] arr, int window)
        {
            if (arr == null || arr.Length == 0)
                return Array.Empty<TrendPoint>();

            if (window < 1) window = 1;

            var pts = new TrendPoint[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                int start = Math.Max(0, i - window + 1);
                int count = i - start + 1;

                int ok = 0;
                int mold = 0;
                int scratch = 0;

                for (int k = start; k <= i; k++)
                {
                    var r = arr[k];

                    if (r.IsOk) ok++;
                    else
                    {
                        if (r.Type == MonitorDefectType.Mold) mold++;
                        else if (r.Type == MonitorDefectType.Scratch) scratch++;
                        else if (r.Type == MonitorDefectType.Both)
                        {
                            //
                        }
                    }
                }

                pts[i] = new TrendPoint
                {
                    X = i + 1,
                    OkRate = count > 0 ? (double)ok / count : 0.0,
                    MoldRate = count > 0 ? (double)mold / count : 0.0,
                    ScratchRate = count > 0 ? (double)scratch / count : 0.0
                };
            }

            return pts;
        }
    }
}