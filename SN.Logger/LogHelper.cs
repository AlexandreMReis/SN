using System;
using System.Collections.Generic;
using System.Text;

namespace SN.Logger
{
    public static class LogHelper
    {
        /// <summary>
        /// Writes to log
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="level">The level</param>
        /// <param name="deviceInfo">(internal) The device info</param>
        /// <param name="message">The message</param>
        public static void WriteToLog(this NLog.Logger logger, LogLevels level, string message)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    logger.Debug(message);
                    break;
                case LogLevels.Info:
                    logger.Info(message);
                    break;
                case LogLevels.Warning:
                    logger.Warn(message);
                    break;
                case LogLevels.Error:
                    logger.Error(message);
                    break;
                default:
                    logger.Debug(message);
                    break;
            }
        }

        /// <summary>
        /// Writes the metric
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="duration">The duration</param>
        /// <param name="succcess">Success or not</param>
        public static void WriteMetric(string request, TimeSpan duration, bool? succcess)
        {
            WriteMetric(request, duration, succcess.GetValueOrDefault() ? 0 : -1);
        }

        /// <summary>
        /// Writes the metric
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="duration">The duration</param>
        /// <param name="status">The status (0 for success and -1 for error)</param>
        public static void WriteMetric(string request, TimeSpan duration, int status)
        {
            NLog.Logger logger = LogEngine.Logger;
            logger.Info($"{request};{(int)duration.TotalMilliseconds};{status}");
        }
    }
}
