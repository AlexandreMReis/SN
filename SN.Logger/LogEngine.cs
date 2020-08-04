using NLog;
using System;

namespace SN.Logger
{
    /// <summary>
    /// The 'LogEngine' class
    /// </summary>
    public class LogEngine
    {
        /// <summary>
        /// The logger
        /// </summary>
        public static readonly NLog.Logger Logger = LogManager.GetLogger("FileLogger");

        /// <summary>
        /// The Book logger
        /// </summary>
        public static readonly NLog.Logger BookLogger = LogManager.GetLogger("BookLogger");

        /// <summary>
        /// The CLI logger
        /// </summary>
        public static readonly NLog.Logger CLILogger = LogManager.GetLogger("CLILogger");

        /// <summary>
        /// The dependency injection logger
        /// </summary>
        public static readonly NLog.Logger DILogger = LogManager.GetLogger("DILogger");

        /// <summary>
        /// The Member logger
        /// </summary>
        public static readonly NLog.Logger MemberLogger = LogManager.GetLogger("MemberLogger");
    }
}
