using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
    public sealed class ResultRecord
    {
        public int No { get; set; }
        public DateTime Time { get; set; }

        public string Result { get; set; } = "OK";          // OK / NG
        public string DefectType { get; set; } = "None";    // None / Mold / Both ...
        public double Ratio { get; set; }                   // Mold ratio
        public string SavedPath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
