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
using FreshCheck_CV.Scratch;

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
            Image bitmap = Image.FromFile(filePath);
            imageViewCtrl.LoadBitmap((Bitmap)bitmap);
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



        public Bitmap GetPreviewImage()
        {
            return imageViewCtrl.PreviewImage;
        }

        public void UpdatePreview(Bitmap bitmap)
        {
            if (imageViewCtrl != null)
            {
                imageViewCtrl.PreviewImage = bitmap;
                imageViewCtrl.Invalidate(); // 다시 그려줌. OnPaint 함수 호출.
            }
        }

        public void ClearPreviewImage()
        {
            if (imageViewCtrl != null)
                imageViewCtrl.PreviewImage = null;
        }
    }
}
