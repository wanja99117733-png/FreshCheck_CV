
using FreshCheck_CV.Core.FreshCheck_CV.Core.Models;
using FreshCheck_CV.Models;
using FreshCheck_CV.Models.FreshCheck_CV.Core.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace FreshCheck_CV.Core.Processing
{
    //이진화 처리 전용 클래스
    //입력(Bitmap) -> OpenCV 처리(Mat) -> 출력(Bitmap)
    public class BinaryProcessor
    {
        //이진화 적용
        public Bitmap Apply(Bitmap srcBitmap, BinaryOptions opt)
        {
            if (srcBitmap == null)
                return null;

            if (opt == null)
                opt = new BinaryOptions();

            // Bitmap -> Mat
            using (Mat src = BitmapConverter.ToMat(srcBitmap))
            {
                // 결과 마스크(0/255) Mat
                using (Mat mask = CreateBinaryMask(src, opt))
                {
                    // mask는 1채널(Gray)
                    // 그대로 Bitmap으로 변환하여 화면에 표시
                    Bitmap dst = BitmapConverter.ToBitmap(mask);
                    return dst;
                }
            }
        }

        // 옵션에 따라 이진화 마스크(Mat)를 생성합니다.
        // 반환 Mat은 8-bit 1채널(0/255) 마스크를 목표로 합니다.
        // </summary>
        private Mat CreateBinaryMask(Mat srcBgr, BinaryOptions opt)
        {
            // 반환용 Mat
            Mat mask = new Mat();

            switch (opt.Mode)
            {
                case BinaryMode.GrayThreshold:
                    mask = MakeGrayThreshold(srcBgr, opt.Threshold, opt.Invert);
                    break;

                case BinaryMode.Otsu:
                    mask = MakeOtsu(srcBgr, opt.Invert);
                    break;

                case BinaryMode.Adaptive:
                    mask = MakeAdaptive(srcBgr, opt.Invert, opt.AdaptiveBlockSize, opt.AdaptiveC);
                    break;

                case BinaryMode.RgbRange:
                    mask = MakeRgbRange(srcBgr, opt);
                    break;

                default:
                    // 기본은 GrayThreshold
                    mask = MakeGrayThreshold(srcBgr, opt.Threshold, opt.Invert);
                    break;
            }

            return mask;
        }

        #region Gray 계열 이진화

        private Mat MakeGrayThreshold(Mat srcBgr, int threshold, bool invert)
        {
            Mat gray = new Mat();
            if (srcBgr.Channels() == 1)
                gray = srcBgr.Clone();
            else
                Cv2.CvtColor(srcBgr, gray, ColorConversionCodes.BGR2GRAY);

            ThresholdTypes type = invert ? ThresholdTypes.BinaryInv : ThresholdTypes.Binary;

            Mat bin = new Mat();
            Cv2.Threshold(gray, bin, threshold, 255, type);

            gray.Dispose();
            return bin; // 1채널 0/255
        }

        private Mat MakeOtsu(Mat srcBgr, bool invert)
        {
            Mat gray = new Mat();
            if (srcBgr.Channels() == 1)
                gray = srcBgr.Clone();
            else
                Cv2.CvtColor(srcBgr, gray, ColorConversionCodes.BGR2GRAY);

            ThresholdTypes type = invert ? ThresholdTypes.BinaryInv : ThresholdTypes.Binary;
            type |= ThresholdTypes.Otsu; // Otsu 플래그 추가

            Mat bin = new Mat();
            Cv2.Threshold(gray, bin, 0, 255, type);

            gray.Dispose();
            return bin;
        }

        private Mat MakeAdaptive(Mat srcBgr, bool invert, int blockSize, int c)
        {
            // Adaptive는 blockSize가 반드시 홀수, 3 이상
            if (blockSize < 3) blockSize = 3;
            if (blockSize % 2 == 0) blockSize += 1;

            Mat gray = new Mat();
            if (srcBgr.Channels() == 1)
                gray = srcBgr.Clone();
            else
                Cv2.CvtColor(srcBgr, gray, ColorConversionCodes.BGR2GRAY);

            ThresholdTypes type = invert ? ThresholdTypes.BinaryInv : ThresholdTypes.Binary;

            Mat bin = new Mat();
            Cv2.AdaptiveThreshold(
                gray,
                bin,
                255,
                AdaptiveThresholdTypes.GaussianC,
                type,
                blockSize,
                c);

            gray.Dispose();
            return bin;
        }

        #endregion

        #region RGB 범위 이진화 (OpenCV는 BGR 순서)

        private Mat MakeRgbRange(Mat srcBgr, BinaryOptions opt)
        {
            Scalar lower = new Scalar(opt.MinB, opt.MinG, opt.MinR);
            Scalar upper = new Scalar(opt.MaxB, opt.MaxG, opt.MaxR);

            Mat mask = new Mat();
            Cv2.InRange(srcBgr, lower, upper, mask); // 0/255 마스크 생성

            // Invert 옵션을 색상 범위 마스크에도 적용하고 싶다면 아래처럼
            if (opt.Invert)
            {
                Cv2.BitwiseNot(mask, mask);
            }

            return mask;
        }

        #endregion
    }
}
