using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
    public enum MonitorDefectType
    {
        None = 0,
        Mold = 1,
        Scratch = 2,
        Both = 3,
    }

    public sealed class InspectionResultDto
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsOk { get; set; }
        public MonitorDefectType Type { get; set; } = MonitorDefectType.None;
        public double? Ratio { get; set; }
        public string SavedPath { get; set; }
        public string Message { get; set; }
    }

    // ★ OK/Mold/Scratch 3개 라인용
    public struct TrendPoint
    {
        public int X;                 // 1..최근50개
        public double OkRate;         // 0~1
        public double MoldRate;       // 0~1
        public double ScratchRate;    // 0~1
    }

    public sealed class InspectionSnapshot
    {
        public bool IsRunning { get; set; }
        public double ItemsPerMin { get; set; }

        // 누적
        public int Total { get; set; }
        public int Ok { get; set; }
        public int Mold { get; set; }
        public int Scratch { get; set; }
        public int Both { get; set; }

        // 최근 N개(=50) 기준
        public int RecentCount { get; set; }
        public double RecentOkRate { get; set; }
        public double RecentMoldRate { get; set; }
        public double RecentScratchRate { get; set; }

        // 차트 포인트(최근 50개 기준, 이동평균 적용)
        public TrendPoint[] TrendPoints { get; set; } = Array.Empty<TrendPoint>();

        public string AlertText { get; set; }
    }
}
