using Common.Util.Helpers;
using FreshCheck_CV.Setting;
using FreshCheck_CV.Util;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using MessagingLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Sequence
{
    public enum CommunicatorType
    {
        None,
        WCF
    }

    public delegate bool CallBack(object sender, IMessageDuplexCallback channel,
        MmiMessageInfo message, ClientInfo clinet);
    public class Communicator
    {
        public event EventHandler Closed = delegate { };
        public event EventHandler Opened = delegate { };
        public event EventHandler<Message> ReceiveMessage = delegate { };

        private Message _currentMessage;
        private IMessageDuplexClient _clinet;

        public CommunicationState State
        {
            get
            {
                if (_clinet == null)
                    return CommunicationState.Faulted;
                else
                    return _clinet.State;
            }
        }

        public void Create(CommunicatorType type, params object[] parameters)
        {
            switch (type)
            {
                case CommunicatorType.None:
                    _clinet = new MessageDuplexEmptyClient();
                    break;
                case CommunicatorType.WCF:
                    {
                        string raw = (parameters[0] as string) ?? "";
                        raw = raw.Trim();

                        // 로그로 실제 전달값 확인
                        SLogger.Write($"[WCF] CommIP for client = '{raw}'");

                        // IPv4:port 형태면 라이브러리가 내부에서 포트를 붙일 가능성이 있으니 host만 전달(안전 우선)
                        // 例) "192.168.1.85:7900" -> "192.168.1.85"
                        string hostOnly = raw;
                        if (System.Text.RegularExpressions.Regex.IsMatch(raw, @"^\d{1,3}(\.\d{1,3}){3}:\d{1,5}$"))
                        {
                            hostOnly = raw.Split(':')[0].Trim();
                            SLogger.Write($"[WCF] CommIP looks like ip:port. Using hostOnly='{hostOnly}'");
                        }

                        _clinet = new MessageDuplexClient(BindingType.Tcp, hostOnly);
                        break;
                    }
            }

            _clinet.WcfReceivedMessage += Client_WcfReceivedMessage;
            _clinet.Closed += Client_Closed;
            _clinet.Opened += Client_Opened;
            _clinet.Connect();
        }

        public void Close()
        {
            if (_clinet != null)
            {
                _clinet.Close();
            }
        }

        public bool Connect()
        {
            if (_clinet is null)
                return false;

            return _clinet.Connect();
        }

        public bool SendMessage(MmiMessageInfo message)
        {
            if (_clinet == null)
                return false;
            var result = _clinet.SendMessage(new MmiMessageInfo() { Message = message.ToXmlContent() });
            if (result != CommunicationErrorType.None)
            {
                SLogger.Write($"SendMessage FAiled : {result}", SLogger.LogType.Error);
            }


            return true;
        }

        public void SendMachineInfo()
        {
            //#WCF_FSM#1 서버와 통신을 하기 위해서는, 약속된 클라이언트 여야함.
            //현재 정의된 클라이언트는 아래 중에서 하나를 선택하여 MachineName으로 설정해야함
            //VISION01
            //VISION02
            //VISION03
            //환경설정에서 SettingXml.Inst.MachineName에 값을 설정하도록 할것
            Message content = new Message
            {
                Command = Message.MessageCommand.HandShake,
                MachineName = SettingXml.Inst.MachineName
            };

            MmiMessageInfo messageInfo = new MmiMessageInfo
            {
                Message = content.ToXmlContent()
            };

            SendMessage(messageInfo);
        }

        private void Client_Opened(object sender, EventArgs e)
        {
            Opened(this, e);
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            Closed(this, e);

            if (_clinet != null)
            {
                _clinet.WcfReceivedMessage -= Client_WcfReceivedMessage;
                _clinet.Closed -= Client_Closed;
                _clinet.Opened -= Client_Opened;
                _clinet = null;
            }
        }

        private void Client_WcfReceivedMessage(object sender, IMessageDuplexCallback channel,
            MmiMessageInfo message, ClientInfo clinet)
        {
            _currentMessage = XmlHelper.XmlDeserialize<Message>(message.Message);
            string tool = string.IsNullOrWhiteSpace(_currentMessage.Tool) ? "" : $" Tool={_currentMessage.Tool}";
            string err = string.IsNullOrWhiteSpace(_currentMessage.ErrorMessage) ? "" : $" Err={_currentMessage.ErrorMessage}";

            // ★ Fail/Error일 때만 로그 타입을 Error로
            var logType =
                (_currentMessage.Command == Message.MessageCommand.Error ||
                 _currentMessage.Status == Message.CommandStatus.Fail)
                    ? SLogger.LogType.Error
                    : SLogger.LogType.Info;

            SLogger.Write(
                $"[WCF RX] Time={_currentMessage.Time} Cmd={_currentMessage.Command} Status={_currentMessage.Status}{tool}{err}",
                logType);

            ReceiveMessage(this, _currentMessage);
        }
    }
}

