using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV
{
    internal static class StartupContextFactory
    {
        public static StartupContext Create()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            string configPath = Path.Combine(basePath, "Config");
            string logPath = Path.Combine(basePath, "Log");
            string tempPath = Path.Combine(basePath, "Temp");

            return new StartupContext(basePath, configPath, logPath, tempPath);
        }
    }
}
