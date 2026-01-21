using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
    public sealed class BinaryOptions
    {
        public BinaryMode Mode { get; set; } = BinaryMode.GrayScale;

        /// <summary>0~255, B/G/R에 동일 적용</summary>
        public int MinValue { get; set; } = 0;

        /// <summary>0~255, B/G/R에 동일 적용</summary>
        public int MaxValue { get; set; } = 255;

        /// <summary>마스크 반전 여부</summary>
        public bool Invert { get; set; } = false;

        public void Validate()
        {
            if (MinValue < 0 || MinValue > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(MinValue));
            }

            if (MaxValue < 0 || MaxValue > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxValue));
            }

            if (MinValue > MaxValue)
            {
                throw new ArgumentException("MinValue는 MaxValue보다 클 수 없습니다.");
            }
        }
    }
}
