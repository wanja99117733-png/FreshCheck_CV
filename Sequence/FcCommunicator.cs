using FreshCheck_CV.Util;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace FreshCheck_CV.Sequence
{
    public enum CommunicatorType
    {
        None = 0,
        WCF = 1
    }

    public sealed class FcCommunicator : IFcDuplexCallback, IDisposable
    {
        public event EventHandler Opened = delegate { };
        public event EventHandler Closed = delegate { };
        public event EventHandler<FcServerCommand> ReceiveCommand = delegate { };

        private readonly object _sync = new object();

        private DuplexChannelFactory<IFcDuplexService> _factory;
        private IFcDuplexService _proxy;
        private ICommunicationObject _commObj;

        private string _ip;
        private int _port;

        // 식별(서버 UI 상단과 매칭)
        public string MachineName { get; set; } = "VISION01";
        public string ModelName { get; set; } = "TEST";
        public string SerialId { get; set; } = "FC-LOCAL";
        public string AppVersion { get; set; } = "1.0.0";

        public CommunicationState State
        {
            get
            {
                if (_commObj == null) return CommunicationState.Faulted;
                return _commObj.State;
            }
        }

        public void Create(CommunicatorType type, params object[] parameters)
        {
            if (type == CommunicatorType.None)
                return;

            _ip = parameters[0] as string;
            _port = (int)parameters[1];

            ConnectInternal();
        }

        public bool Connect()
        {
            lock (_sync)
            {
                if (_commObj != null && _commObj.State == CommunicationState.Opened)
                    return true;

                return ConnectInternal();
            }
        }

        public void Close()
        {
            lock (_sync)
            {
                SafeClose_NoThrow();
            }
        }

        public FcAck SendMachineInfo()
        {
            lock (_sync)
            {
                if (_proxy == null) return new FcAck { Ok = false, Message = "Proxy is null" };

                var req = new FcHandShakeRequest
                {
                    MachineName = MachineName,
                    ModelName = ModelName,
                    SerialId = SerialId,
                    AppVersion = AppVersion,
                    Time = DateTime.Now.ToString("HH:mm:ss.fff")
                };

                return _proxy.HandShake(req);
            }
        }

        public FcAck SendInspection(FcInspectionResult result)
        {
            lock (_sync)
            {
                if (_proxy == null)
                    return new FcAck { Ok = false, Message = "Proxy is null" };

                return _proxy.SendInspectionResult(result);
            }
        }

        public void SendHeartbeat(FcHeartbeat hb)
        {
            lock (_sync)
            {
                if (_proxy == null) return;
                _proxy.Heartbeat(hb);
            }
        }

        // ===== Callback (Server -> FC) =====
        public void OnServerCommand(FcServerCommand cmd)
        {
            ReceiveCommand(this, cmd);
        }

        // ===== Internal =====
        private bool ConnectInternal()
        {
            SafeClose_NoThrow();

            try
            {
                // net.tcp://{ip}:{port}/FCComm
                string address = $"net.tcp://{_ip}:{_port}/FCComm";
                var endpoint = new EndpointAddress(address);

                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                binding.MaxReceivedMessageSize = 16 * 1024 * 1024; // 16MB (이미지 Base64 쓰면 필요)
                binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                binding.SendTimeout = TimeSpan.FromSeconds(5);
                binding.OpenTimeout = TimeSpan.FromSeconds(3);
                binding.CloseTimeout = TimeSpan.FromSeconds(3);

                // Duplex
                var ctx = new InstanceContext(this);
                _factory = new DuplexChannelFactory<IFcDuplexService>(ctx, binding, endpoint);

                _proxy = _factory.CreateChannel();
                _commObj = (ICommunicationObject)_proxy;

                _commObj.Opened += (s, e) => Opened(this, EventArgs.Empty);
                _commObj.Closed += (s, e) => Closed(this, EventArgs.Empty);
                _commObj.Faulted += (s, e) =>
                {
                    SLogger.Write("[WCF] Faulted", SLogger.LogType.Error);
                    Closed(this, EventArgs.Empty);
                };

                _commObj.Open();

                SLogger.Write($"[WCF] Opened : {address}", SLogger.LogType.Info);
                Opened(this, EventArgs.Empty);

                return true;
            }
            catch (Exception ex)
            {
                SLogger.Write($"[WCF] Connect failed : {ex.Message}", SLogger.LogType.Error);
                SafeClose_NoThrow();
                return false;
            }
        }

        private void SafeClose_NoThrow()
        {
            try
            {
                if (_commObj != null)
                {
                    try
                    {
                        if (_commObj.State == CommunicationState.Faulted)
                            _commObj.Abort();
                        else
                            _commObj.Close();
                    }
                    catch { _commObj.Abort(); }
                }
            }
            catch { }

            try { if (_factory != null) _factory.Close(); } catch { try { _factory.Abort(); } catch { } }

            _proxy = null;
            _commObj = null;
            _factory = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
