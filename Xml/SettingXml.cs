using FreshCheck_CV.Sequence;
using System;
using System.IO;

namespace FreshCheck_CV.Setting
{

    /// <summary>
    /// 통신/장비 설정을 XML로 저장/로드합니다. (Jidam 방식)
    /// </summary>
    public sealed class SettingXml
    {
        private const string SettingDir = "Setup";
        private const string SettingFileName = @"Setup\Setting.xml";

        private static SettingXml _setting;

        public static SettingXml Inst
        {
            get
            {
                if (_setting == null)
                {
                    Load();
                }

                return _setting;
            }
        }

        public static void Load()
        {
            if (_setting != null)
                return;

            string path = Path.Combine(Environment.CurrentDirectory, SettingFileName);

            if (File.Exists(path))
            {
                _setting = XmlHelper.LoadXml<SettingXml>(path);
            }

            if (_setting == null)
            {
                _setting = CreateDefaultInstance();
                Save(); // 최초 생성 시 저장까지
            }
        }

        public static void Save()
        {
            string path = Path.Combine(Environment.CurrentDirectory, SettingFileName);

            if (!File.Exists(path))
            {
                string dir = Path.Combine(Environment.CurrentDirectory, SettingDir);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var fs = File.Create(path)) { }
            }

            XmlHelper.SaveXml(path, Inst);
        }

        private static SettingXml CreateDefaultInstance()
        {
            return new SettingXml
            {
                // ⚠️ 서버 IP로 바꾸세요
                CommIP = "192.168.1.85",

                // ⚠️ 교수님 서버에서 허용한 이름으로 맞춰야 함 (Jidam 주석에 VISION01~)
                MachineName = "VISION2",

                CommType = CommunicatorType.WCF
            };
        }

        public string MachineName { get; set; } = "VISION2";
        public CommunicatorType CommType { get; set; } = CommunicatorType.WCF;
        public string CommIP { get; set; } = "192.168.1.85";
    }
}