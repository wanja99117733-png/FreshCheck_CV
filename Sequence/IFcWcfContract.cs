using System.ServiceModel;

namespace FreshCheck_CV.Sequence
{
    // 서버 -> FC 콜백
    public interface IFcDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnServerCommand(FcServerCommand cmd);
    }

    // FC -> 서버 서비스
    [ServiceContract(CallbackContract = typeof(IFcDuplexCallback), SessionMode = SessionMode.Required)]
    public interface IFcDuplexService
    {
        [OperationContract]
        FcAck HandShake(FcHandShakeRequest req);

        [OperationContract]
        FcAck SendInspectionResult(FcInspectionResult result);

        [OperationContract(IsOneWay = true)]
        void Heartbeat(FcHeartbeat hb);
    }
}
