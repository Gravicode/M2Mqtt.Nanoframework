using System;
using System.Collections;
using System.Text;

namespace M2Mqtt.Nanoframework.TimeHelper
{
    public class Environment
    {
        public static int TickCount => (int)(DateTime.UtcNow.Ticks / 10000);
        
    }
}
