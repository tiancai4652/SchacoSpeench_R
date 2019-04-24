using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoRecorder_WPF
{
    public class MyLogger
    {
        static ILogger _LoggerInstance;
        public static ILogger LoggerInstance
        {
            get
            {
                if (_LoggerInstance == null)
                {
                    _LoggerInstance = LogManager.GetCurrentClassLogger();
                }
                return _LoggerInstance;
            }
        }
    }
}
