using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoRecorder_WPF
{
    public class Gloable
    {
        static string _RecordFileName;
        public static string RecordFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_RecordFileName))
                {
                    _RecordFileName = System.Environment.CurrentDirectory + "\\" + "Output" + "\\" + "out.wav";
                }
                return _RecordFileName;
            }
        }
    }
}
