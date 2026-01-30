using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace FreshCheck_CV.Sequence
{
    public enum FcCommand
    {
        None = 0,
        HandShake = 1,
        InspDone = 2,
        Heartbeat = 3,
        Error = 4,

        // (서버 -> 비전) 확장용
        InspStart = 100,
        InspStop = 101
    }

    public enum FcJudge
    {
        None = 0,
        Good = 1,
        Ng = 2
    }

    public enum FcNgReason
    {
        None = 0,
        Mold = 1,
        Scratch = 2,
        Both = 3,
        Unknown = 9
    }

    [DataContract]
    public sealed class FcAck
    {
        [DataMember(Order = 1)] public bool Ok { get; set; }
        [DataMember(Order = 2)] public string Message { get; set; }
        [DataMember(Order = 3)] public long EchoInspectionNo { get; set; }
        [DataMember(Order = 4)] public string ServerTime { get; set; }
    }

    [DataContract]
    public sealed class FcHandShakeRequest
    {
        [DataMember(Order = 1)] public string MachineName { get; set; }   // ex) VISION01
        [DataMember(Order = 2)] public string ModelName { get; set; }     // ex) TEST
        [DataMember(Order = 3)] public string SerialId { get; set; }      // ex) WAF-...
        [DataMember(Order = 4)] public string AppVersion { get; set; }    // ex) 1.0.0
        [DataMember(Order = 5)] public string Time { get; set; }          // "HH:mm:ss.fff"
    }

    [DataContract]
    public sealed class FcCounters
    {
        [DataMember(Order = 1)] public int Total { get; set; }
        [DataMember(Order = 2)] public int Good { get; set; }
        [DataMember(Order = 3)] public int Ng { get; set; }
        [DataMember(Order = 4)] public int Mold { get; set; }
        [DataMember(Order = 5)] public int Scratch { get; set; }
        [DataMember(Order = 6)] public int Both { get; set; }
    }

    [DataContract]
    public sealed class FcTiming
    {
        [DataMember(Order = 1)] public string InspectStartTime { get; set; }  // "HH:mm:ss.fff"
        [DataMember(Order = 2)] public string InspectEndTime { get; set; }    // "HH:mm:ss.fff"
        [DataMember(Order = 3)] public long ProcessMsTotal { get; set; }
        [DataMember(Order = 4)] public long MoldMs { get; set; }
        [DataMember(Order = 5)] public long ScratchMs { get; set; }
    }

    [DataContract]
    public sealed class FcInspectionResult
    {
        // 식별/표시
        [DataMember(Order = 1)] public long InspectionNo { get; set; }     // 일련번호
        [DataMember(Order = 2)] public string ProductNo { get; set; }      // 제품번호(없으면 InspectionNo 문자열로 대체)
        [DataMember(Order = 3)] public string Time { get; set; }           // "HH:mm:ss.fff"

        // 판정
        [DataMember(Order = 10)] public FcJudge Judge { get; set; }        // Good/Ng
        [DataMember(Order = 11)] public FcNgReason NgReason { get; set; }  // Mold/Scratch/Both
        [DataMember(Order = 12)] public double MoldRatio { get; set; }     // Mold면적비(없으면 0)
        [DataMember(Order = 13)] public double MoldThreshold { get; set; } // Mold 임계(예: 0.01)
        [DataMember(Order = 14)] public int ScratchCount { get; set; }     // Scratch 객체 수(가능하면)
        [DataMember(Order = 15)] public double ScratchScore { get; set; }  // 모델 score(가능하면)

        // 파일/로그
        [DataMember(Order = 20)] public string SavedPath { get; set; }     // FC 저장 경로
        [DataMember(Order = 21)] public string Message { get; set; }       // 설명/디버그

        // 운영 정보(서버 화면 우측 Item/Value용)
        [DataMember(Order = 30)] public FcCounters Counters { get; set; }
        [DataMember(Order = 31)] public FcTiming Timing { get; set; }

        // 장비 식별(상단 MODEL/SERIAL ID용)
        [DataMember(Order = 40)] public string MachineName { get; set; }
        [DataMember(Order = 41)] public string ModelName { get; set; }
        [DataMember(Order = 42)] public string SerialId { get; set; }

        // (선택) 서버가 이미지를 직접 갱신하려면: Base64 JPEG
        [DataMember(Order = 50)] public string ImageJpegBase64 { get; set; }
    }

    [DataContract]
    public sealed class FcHeartbeat
    {
        [DataMember(Order = 1)] public string MachineName { get; set; }
        [DataMember(Order = 2)] public string Time { get; set; }
        [DataMember(Order = 3)] public FcCounters Counters { get; set; }
        [DataMember(Order = 4)] public string State { get; set; } // RUNNING/IDLE/ERROR
    }

    // (서버 -> FC) 확장용: Start/Stop/Recipe 등
    [DataContract]
    public sealed class FcServerCommand
    {
        [DataMember(Order = 1)] public FcCommand Command { get; set; }     // InspStart/InspStop
        [DataMember(Order = 2)] public string ProductNo { get; set; }      // 서버가 내려줄 수 있음
        [DataMember(Order = 3)] public string Tool { get; set; }           // 모델명 등
        [DataMember(Order = 4)] public string Time { get; set; }
    }
}
