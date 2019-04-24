using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoRecorderer
{
    public class MyAudioInputDevice
    {
        public int Channels { get; set; }
        public MMDevice Device { get; set; }
        public CaptureMode CaptureMode { get; set; }
    }
}
