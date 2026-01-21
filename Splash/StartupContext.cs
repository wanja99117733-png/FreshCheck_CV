using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV
{
    internal sealed class StartupContext
    {
        public string AppBasePath { get; }
        public string ConfigPath { get; }
        public string LogPath { get; }
        public string TempPath { get; }

        // 설정/상태를 여기에 보관
        public AppConfig Config { get; set; }

        public StartupContext(string appBasePath, string configPath, string logPath, string tempPath)
        {
            AppBasePath = appBasePath ?? throw new ArgumentNullException(nameof(appBasePath));
            ConfigPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
            LogPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
            TempPath = tempPath ?? throw new ArgumentNullException(nameof(tempPath));
        }
    }

    internal sealed class AppConfig
    {
        // FC에 맞게 확장
        public string LastImageFolder { get; set; }
        public bool UseCameraOnStartup { get; set; } = true;
    }
}
