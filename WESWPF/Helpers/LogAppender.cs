using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WES.Helpers
{
    public class LogAppender : AppenderSkeleton
    {
        public event Action<object> LogAppendEvent;
        protected override void Append(LoggingEvent loggingEvent)
        {
            string log;
            if (Layout != null)
            {
                PatternLayout patternLayout = Layout as PatternLayout;
                log = patternLayout.Format(loggingEvent);

                if (loggingEvent.ExceptionObject != null)
                {
                    log += loggingEvent.ExceptionObject.ToString();
                }
            }
            else
            {
                log = loggingEvent.RenderedMessage;
            }
            LogAppendEvent?.Invoke(log);
        }
    }
}
