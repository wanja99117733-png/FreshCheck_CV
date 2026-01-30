using FreshCheck_CV.Models;
using FreshCheck_CV.Util;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Defect
{
    public sealed class MoldDetector : IDefectDetector
    {
        public long ElapsedMs { get; set; }

        private readonly Func<BinaryOptions> _getBinaryOptions;

        // 흰 픽셀 비율 임계값 (기본 1%)
        public double AreaRatioThreshold { get; set; } = 0.001;

        public DefectType Type => DefectType.Mold;

        //과탐 많을 경우 값 올리고, 미탐이 많을 경우 값 내리기 -> KBS
        public double TextureEdgeThresh { get; set; } = 24;

        //곰팡이가 점처럼 끊겨서 비율이 너무 줄면 값 올리기 -> KBS
        public int TextureBlurK { get; set; } = 3;

        //노이즈가 많이 살아남으면 값 올리기 -> KBS
        public int TextureDilateK { get; set; } = 3;

        public MoldDetector(Func<BinaryOptions> getBinaryOptions)
        {
            _getBinaryOptions = getBinaryOptions ?? throw new ArgumentNullException(nameof(getBinaryOptions));
        }

        public DefectResult Detect(Bitmap sourceBitmap)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            // 스탑워치
            var sw = System.Diagnostics.Stopwatch.StartNew();

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

                Bitmap overlayBmp = null;

                if (isDefect)
                {
                    using (Mat baseImg = bgr.Clone())
                    using (Mat overlay = bgr.Clone())
                    using (Mat result = new Mat())
                    {
                        Scalar color = new Scalar(0, 0, 255); // 빨강
                        overlay.SetTo(color, mask);

                        Cv2.AddWeighted(baseImg, 0.7, overlay, 0.3, 0, result);
                        overlayBmp = BitmapConverter.ToBitmap(result);
                    }
                }

                sw.Stop();



                return new DefectResult
                {
                    Type = isDefect ? DefectType.Mold : DefectType.OK,
                    IsDefect = isDefect,
                    AreaRatio = ratio,
                    Message = $"Mold ratio={ratio:0.0000}, threshold={AreaRatioThreshold:0.0000}",
                    OverlayBitmap = overlayBmp,
                    ElapsedMs = sw.ElapsedMilliseconds
                };


            }
        }




        public DefectResult Detect(Bitmap sourceBitmap, Bitmap originalBitmap)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            // 스탑워치
            var sw = System.Diagnostics.Stopwatch.StartNew();

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

                using (Mat textureMask = BuildTextureMask(bgr))
                {
                    Cv2.BitwiseAnd(mask, textureMask, mask); // ✅ "흰색 후보" AND "텍스처"
                }

                int whitePixels = Cv2.CountNonZero(mask);
                int totalPixels = mask.Rows * mask.Cols;
                double ratio = totalPixels <= 0 ? 0.0 : (double)whitePixels / totalPixels;

                bool isDefect = ratio >= AreaRatioThreshold;

                Bitmap overlayBmp = null;

                if (isDefect)
                {
                    using (Mat originalSrc = BitmapConverter.ToMat(originalBitmap))
                    using (Mat originalBgr = EnsureBgr(originalSrc))
                    using (Mat baseImg = originalBgr.Clone())
                    using (Mat overlay = originalBgr.Clone())
                    using (Mat result = new Mat())
                    {
                        Scalar color = new Scalar(0, 0, 255); // 빨강
                        overlay.SetTo(color, mask);

                        Cv2.AddWeighted(baseImg, 0.7, overlay, 0.3, 0, result);
                        overlayBmp = BitmapConverter.ToBitmap(result);
                    }
                }

                sw.Stop();

                return new DefectResult
                {
                    Type = isDefect ? DefectType.Mold : DefectType.OK,
                    IsDefect = isDefect,
                    AreaRatio = ratio,
                    Message = $"Mold ratio={ratio:0.0000}, threshold={AreaRatioThreshold:0.0000}",
                    OverlayBitmap = overlayBmp,
                    ElapsedMs = sw.ElapsedMilliseconds
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

        private Mat BuildTextureMask(Mat bgr)
        {
            using (Mat gray = new Mat())
            {
                Cv2.CvtColor(bgr, gray, ColorConversionCodes.BGR2GRAY);

                int k = Math.Max(1, TextureBlurK);
                if (k % 2 == 0) k += 1;

                // blurMat은 상황에 따라 생성/해제
                Mat blurMat = null;

                try
                {
                    if (k > 1)
                    {
                        blurMat = new Mat();
                        Cv2.GaussianBlur(gray, blurMat, new OpenCvSharp.Size(k, k), 0);
                    }
                    else
                    {
                        // k==1이면 블러 없이 gray를 그대로 사용 (Clone 불필요)
                        blurMat = gray;
                    }

                    using (Mat lap16 = new Mat())
                    using (Mat lap8 = new Mat())
                    using (Mat edgeMask = new Mat())
                    {
                        Cv2.Laplacian(blurMat, lap16, MatType.CV_16S, ksize: 3);
                        Cv2.ConvertScaleAbs(lap16, lap8);

                        Cv2.Threshold(lap8, edgeMask, TextureEdgeThresh, 255, ThresholdTypes.Binary);

                        int dk = Math.Max(1, TextureDilateK);
                        if (dk % 2 == 0) dk += 1;

                        if (dk > 1)
                        {
                            using (Mat kDil = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(dk, dk)))
                            {
                                Cv2.Dilate(edgeMask, edgeMask, kDil);
                            }
                        }

                        return edgeMask.Clone();
                    }
                }
                finally
                {
                    // gray를 그대로 쓴 경우(gray 참조)면 Dispose하면 안 됨
                    if (blurMat != null && object.ReferenceEquals(blurMat, gray) == false)
                    {
                        blurMat.Dispose();
                    }
                }
            }
        }
    }
}