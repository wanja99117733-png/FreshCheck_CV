using FreshCheck_CV.Core;
using FreshCheck_CV.Inspect;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreshCheck_CV.Scratch
{
    public partial class ScratchProp : UserControl
    {
        public ScratchProp()
        {
            InitializeComponent();
        }

        private void btnEraseBg_Click(object sender, EventArgs e)
        {
            SaigeAI saigeAI = Global.Inst.InspStage.AIModule; // SaigeAI 인스턴스

            AIEngineType engineType = AIEngineType.Segmentation;
            string modelPath = "C:\\project\\teamProject\\FreshCheck_CV\\inspect\\Cu_seg.saigeseg";
            saigeAI.LoadEngine(modelPath, engineType); // 엔진 연결

            if (saigeAI == null)
            {
                MessageBox.Show("AI 모듈이 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap bitmap = Global.Inst.InspStage.GetCurrentImage(); // 현재 이미지 가져오기
            if (bitmap is null)
            {
                MessageBox.Show("현재 이미지가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            saigeAI.InspAIModule(bitmap); // 검사

            Bitmap resultImage = saigeAI.GetResultImage(); // 배경 삭제된 이미지

            Global.Inst.InspStage.UpdatePreview(resultImage);


            /* 테스트 용도 - S */
            Bitmap curBitmap = Global.Inst.InspStage.GetCurrentImage();
            Bitmap previewBitmap = Global.Inst.InspStage.GetPreviewImage();

            Mat curMat = BitmapConverter.ToMat(curBitmap);
            Mat previewMat = BitmapConverter.ToMat(previewBitmap);

            Cv2.Resize(curMat, curMat, new OpenCvSharp.Size(0, 0), 0.2, 0.2);
            Cv2.Resize(previewMat, previewMat, new OpenCvSharp.Size(0, 0), 0.2, 0.2);

            //Cv2.ImShow("curMat", curMat);
            //Cv2.ImShow("previewMat", previewMat);
            /* 테스트 용도 - E */


        }
    }
}
