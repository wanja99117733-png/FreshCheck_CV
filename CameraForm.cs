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
            Bitmap curBitmap = Global.Inst.InspStage.GetCurrentImage();

            // Bitmap 이미지를 Mat 타입으로 변경
            Mat curMat = BitmapConverter.ToMat(curBitmap);


            Mat hsv = new Mat();
            Cv2.CvtColor(curMat, hsv, ColorConversionCodes.BGR2HSV);

            // HSV 데이터 변경
            Scalar lower = new Scalar(35, 40, 40);
            Scalar upper = new Scalar(85, 255, 255);

            // 컬러로 마스킹
            Mat mask = new Mat();
            Cv2.InRange(hsv, lower, upper, mask);

            // Morphology함수로 노이즈 제거
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(7, 7));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);

            Bitmap newBitmap = BitmapConverter.ToBitmap(mask);
            UpdateDisplay(newBitmap);
        }
    }
}
