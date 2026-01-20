using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Core
{
    namespace FreshCheck_CV.Core.Models
    {
        public enum BinaryMode
        {
            // 일반 흑백 이진화(고정 임계값)
            GrayThreshold = 0,
            // Otsu 자동 임계값 이진화
            Otsu = 1,
            // Adaptive(적응형) 이진화
            Adaptive = 2,
            // RGB(BGR) 색상 범위로 마스크 생성
            RgbRange = 3
        }
    }

}
