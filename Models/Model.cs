using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using OpenCvSharp;
using Common.Util.Helpers;

namespace FreshCheck_CV.Models
{
    [Serializable]
    public class Model
    {
        // 1. 모델 기본 정보
        public string ModelName { get; set; } = "";
        public string ModelInfo { get; set; } = "";

        [XmlIgnore] // 경로는 파일 자체에 저장할 필요 없으므로 제외 (런타임용)
        public string ModelPath { get; set; } = "";

        // 2. 핵심 설정: BinaryOptions (이것이 XML에 같이 저장됩니다)
        public BinaryOptions BinaryConfig { get; set; } = new BinaryOptions();

        // 3. 이미지 관련 정보
        public string MasterImageName { get; set; } = "MasterImage.png";

        public Model() { }

        /// <summary>
        /// 모델 파일(.xml) 및 관련 리소스를 저장합니다.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(ModelPath))
            {
                // 경로가 없으면 실행 안 함 (혹은 SaveFileDialog 호출 로직 필요)
                return;
            }

            try
            {
                // A. 모델 폴더 생성 (ModelPath가 파일 경로인 경우 그 상위 폴더 생성)
                string directory = Path.GetDirectoryName(ModelPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // B. XML 데이터 저장 (Model 정보 + BinaryOptions)
                // XmlHelper는 기존 프로젝트의 유틸리티를 그대로 사용한다고 가정합니다.
                XmlHelper.SaveXml(ModelPath, this);

                // C. 관련 이미지 저장 (필요 시)
                SaveResources(directory);

                System.Diagnostics.Debug.WriteLine($"모델 저장 완료: {ModelPath}");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"모델 저장 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 모델에 부수되는 이미지 리소스 등을 저장합니다.
        /// </summary>
        private void SaveResources(string rootPath)
        {
            string imgDir = Path.Combine(rootPath, "Images");
            if (!Directory.Exists(imgDir))
            {
                Directory.CreateDirectory(imgDir);
            }

            // 예: 현재 화면에 떠있는 원본 이미지를 마스터 이미지로 저장하고 싶을 때
            // InspStage나 Global에서 현재 이미지를 가져오는 로직이 필요합니다.
            // 여기서는 예시로 설명만 추가합니다.
            /*
            Bitmap currentImg = Global.Inst.InspStage.GetCurrentImage();
            if (currentImg != null)
            {
                string targetPath = Path.Combine(imgDir, MasterImageName);
                currentImg.Save(targetPath, System.Drawing.Imaging.ImageFormat.Png);
            }
            */
        }
    }
}