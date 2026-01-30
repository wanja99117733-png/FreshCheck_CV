using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FreshCheck_CV.Setting
{

    internal static class XmlHelper
    {
        public static T LoadXml<T>(string filePath) where T : class
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            if (File.Exists(filePath) == false)
                return null;

            try
            {
                using (var fs = File.OpenRead(filePath))
                {
                    var ser = new XmlSerializer(typeof(T));
                    return ser.Deserialize(fs) as T;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void SaveXml<T>(string filePath, T instance)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("filePath is null/empty.", nameof(filePath));

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir) == false)
                Directory.CreateDirectory(dir);

            using (var fs = File.Create(filePath))
            {
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(fs, instance);
            }
        }

        public static T XmlDeserialize<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
                return null;

            using (var sr = new StringReader(xml))
            {
                var ser = new XmlSerializer(typeof(T));
                return ser.Deserialize(sr) as T;
            }
        }

        public static string ToXmlContent<T>(this T instance)
        {
            if (instance == null)
                return string.Empty;

            var ser = new XmlSerializer(typeof(T));

            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                ser.Serialize(sw, instance);
                sw.Flush();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}