using FreshCheck_CV.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Defect
{
    public sealed class MoldDetector : IDefectDetector
    {
        private readonly Func<BinaryOptions> _getBinaryOptions;

        // 흰 픽셀 비율 임계값 (기본 1%)
        public double AreaRatioThreshold { get; set; } = 0.01;

        public DefectType Type => DefectType.Mold;

        public MoldDetector(Func<BinaryOptions> getBinaryOptions)
        {
            _getBinaryOptions = getBinaryOptions ?? throw new ArgumentNullException(nameof(getBinaryOptions));
        }

        public DefectResult Detect(Bitmap sourceBitmap)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            BinaryOptions options = _getBinaryOptions.Invoke() ?? new BinaryOptions();
            options.Validate();

            options.GetLowerUpper(out Scalar lower, out Scalar upper);

            using (Mat src = BitmapConverter.ToMat(sourceBitmap))
            using (Mat bgr = EnsureBgr(src))
            using (Mat mask = new Mat())
            {
                Cv2.InRange(bgr, lower, upper, mask);

                if (options.Invert)
                {
                    Cv2.BitwiseNot(mask, mask);
                }

                int whitePixels = Cv2.CountNonZero(mask);
                int totalPixels = mask.Rows * mask.Cols;
                double ratio = totalPixels <= 0 ? 0.0 : (double)whitePixels / totalPixels;

                bool isDefect = ratio >= AreaRatioThreshold;

                return new DefectResult
                {
                    Type = isDefect ? DefectType.Mold : DefectType.None,
                    IsDefect = isDefect,
                    Message = $"Mold ratio={ratio:0.0000}, threshold={AreaRatioThreshold:0.0000}"
                };
            }
        }

        private static Mat EnsureBgr(Mat src)
        {
            int channels = src.Channels();

            if (channels == 3)
                return src.Clone();

            if (channels == 1)
            {
                Mat bgr = new Mat();
                Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
                return bgr;
            }

            if (channels == 4)
            {
                Mat bgr = new Mat();
                Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
                return bgr;
            }

            throw new NotSupportedException($"지원하지 않는 채널 수입니다. Channels={channels}, Type={src.Type()}");
        }
    }
}
