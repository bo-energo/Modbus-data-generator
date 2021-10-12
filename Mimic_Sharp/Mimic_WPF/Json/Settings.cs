using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mimic_WPF
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
        public byte Slave_ID { get; set; }
        public int[] Gen_Period { get; set; }
        public int[] Gen_End { get; set; }
        public Signals_Settings[] Signals_Set { get; set; }
    }

    public class RtuSlave_Settings
    {

    }

    public class Signals_Settings
    {
        public string Name { get; set; }
        public int Address { get; set; }
        public string Data_Type { get; set; }
        public string Register_Type { get; set; }

        public Component[] Components { get; set; }
    }

    public class Component
    {
        public string Name { get; set; } = "";

        public Dictionary<string, object> Params { get; set; }
    }

}
