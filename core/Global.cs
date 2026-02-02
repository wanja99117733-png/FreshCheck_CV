using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreshCheck_CV.Seqeunce;

namespace FreshCheck_CV.Core
{
    //싱글톤을 이용하여, 전역적으로 쉽게 접근하는 클래스
    public class Global : IDisposable
    {
        #region Singleton Instance
        private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

        public static Global Inst
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion

        private InspStage _stage = new InspStage();
        private readonly Vision4SequenceRuntime _vision4Runtime = new Vision4SequenceRuntime();

        internal Vision4SequenceRuntime Vision4Runtime
        {
            get { return _vision4Runtime; }
        }

        public InspStage InspStage
        {
            get { return _stage; }
        }


        public Global()
        {
        }

        public bool Initialize()
        {
            return _stage.Initialize();
        }

        public void Dispose()
        {
            try { _vision4Runtime.Dispose(); } catch { }
            try { _stage.Dispose(); } catch { }
        }
    }
}
