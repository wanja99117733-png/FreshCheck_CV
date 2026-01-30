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


        //신규 모델 생성
        public void CreateModel(string path, string modelName, string modelInfo)
        {
            ModelPath = path;
            ModelName = modelName;
            ModelInfo = modelInfo;
        }


        //모델 로딩함수
        public Model Load(string path)
        {
            Model model = XmlHelper.LoadXml<Model>(path);
            if (model == null)
                return null;

            ModelPath = path;

            return model;
        }

        public void Save()
        {
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

                System.Diagnostics.Debug.WriteLine($"모델 저장 완료: {ModelPath}");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"모델 저장 중 오류 발생: {ex.Message}");
            }
        }

        //모델 다른 이름으로 저장함수
        public void SaveAs(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (Directory.Exists(filePath) == false)
            {
                ModelPath = Path.Combine(filePath, fileName + ".xml");
                ModelName = fileName;
                Save();
            }
        }
    }
}