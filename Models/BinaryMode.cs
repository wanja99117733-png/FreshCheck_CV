using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Models
{
    public enum BinaryMode : int
    {
        GrayScale = 0
    }

    public enum ShowBinaryMode : int
    {
        None = 0,
        HighlightRed,
        HighlightGreen,
        HighlightBlue,
        BinaryOnly
    }
}
