using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using BO_Math;
using BO_Modbus;

namespace Mimic_Console
{
    class Program
    {
        // #################################################################################

        static void Main(string[] args)
        {
            var settings = JsonControl.LoadSettings("Settings.json");
            var converter = new JsonConverter();
            var signals = new List<Signal>();

            for (int i = 0; i < settings.TcpSlave.Signals_Set.Length; i++)
            {
                var genset = settings.TcpSlave.Signals_Set[i];
                DataGenerator generator = converter.CreateGenerator(genset.Components);
                signals.Add(new Signal(genset, generator));
            }

            DisplayInfo(settings, signals);
            Loop(settings, signals);

            Console.ReadLine();
            //Application.Run(new Form1(generator));
        }

        static void Loop(Settings settings, List<Signal> signals)
        {
            int exStage = 0;

            try
            {
                TcpSlave tcpSlave = new TcpSlave(settings.TcpSlave.IP, settings.TcpSlave.Port, settings.TcpSlave.Address);
                tcpSlave.Start();

                DateTime start_time = DateTime.Now;
                int hour = settings.TcpSlave.Gen_Period[0];
                int minute = settings.TcpSlave.Gen_Period[1];
                int sec = settings.TcpSlave.Gen_Period[2];
                TimeSpan period = new TimeSpan(hour, minute, sec);

                DisplayTitle("Начало записи");
                exStage = 1;

                while (true)
                {
                    DateTime now_time = DateTime.Now;

                    Console.WriteLine();
                    DisplayChapter($"{now_time}", ConsoleColor.DarkYellow);
                    DisplayLine(ConsoleColor.DarkYellow);

                    foreach (var signal in signals)
                    {
                        var values = signal.Next();

                        List<string> paramList = new List<string>();
                        paramList.Add(signal.name);
                        paramList.Add("Значение");
                        paramList.Add(signal.last.ToString() + '\t');
                        paramList.Add("Записано");

                        string mess = $"[";
                        foreach (var item in values) mess += $"{item}, ";
                        mess = mess.Remove(mess.Length - 2) + "]";
                        paramList.Add(mess);
                        DisplayParam(paramList.ToArray());

                        tcpSlave.Write(signal.address, values, signal.register);
                    }
                    
                    Thread.Sleep(period);
                }
            }
            catch (Exception ex)
            {
                string mess;
                switch (exStage)
                {
                    case 1:
                        mess = "Ошибка при записи данных! " + ex.Message;
                        break;
                    default:
                        mess = "Ошибка при запуске TCP-slave! " + ex.Message;
                        break;
                }

                Console.WriteLine(mess);
            }
        }

        static void DisplayInfo(Settings settings, List<Signal> signals)
        {
            DisplayLogo();

            DisplayTitle("Параметры сервера");
            DisplayParam("IP", settings.TcpSlave.IP);
            DisplayParam("Port", settings.TcpSlave.Port);
            var time = settings.TcpSlave.Gen_Period;
            TimeSpan period = new TimeSpan(time[0], time[1], time[2]);
            time = settings.TcpSlave.Gen_End;
            TimeSpan timeEnd = new TimeSpan(time[0], time[1], time[2]);
            DisplayParam("Период записи", period);
            DisplayParam("Конец записи", timeEnd);

            DisplayTitle("Параметры сигналов");
            for (int i = 0; i< signals.Count; i++)
            {
                DisplayChapter($"Сигнал №{i+1}:", ConsoleColor.DarkYellow);
                DisplayLine(ConsoleColor.DarkYellow);
                DisplayParam("Имя сигнала", signals[i].name);
                DisplayParam("Slave ID", signals[i].address);
                DisplayParam("Тип данных", signals[i].dataType);
                DisplayParam("Тип регистра", signals[i].register);
                Console.WriteLine();

                DisplayChapter("Компоненты Сигнала:", ConsoleColor.Yellow);
                foreach (var generator in signals[i].generator.Generators)
                {
                    DisplayParam(generator.GetParams());
                }
                DisplayLine(ConsoleColor.DarkYellow);
                Console.WriteLine();
            }
        }

        // #################################################################################

        static void DisplayLogo()
        {
            int pad = 22;
            Console.Write("\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "        ██  ██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "    ██  ██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "██  ██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "██  ██  ██                      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "██  ██  ██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "██  ██  ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "    ██  ██  ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██  ██" + "\n\n");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("".PadLeft(pad) + "        ██      ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("██  ██  ██  ██  ██" + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;

            //Thread.Sleep(1000);
        }

        static void DisplayTitle(string name)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\n   ");
            Console.Write("".PadLeft(80, '▬'));
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.Green;
            int b = name.Length / 2;
            Console.Write(name.PadLeft(43+b).PadRight(80+b));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("\n   ");
            Console.Write("".PadLeft(80, '▬'));
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\n");
        }

        static void DisplayChapter(string name, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            int b = name.Length / 2;
            Console.Write(name.PadLeft(43 + b).PadRight(80 + b) + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void DisplayLine(ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write("".PadLeft(25).PadRight(60, '▬') + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void DisplayParam(string name, object value)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{name}: ".PadLeft(30));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{value}\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void DisplayParam(string[] paramList)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{paramList[0]}:".PadLeft(30));

            for (int i = 1; i<paramList.Length-1; i+=2)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($" {paramList[i]}: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{paramList[i+1]}");
            }

            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    public class Signal
    {
        public string name;
        public int address;
        public string dataType;
        public RegistersTypes register;
        public DataGenerator generator;
        public object last;

        // #################################################################################

        public Signal(Signals_Settings settings, DataGenerator generator)
        {
            name = settings.Name;
            address = settings.Address;
            dataType = settings.Data_Type;
            this.generator = generator;

            switch (settings.Register_Type)
            {
                case "Holding":
                    register = RegistersTypes.Holding;
                    break;
                case "Input":
                    register = RegistersTypes.Input;
                    break;
            }
        }

        public ushort[] Next()
        {
            List<ushort> values = new List<ushort>();
            double value = generator.Next();
            last = value;

            switch (dataType)
            {
                case "Int16":
                    byte[] bites = BitConverter.GetBytes(Convert.ToInt16(value));
                    values.Add(BitConverter.ToUInt16(bites, 0));
                    last = Convert.ToInt16(value);
                    break;
                case "Int32":
                {
                    int[] buf = new int[] {Convert.ToInt32(value)};
                    values.AddRange(TypeConverter.I32_to_UI16(buf));
                    last = Convert.ToInt32(value);
                    break;
                }
                case "Float16":
                {
                    float[] buf = new float[] {Convert.ToSingle(value)};
                    values.AddRange(TypeConverter.F32_to_UI16(buf));
                    break;
                }
                case "Float32":
                {
                    float[] buf = new float[] { Convert.ToSingle(value) };
                    values.AddRange(TypeConverter.F32_to_UI16(buf));
                    break;
                }
            }

            return values.ToArray();
        }

    }
}
