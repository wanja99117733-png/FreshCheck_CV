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

        private readonly FcCommunicator _communicator = new FcCommunicator();
        public FcCommunicator Communicator { get { return _communicator; } }

        public FcSequenceClient SequenceClient { get; private set; }


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
            bool ok = _stage.Initialize();
            if (!ok)
                return false;

            try
            {
                SequenceClient = new FcSequenceClient();
                SequenceClient.Initialize();
            }
            catch (Exception ex)
            {
                Util.SLogger.Write($"[WCF] Init error: {ex.Message}", Util.SLogger.LogType.Error);
                // 통신 실패해도 검사는 돌 수 있게 false 반환은 안 함
            }

            return true;
        }
        

        public void Dispose()
        {
            SequenceClient?.Dispose();
            _stage.Dispose();
        }
    }
}
