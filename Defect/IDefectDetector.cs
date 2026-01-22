using FreshCheck_CV.Defect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Defect
{
    public interface IDefectDetector
    {
        DefectType Type { get; }

        DefectResult Detect(Bitmap sourceBitmap);
    }
}
