using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreshCheck_CV.Core;
using WeifenLuo.WinFormsUI.Docking;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FreshCheck_CV
{
    public partial class CameraForm : DockContent
    {
        public CameraForm()
        {
            InitializeComponent();
        }

        //#3_CAMERAVIEW_PROPERTY#1 이미지 경로를 받아 PictureBox에 이미지를 로드하는 메서드
        public void LoadImage(string filePath)
        {
            if (File.Exists(filePath) == false)
                return;

            //#4_IMAGE_VIEWER#6 이미지 뷰어 컨트롤을 사용하여 이미지를 로드
            //picMainview.Image = Image.FromFile(filePath);
            Image bitmap = Image.FromFile(filePath);
            imageViewCtrl.LoadBitmap((Bitmap)bitmap);

            RemoveImageBg();
        }

        private void CameraForm_Resize_1(object sender, EventArgs e)
        {
            int margin = 0;
            imageViewCtrl.Width = this.Width - margin * 2;
            imageViewCtrl.Height = this.Height - margin * 2;

            imageViewCtrl.Location = new System.Drawing.Point(margin, margin);
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (imageViewCtrl != null)
                imageViewCtrl.LoadBitmap(bitmap);
        }

        public Bitmap GetDisplayImage()
        {
            Bitmap curImage = null;

            if (imageViewCtrl != null)
                curImage = imageViewCtrl.GetCurBitmap();

            return curImage;
        }

        // 작물과 배경 분리하여 배경 제거하는 함수
        public void RemoveImageBg()
        {
            Cv2.DestroyAllWindows();

            Bitmap curBitmap = Global.Inst.InspStage.GetCurrentImage();

            // Bitmap 이미지를 Mat 타입으로 변경
            Mat curMat = BitmapConverter.ToMat(curBitmap);
            Cv2.Resize(curMat, curMat, new OpenCvSharp.Size(0, 0), 0.2, 0.2);


            // HSV로 변환
            /* 이유:
            밝은 초록(B=70, G=180, R=70), 어두운 초록(B=40, G=90, R=40)
            이런 식으로 색상과 밝기가 나뉘어지지 않아 다른 색이 들어가버리는 오류가 발생할 수도 있음.
            따라서 색상, 채도, 밝기로 나뉘어지는 HSV를 사용.
            */
            Mat hsv = new Mat();
            Cv2.CvtColor(curMat, hsv, ColorConversionCodes.BGR2HSV);

            // mGreen : 초록 오이
            Mat mGreen = new Mat();
            Cv2.InRange(hsv, new Scalar(18, 25, 25), new Scalar(95, 255, 255), mGreen);

            // mPale : 흰 오이 후보
            Mat mPale = new Mat();
            Cv2.InRange(hsv, new Scalar(0, 0, 80), new Scalar(179, 80, 255), mPale);

            // 초록 마스크를 크게 팽창시켜 "오이 주변 영역"을 만들고,
            // 그 영역 내부에서만 mPale를 인정합니다.
            Mat seed = mGreen.Clone();

            // 씨앗이 너무 점처럼만 잡히면 (어두운 이미지 등) seed가 빈약해질 수 있으니
            // 작은 Close로 씨앗을 조금 보강
            Mat kSeed = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(seed, seed, MorphTypes.Close, kSeed, iterations: 1);

            // 초록 주변 허용 영역(팽창). 커질수록 흰 오이 연결이 쉬워지지만 바닥 유입 위험도 증가
            Mat kDilate = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(13, 13));
            Mat greenDilated = new Mat();
            Cv2.Dilate(seed, greenDilated, kDilate);

            Mat mPaleFiltered = new Mat();
            Cv2.BitwiseAnd(mPale, greenDilated, mPaleFiltered);

            // 최종 마스크
            Mat mask = new Mat();
            Cv2.BitwiseOr(mGreen, mPaleFiltered, mask);

            // 노이즈 제거
            Mat k = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, k, iterations: 1);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, k, iterations: 2);

            // 오이만 남기기
            Cv2.FindContours(mask, out OpenCvSharp.Point[][] contours, out _,
                RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            Mat finalMask = Mat.Zeros(mask.Size(), MatType.CV_8UC1);

            int width = mask.Width;
            int height = mask.Height;

            foreach (var c in contours)
            {
                double area = Cv2.ContourArea(c);
                if (area < 800) // 너무 작은 노이즈 제거
                    continue;

                Rect r = Cv2.BoundingRect(c);

                // 큰 덩어리면서 가장자리 접촉하면 제외(바닥 제외하기 위함)
                bool touchesBorder =
                    r.X <= 2 || r.Y <= 2 || r.Right >= width - 2 || r.Bottom >= height - 2;

                if (touchesBorder && area > (width * height * 0.30)) // 화면의 10% 이상 + 가장자리 접촉이면 제외
                    continue;

                // 통과한 컨투어는 마스크에 누적
                Cv2.DrawContours(finalMask, new[] { c }, -1, Scalar.White, thickness: -1);
            }

            Mat result = new Mat();
            curMat.CopyTo(result, finalMask); // 컬러 이미지에서 마지막으로 마스킹 처리한 이미지에 현재 이미지를 덧씌워서 result로 저장함.

            Cv2.ImShow("curMat", curMat);
            Cv2.ImShow("hsv", hsv);
            Cv2.ImShow("mGreen", mGreen);
            Cv2.ImShow("mPale", mPale);
            Cv2.ImShow("mask", mask);
            Cv2.ImShow("finalMask", finalMask);
            Cv2.ImShow("result_no_bg", result);
        }

        // HSV 컬러를 확인하기 위한 캔버스를 띄우는 함수. OpenCV의 HSV는 Scalar를 사용하기 때문에 일반적인 HSV의 값과 다름.
        private void ShowHSVColorCanvas(Scalar hsvColor, string title = "canvas")
        {
            Mat canvas = new Mat(200, 200, MatType.CV_8UC3, hsvColor);
            Cv2.ImShow(title, canvas);
        }
    }
}
