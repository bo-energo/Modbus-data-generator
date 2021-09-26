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
        static Random rand;
        static JsonConverter converter;

        // #################################################################################

        static void Main(string[] args)
        {
            Settings settings = JsonControl.LoadSettings("Settings.json");
            //DataGenerator generator = Create_Generator(settings.TcpSlave.Generate_Set[0], 1);
            List<Signal> signals = new List<Signal>();
            rand = new Random();
            converter = new JsonConverter();

            for (int i = 0; i < settings.TcpSlave.Generate_Set.Length; i++)
            {
                var genset = settings.TcpSlave.Generate_Set[i];
                DataGenerator generator = Create_Generator(genset, i+1);
                signals.Add(new Signal(genset, generator));
            }

            Loop(settings, signals);

            //Application.Run(new Form1(generator));
        }

        static void Loop(Settings settings, List<Signal> signals)
        {
            TcpSlave tcpSlave;

            try
            {
                tcpSlave = new TcpSlave(settings.TcpSlave.IP, settings.TcpSlave.Port, settings.TcpSlave.Address);
                tcpSlave.Start();

                //AddLogMessage($"TCP-slave по адресу: {address}, порт {port} - Включён");
            }
            catch (Exception ex)
            {
                string message = $"Ошибка при подключении TCP-slave! {ex.Message}";
                Console.WriteLine(message);
                //AddLogMessage("ERROR! " + message);
                return;
            }

            try
            {
                DateTime start_time = DateTime.Now;
                int hour = settings.TcpSlave.Gen_Period[0];
                int minute = settings.TcpSlave.Gen_Period[1];
                int sec = settings.TcpSlave.Gen_Period[2];
                TimeSpan period = new TimeSpan(hour, minute, sec);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\n\n   ");
                for (int i = 0; i < 80; i++) Console.Write('#');
                Console.Write("\n\t\t Начало записи...\n   ");
                for (int i = 0; i < 80; i++) Console.Write('#');
                Console.Write("\n\n");
                Console.ForegroundColor = ConsoleColor.Green;


                while (true)
                {
                    DateTime now_time = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine($"\t ## [{now_time}] Запись:");
                    Console.ForegroundColor = ConsoleColor.Green;

                    foreach (var signal in signals)
                    {
                        var values = signal.Next();

                        string mess = $"\t\t - {signal.name}: значение - [{signal.last}], записано - [";
                        foreach (var item in values) mess += $"{item}, ";
                        mess = mess.Remove(mess.Length - 2) + "]";
                        Console.WriteLine(mess);

                        tcpSlave.Write(signal.address, values, signal.register);
                    }

                    Thread.Sleep(period);
                }

            }
            catch (Exception ex)
            {
                string message = $"Ошибка при записи данных TCP-slave! {ex.Message}";
                Console.WriteLine(message);
                Console.ReadLine();
                //AddLogMessage("ERROR! " + message);
            }
        }

        static DataGenerator Create_Generator(Generation_Settings settings, int num)
        {
            DataGenerator generator = new DataGenerator();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n\n   ");
            for (int i = 0; i < 80; i++) Console.Write('#');
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n\t Параметры генерации {num} сигнала:");
            Console.WriteLine($"\t - Имя - {settings.Name}");
            Console.WriteLine($"\t - Адрес - {settings.Address}");
            Console.WriteLine($"\t - Тип данных - {settings.Data_Type}");
            Console.WriteLine($"\t - Тип регистра - {settings.Register_Type}");

            if (converter.ToBoolean(settings.Sinewave_Use))
            {
                Console.Write("\t");
                for (int i = 0; i < 70; i++) Console.Write('-');
                Console.Write("\n ");
                Console.WriteLine("\t Синусоиды:");
                Console.WriteLine("\t\t [Амплитуда, Период, Фаза]:");

                int count = converter.ToInt(settings.Sinewave_Count);
                for (int i = 0; i < count; i++)
                {
                    double amplitude = converter.ToDouble(settings.Sinewave_Amplitude);
                    double period = converter.ToDouble(settings.Sinewave_Period);
                    double phase = converter.ToDouble(settings.Sinewave_Phase);
                    generator.generators.Add(new Sinewave(amplitude, period, phase));
                    Console.WriteLine($"\t - {i+1} - [{amplitude}, {period}, {phase}]");
                }
            }

            if (converter.ToBoolean(settings.Randwalk_Use))
            {
                Console.Write("\t");
                for (int i = 0; i < 70; i++) Console.Write('-');
                Console.WriteLine("\n\t Случайное блуждание:");

                double start = converter.ToDouble(settings.Randwalk_Start);
                double factor = converter.ToDouble(settings.Randwalk_Factor);
                generator.generators.Add(new Randwalk(start, factor, rand.Next()));

                Console.WriteLine($"\t - Start - {start}");
                Console.WriteLine($"\t - Factor - {factor}");
            }

            if (converter.ToBoolean(settings.Trend_Use))
            {
                Console.Write("\t");
                for (int i = 0; i < 70; i++) Console.Write('-');
                Console.WriteLine("\n\t Тренд:");

                double slope = converter.ToDouble(settings.Trend_Slope);
                double zero = converter.ToDouble(settings.Trend_Zero);
                generator.generators.Add(new Trend(slope, zero));

                Console.WriteLine($"\t - Slope - {slope}");
                Console.WriteLine($"\t - Zero - {zero}");
            }

            return generator;
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

        public Signal(Generation_Settings settings, DataGenerator generator)
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
