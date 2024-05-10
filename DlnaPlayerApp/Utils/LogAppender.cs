using log4net.Appender;
using log4net.Core;
using System;
using System.Windows.Forms;

namespace DlnaPlayerApp.Utils
{
    public class LogAppender : AppenderSkeleton
    {
        public TextBox LogTextBox { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (LogTextBox == null || LogTextBox.IsDisposed)
            {
                return;
            }
            if (LogTextBox.InvokeRequired)
            {
                LogTextBox.BeginInvoke(new Action(() => Append(loggingEvent)));
                return;
            }
            if (LogTextBox.Text.Length > 100 * 1024)
            {
                LogTextBox.Clear();
            }

            LogTextBox.AppendText(RenderLoggingEvent(loggingEvent));
        }
    }
}
