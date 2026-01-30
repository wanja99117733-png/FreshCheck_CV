using FreshCheck_CV.Setting;     // SettingXml 네임스페이스(중요)
using FreshCheck_CV.Util;        // SLogger
using System;
using System.ServiceModel;

namespace FreshCheck_CV.Sequence
{
    /// <summary>
    /// FC용: 연결/핸드셰이크/검사결과 전송(InspDone)
    /// </summary>
    public sealed class FcSequenceClient : IDisposable
    {
        // ✅ 기존 Communicator(없음) 대신, 프로젝트에 있는 FcCommunicator 사용
        private readonly FcCommunicator _comm = new FcCommunicator();

        private bool _disposed;

        public bool IsConnected => _comm.State == CommunicationState.Opened;

        public void Initialize()
        {
            // 1) 설정 로드(Setup\Setting.xml)
            SettingXml.Load();

            // 2) 장비 식별값 세팅(서버 UI 상단 표시 매칭용)
            _comm.MachineName = SettingXml.Inst.MachineName;

            // 필요하면 아래도 SettingXml에 항목 추가 후 연결 가능
            // _comm.ModelName = "...";
            // _comm.SerialId = "...";
            // _comm.AppVersion = "...";

            // 3) 이벤트 연결
            _comm.Opened += (s, e) =>
            {
                SLogger.Write($"[WCF] Opened. ip={SettingXml.Inst.CommIP}", SLogger.LogType.Info);

                // 연결되면 즉시 핸드셰이크
                FcAck ack = _comm.SendMachineInfo();
                SLogger.Write($"[WCF] HandShake ack: ok={ack?.Ok}, msg={ack?.Message}", SLogger.LogType.Info);
            };

            _comm.Closed += (s, e) =>
            {
                SLogger.Write("[WCF] Closed.", SLogger.LogType.Error);
            };

            // 서버 -> FC 명령(확장용)
            _comm.ReceiveCommand += (s, cmd) =>
            {
                if (cmd == null)
                    return;

                // 예: cmd.Command == FcCommand.InspStart / InspStop 등
                SLogger.Write($"[WCF] ServerCommand: {cmd.Command}, product={cmd.ProductNo}, tool={cmd.Tool}", SLogger.LogType.Info);
            };

            // 4) 생성 + 연결
            //    ✅ SettingXml.Inst.CommIP에 "IP:PORT" 형태도 허용되도록 아래 2)에서 FcCommunicator.Create 수정할 거야.
            _comm.Create(SettingXml.Inst.CommType, SettingXml.Inst.CommIP);
        }

        /// <summary>
        /// InspDone 전송(OK/NG 통지)
        /// </summary>
        public void SendInspDone(long inspectionNo, bool isNg, string ngDetail)
        {
            if (!IsConnected)
            {
                SLogger.Write("[WCF] Not connected. Skip SendInspDone.", SLogger.LogType.Error);
                return;
            }

            // 1) NG 사유를 문자열에서 간단 매핑
            FcNgReason reason = MapNgReason(isNg, ngDetail);

            // 2) 서버로 보낼 결과 패킷 구성(FcMessages.cs의 FcInspectionResult 사용)
            var result = new FcInspectionResult
            {
                InspectionNo = inspectionNo,
                ProductNo = inspectionNo.ToString(),
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),

                Judge = isNg ? FcJudge.Ng : FcJudge.Good,
                NgReason = reason,

                // 지금은 MoldRatio/Threshold 같은 수치가 없으니 기본값(0) 유지
                MoldRatio = 0,
                MoldThreshold = 0,

                ScratchCount = 0,
                ScratchScore = 0,

                // 메시지/로그 용도
                Message = isNg ? (ngDetail ?? string.Empty) : string.Empty,

                // 장비 식별
                MachineName = _comm.MachineName,
                ModelName = _comm.ModelName,
                SerialId = _comm.SerialId,
            };

            // 3) 전송
            FcAck ack = _comm.SendInspection(result);
            bool ok = ack != null && ack.Ok;

            SLogger.Write(
                $"[WCF] Send InspDone: inspNo={inspectionNo}, isNg={isNg}, ok={ok}, reason={reason}, detail={ngDetail}",
                ok ? SLogger.LogType.Info : SLogger.LogType.Error);
        }

        private static FcNgReason MapNgReason(bool isNg, string ngDetail)
        {
            if (!isNg)
                return FcNgReason.None;

            if (string.IsNullOrWhiteSpace(ngDetail))
                return FcNgReason.Unknown;

            bool hasMold =
                ngDetail.IndexOf("mold", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ngDetail.IndexOf("곰팡", StringComparison.OrdinalIgnoreCase) >= 0;

            bool hasScratch =
                ngDetail.IndexOf("scratch", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ngDetail.IndexOf("스크래치", StringComparison.OrdinalIgnoreCase) >= 0;

            if (hasMold && hasScratch) return FcNgReason.Both;
            if (hasMold) return FcNgReason.Mold;
            if (hasScratch) return FcNgReason.Scratch;
            return FcNgReason.Unknown;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

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
