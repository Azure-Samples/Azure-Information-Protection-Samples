using System;
using Microsoft.Applications.Telemetry.Windows;
using com.microsoft.rightsmanagement.mobile.viewer.lib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.microsoft.rightsmanagement.windows.viewer.Telemetry
{
    class TelemetryWindows : TelemetryBase
    {
        //private static Android.App.Application m_Application;
        //private static Context m_Context;

        private static ILogger m_AriaLogger = null;

        protected override void StartImpl(){ }

        protected override void CreateAriaLogger(bool isAutoSession = true)
        {
            if (m_AriaLogger != null)
                return;

            m_AriaLogger = LogManager.Initialize(MSIP_TENANT_TOKEN);
        }

        protected override void StopImpl() {}

        protected override void LogEventImpl(TelemetryEvent eventType, TimeSpan? ts, Dictionary<string, string> props)
        {
            var eventData = new EventProperties(eventType.ToString());
            eventData.Properties = props;
            if (ts != null)
                eventData.Measurements = PrepareDuration(ts);

            m_AriaLogger.LogEvent(eventData);

        }

        protected override void LogFailureImpl(TelemetryEvent eventType, string details, Dictionary<string, string> props)
        {
            var eventData = new EventProperties(ERROR_EVENT_PROPERTIES_NAME);
            eventData.Properties = props;

            m_AriaLogger.LogFailure(eventType.ToString(), details, null, null, eventData);
        }

        private Dictionary<string, double> PrepareDuration(TimeSpan? ts)
        {
            Dictionary<string, double> durationProp = new Dictionary<string, double>();

            Double sec = ts.Value.TotalSeconds;
            durationProp.Add("Duration", sec);

            return durationProp;
        }
    }
}
