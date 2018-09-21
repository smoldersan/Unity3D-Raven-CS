using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


namespace Unity3DRavenCS
{
    public abstract class Packet
    {
        public struct SDK
        {
            public string name;
            public string version;
        }

        [JsonProperty(PropertyName = "sdk")]
        protected SDK m_sdk = new SDK();

        [JsonProperty(PropertyName = "event_id")]
        protected string m_eventID;
        [JsonProperty(PropertyName = "message")]
        protected string m_message;
        [JsonProperty(PropertyName = "timestamp")]
        protected string m_timestamp;
        [JsonProperty(PropertyName = "platform")]
        protected string m_platform;
        [JsonProperty(PropertyName = "release")]
        protected string m_release;
        [JsonProperty(PropertyName = "tags")]
        protected Dictionary<string, string> m_tags;
        [JsonProperty(PropertyName = "fingerprint")]
        protected List<string> m_fingerprint;

        public Packet(SentryConfig sentryConfig, string message, Dictionary<string, string> tags)
        {
            m_eventID = Guid.NewGuid().ToString("N");
            m_message = message;
            m_platform = "csharp";
            m_sdk.name = "Unity3D-Raven-CS";
            m_sdk.version = Version.VERSION;
            m_timestamp = DateTime.UtcNow.ToString("s");
            m_release = sentryConfig.release;
            m_tags = tags;
            m_fingerprint = GetFingerPrint(message);
        }

        private List<string> GetFingerPrint(string message)
        {
            string messageFirstLine = message;

            if (messageFirstLine.Contains("\n"))
            {
                messageFirstLine = message.Substring(0, message.IndexOf('\n'));
            }

            string[] fingerprint = { "{{ default }}", messageFirstLine};
            return new List<string>(fingerprint);
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class MessagePacket: Packet
    {
#pragma warning disable 0414
        [JsonProperty(PropertyName = "level")]
        private string m_level;
        [JsonProperty(PropertyName = "logger")]
        private string m_logger;
        [JsonProperty(PropertyName = "stacktrace")]
        private RavenStackTrace m_stacktrace;
#pragma warning restore 0414

        public MessagePacket(SentryConfig sentryConfig, string message, LogType logType, Dictionary<string, string> tags, string stackTrace): base(sentryConfig, message, tags)
        {
            m_level = ToLogLevelFromLogType(logType);
            if (!string.IsNullOrEmpty(stackTrace))
            {
                m_stacktrace = new RavenStackTrace(stackTrace);
            }
        }

        public MessagePacket(SentryConfig sentryConfig, string message, LogType logType, Dictionary<string, string> tags, System.Diagnostics.StackTrace stackTrace) : base(sentryConfig, message, tags)
        {
            this.m_level = ToLogLevelFromLogType(logType);
            if (stackTrace != null)
            {
                m_stacktrace = new RavenStackTrace(stackTrace);
            }
        }

        private string ToLogLevelFromLogType(LogType logType)
        {
            string logLevel;
            switch (logType)
            {
            case LogType.Log:
                logLevel = "info";
                break;
            case LogType.Warning:
                logLevel = "warning";
                break;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                logLevel = "error";
                break;
            default:
                logLevel = "error";
                break;
            }
            return logLevel;
        }
    }


    public class ExceptionPacket: Packet
    {
#pragma warning disable 0414
        [JsonProperty(PropertyName = "exception")]
        private RavenException m_exception;
#pragma warning restore 0414

        public ExceptionPacket(SentryConfig sentryConfig, Exception exception, Dictionary<string, string> tags): base(sentryConfig, exception.Message, tags)
        {
            this.m_exception = new RavenException(exception);
        }

        public ExceptionPacket(SentryConfig sentryConfig, string message, string stackTrace, Dictionary<string, string> tags) : base(sentryConfig, message, tags)
        {
            this.m_exception = new RavenException(message, stackTrace);
        }

        public ExceptionPacket(SentryConfig sentryConfig, string message, System.Diagnostics.StackTrace stackTrace, Dictionary<string, string> tags) : base(sentryConfig, message, tags)
        {
            this.m_exception = new RavenException(message, stackTrace);
        }
    }

    public struct ResponsePacket
    {
        public string id;
    }
}
