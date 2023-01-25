using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleLauncher
{
    internal enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    }

    internal static class Logger
    {
        // Create a logger for use in this class
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool DebugEnabled = log.IsDebugEnabled;
        private static readonly bool WarnEnabled = log.IsWarnEnabled;
        private static readonly bool InfoEnabled = log.IsInfoEnabled;

        /// <summary>
        /// Log informational messages for troubleshooting.
        /// </summary>
        /// <param name="Message">Message to log</param>
        /// <param name="StackMethod">Current executing method</param>
        /// <param name="Level">Type of message being logged. One of: Debug, Info, Warn or Error (default)</param>        
        public static void Log(string Message, string StackMethod, LogLevel Level = LogLevel.Error)
        {
            // Buid message
            string logmessage = StackMethod + " at: " + Message;

            // Log
            switch (Level)
            {
                case LogLevel.Debug:
                    // Log debug only if enabled
                    if (DebugEnabled)
                        log.Debug(logmessage);
                    break;
                case LogLevel.Info:
                    // Log Info only if enabled
                    if (InfoEnabled)
                        log.Info(logmessage);
                    break;
                case LogLevel.Warn:
                    // Log warnings only if enabled
                    if (WarnEnabled)
                        log.Warn(logmessage);
                    break;
                default:
                    // Log error
                    log.Error(logmessage);
                    break;
            }

            return;
        }

    }
}
