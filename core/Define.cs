using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Core
{

    //#17_WORKING_STATE#1 작업 상태 정의
    public enum WorkingState
    {
        NONE = 0,
        INSPECT,
        LIVE,
        CYCLE,
        ALARM
    }

    public static class Define
    {
        public const string PROGRAM_NAME = "FreshCheck_CV";
    }
}
