using System;
using System.Drawing;

namespace FreshCheck_CV.Scratch
{
    // 스포이드로 찍은 컬러 룰 1개(포함/제외)
    public class CucumberColorRule
    {
        public CucumberColorRule(System.Drawing.Point seedPoint, System.Drawing.Color seedColor)
        {
            SeedPoint = seedPoint;
            SeedColor = seedColor;

            // 기본 허용오차(현장 튜닝 가능)
            HueTolerance = 12;
            SatTolerance = 45;
            ValTolerance = 45;

            IsExclude = false;
        }

        public System.Drawing.Point SeedPoint { get; }

        public System.Drawing.Color SeedColor { get; }

        // HSV 기준 허용오차 (OpenCV H:0~179)
        public int HueTolerance { get; set; }

        public int SatTolerance { get; set; }

        public int ValTolerance { get; set; }

        // true면 제외 마스크로 취급
        public bool IsExclude { get; set; }

        public override string ToString()
        {
            return $"[{(IsExclude ? "EXC" : "INC")}] RGB({SeedColor.R},{SeedColor.G},{SeedColor.B}) @ ({SeedPoint.X},{SeedPoint.Y})";
        }
    }
}
