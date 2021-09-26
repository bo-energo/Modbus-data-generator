using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mimic_Console
{
    public class Settings
    {
        public TcpSlave_Settings TcpSlave { get; set; } = new TcpSlave_Settings();

        //public RtuSlave_Settings RtuMmaster { get; set; } = new RtuSlave_Settings();
    }

    public class TcpSlave_Settings
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public byte Address { get; set; }
        public int[] Gen_Period { get; set; }
        public int[] Gen_End { get; set; }
        public Generation_Settings[] Generate_Set { get; set; }
    }

    public class RtuSlave_Settings
    {

    }

    public class Generation_Settings
    {
        public string Name { get; set; }
        public int Address { get; set; }
        public string Data_Type { get; set; }
        public string Register_Type { get; set; }

        public object Sinewave_Use { get; set; }
        public object Sinewave_Count { get; set; }
        public object Sinewave_Amplitude { get; set; }
        public object Sinewave_Period { get; set; }
        public object Sinewave_Phase { get; set; }

        public object Randwalk_Use { get; set; }
        public object Randwalk_Start { get; set; }
        public object Randwalk_Factor { get; set; }

        public object Trend_Use { get; set; }
        public object Trend_Slope { get; set; }
        public object Trend_Zero { get; set; }

    }
}
