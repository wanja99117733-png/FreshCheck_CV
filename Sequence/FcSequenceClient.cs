using FreshCheck_CV.Util;
using MessagingLibrary;
using System;
using System.ServiceModel;
using FreshCheck_CV.Xml;

namespace FreshCheck_CV.Sequence
{
    /// <summary>
    /// FC용: 연결/핸드셰이크/검사결과 전송(InspDone)
    /// </summary>
    public sealed class FcSequenceClient : IDisposable
    {
        private readonly Communicator _comm = new Communicator();

        public bool IsConnected => _comm.State == CommunicationState.Opened;

        public void Initialize()
        {
            // 설정 로드(Setup\Setting.xml)
            SettingXml.Load();

            _comm.Opened += (s, e) =>
            {
                SLogger.Write($"[WCF] Opened. ip={SettingXml.Inst.CommIP}", SLogger.LogType.Info);

                // 연결되면 즉시 핸드셰이크
                _comm.SendMachineInfo();
                SLogger.Write($"[WCF] HandShake sent. machine={SettingXml.Inst.MachineName}", SLogger.LogType.Info);
            };

            _comm.Closed += (s, e) =>
            {
                SLogger.Write("[WCF] Closed.", SLogger.LogType.Error);
            };

            _comm.ReceiveMessage += (s, msg) =>
            {
                // 필요 시 서버 명령 처리
                // 예: msg.Command == Loaded 등
            };

            // 생성 + 연결
            _comm.Create(SettingXml.Inst.CommType, SettingXml.Inst.CommIP);
        }

        /// <summary>
        /// Jidam과 동일한 Command(InspDone)로 OK/NG 통지
        /// (추가 정보는 Tool/ErrorMessage에 문자열로 담아 전송)
        /// </summary>
        public void SendInspDone(long inspectionNo, bool isNg, string ngDetail)
        {
            if (!IsConnected)
            {
                SLogger.Write("[WCF] Not connected. Skip SendInspDone.", SLogger.LogType.Error);
                return;
            }

            var content = new Message
            {
                Command = Message.MessageCommand.InspDone,
                MachineName = SettingXml.Inst.MachineName,
                Status = isNg ? Message.CommandStatus.Ng : Message.CommandStatus.Good,

                // 서버가 필드를 얼마나 쓰는지 몰라서 안전하게 문자열로만 전달
                Tool = $"inspNo={inspectionNo}",
                ErrorMessage = isNg ? ngDetail : string.Empty
            };

            var info = new MmiMessageInfo
            {
                Message = content.ToXmlContent()
            };

            bool ok = _comm.SendMessage(info);
            SLogger.Write($"[WCF] Send InspDone: inspNo={inspectionNo}, isNg={isNg}, ok={ok}, detail={ngDetail}", SLogger.LogType.Info);
        }

        public void Dispose()
        {
            try
            {
                _comm.Close();
            }
            catch
            {
                // ignore
            }
        }
    }
}
