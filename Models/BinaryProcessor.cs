using FreshCheck_CV.Models.FreshCheck_CV.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
    public static class BinaryProcessor
    {
        public static Bitmap ApplyBinaryOnly(Bitmap sourceBitmap, BinaryOptions options)
        {
            if (sourceBitmap is null) throw new ArgumentNullException(nameof(sourceBitmap));
            if (options is null) throw new ArgumentNullException(nameof(options));

            options.Validate();

            using (Mat src = BitmapConverter.ToMat(sourceBitmap))
            using (Mat bgr = EnsureBgr(src))
            using (Mat mask = CreateBgrMask(bgr, options))
            using (Mat maskBgr = new Mat())
            {
                // BinaryOnly: 마스크를 3채널로 바꿔서 화면 표시용 Bitmap 생성
                Cv2.CvtColor(mask, maskBgr, ColorConversionCodes.GRAY2BGR);
                return BitmapConverter.ToBitmap(maskBgr);
            }
        }

        private static Mat CreateBgrMask(Mat bgr, BinaryOptions options)
        {
            options.GetLowerUpper(out Scalar lower, out Scalar upper);

            Mat mask = new Mat();
            Cv2.InRange(bgr, lower, upper, mask);

            if (options.Invert)
            {
                Cv2.BitwiseNot(mask, mask);
            }

            return mask;
        }

        private static Mat EnsureBgr(Mat src)
        {
            if (src is null) throw new ArgumentNullException(nameof(src));

            int channels = src.Channels();

            // 이미 3채널이면 그대로 복사
            if (channels == 3)
            {
                return src.Clone();
            }

            // 1채널(Gray) -> BGR
            if (channels == 1)
            {
                Mat bgr = new Mat();
                Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
                return bgr;
            }

            // 4채널(BGRA) -> BGR (알파 제거)
            if (channels == 4)
            {
                Mat bgr = new Mat();
                Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
                return bgr;
            }

            throw new NotSupportedException($"지원하지 않는 채널 수입니다. Channels={channels}, Type={src.Type()}");
        }

        public static Bitmap ApplyPreview(Bitmap sourceBitmap, BinaryOptions options)
        {
            if (sourceBitmap is null) throw new ArgumentNullException(nameof(sourceBitmap));
            if (options is null) throw new ArgumentNullException(nameof(options));

            options.Validate();

            if (options.ShowMode == ShowBinaryMode.None)
            {
                return new Bitmap(sourceBitmap);
            }

            using (Mat src = BitmapConverter.ToMat(sourceBitmap))
            using (Mat bgr = EnsureBgr(src))
            using (Mat mask = CreateBgrMask(bgr, options))
            {
                if (options.ShowMode == ShowBinaryMode.BinaryOnly)
                {
                    using (Mat maskBgr = new Mat())
                    {
                        Cv2.CvtColor(mask, maskBgr, ColorConversionCodes.GRAY2BGR);
                        return BitmapConverter.ToBitmap(maskBgr);
                    }
                }

                Scalar color = GetHighlightColor(options.ShowMode);

                using (Mat baseImg = bgr.Clone())
                using (Mat overlay = bgr.Clone())
                using (Mat result = new Mat())
                {
                    overlay.SetTo(color, mask);
                    Cv2.AddWeighted(baseImg, 0.7, overlay, 0.3, 0, result);
                    return BitmapConverter.ToBitmap(result);
                }
            }
        }

        private static Scalar GetHighlightColor(ShowBinaryMode mode)
        {
            switch (mode)
            {
                case ShowBinaryMode.HighlightRed:
                    return new Scalar(0, 0, 255);
                case ShowBinaryMode.HighlightGreen:
                    return new Scalar(0, 255, 0);
                case ShowBinaryMode.HighlightBlue:
                    return new Scalar(255, 0, 0);
                default:
                    return new Scalar(0, 0, 255);
            }
        }

    }
}