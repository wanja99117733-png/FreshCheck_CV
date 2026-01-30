using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreshCheck_CV.Sequence;

namespace FreshCheck_CV.Core
{

    //싱글톤을 이용하여, 전역적으로 쉽게 접근하는 클래스
    public class Global : IDisposable
    {

        private readonly FcCommunicator _communicator = new FcCommunicator();
        public FcCommunicator Communicator { get { return _communicator; } }

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
            _stage.Dispose();
        }
    }
}
