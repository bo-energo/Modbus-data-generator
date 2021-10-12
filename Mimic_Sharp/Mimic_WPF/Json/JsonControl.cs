using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using BO_Math;

namespace Mimic_WPF
{
    public class JsonControl
    {
        /// <summary>
        /// Сохраняет настройки в файл
        /// </summary>
        /// <param name="path">Путь сохранения</param>
        /// <param name="settings">Настройки</param>
        public static void SaveSettings(string path, Settings settings)
        {
            // Добавление отступов
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);

            using (StreamWriter stream = new StreamWriter(path))
            {
                stream.Write(json);
            }
        }

        /// <summary>
        /// Загружает настройки из файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Настройки</returns>
        public static Settings LoadSettings(string path)
        {
            if (!File.Exists(path))
            {
                return CreateSettings();
            }
            else
            {
                try
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string json = reader.ReadToEnd();
                        var settings = JsonSerializer.Deserialize<Settings>(json);

                        return settings;
                    }
                }
                catch
                {
                    return CreateSettings();
                }
            }
        }

        /// <summary>
        /// Генерирует дефолтные настройки
        /// </summary>
        /// <returns>Дефолтные настройки</returns>
        public static Settings CreateSettings()
        {
            Settings settings = new Settings();

            return settings;
        }

    }

    public class JsonConverter
    {
        public SuperRandom Rand { get; set; } = new SuperRandom();

        // #################################################################################

        public DataGenerator CreateGenerator(Component[] components)
        {
            DataGenerator generator = new DataGenerator();
            int exStage = 0;

            try
            {
                foreach (var component in components)
                {
                    exStage = 1;

                    var p = component.Params;
                    int count = p.ContainsKey("Count") ? ToInt(p["Count"]) : 1;
                    bool use = p.ContainsKey("Count") ? ToBoolean(p["Use"]) : true;

                    if (!use || count == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        switch (component.Name)
                        {
                            case "Sinewave":
                            {
                                exStage = 2;

                                var gen = new Sinewave
                                {
                                    Amplitude = ToDouble(p["Amplitude"]),
                                    Period = ToDouble(p["Period"]),
                                    Phase = ToDouble(p["Phase"]),
                                };
                                generator.Generators.Add(gen);

                                break;
                            }
                            case "Randwalk":
                            {
                                exStage = 3;

                                var gen = new Randwalk
                                {
                                    Start = ToDouble(p["Start"]),
                                    Factor = ToDouble(p["Factor"]),
                                    Rand = new Random(Rand.Next()),
                                };
                                generator.Generators.Add(gen);

                                break;
                            }
                            case "Trend":
                            {
                                exStage = 4;

                                var gen = new Trend
                                {
                                    Slope = ToDouble(p["Slope"]),
                                    Zero = ToDouble(p["Zero"]),
                                };
                                generator.Generators.Add(gen);

                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                string mess;
                switch(exStage)
                {
                    case 1:
                        mess = "Параметры компонента заданы некорректно!";
                        break;
                    case 2:
                        mess = "Параметры компонента 'Sinewave' заданы некорректно!";
                        break;
                    case 3:
                        mess = "Параметры компонента 'Randwalk' заданы некорректно!";
                        break;
                    case 4:
                        mess = "Параметры компонента 'Trend' заданы некорректно!";
                        break;
                    default:
                        mess = "Непредвиденная ошибка при чтении компонентов!";
                        break;
                }

                throw new ArgumentException(mess);
            }
            

            return generator;
        }

        public int ToInt(object value)
        {
            var val = (value as JsonElement?).Value;

            switch (val.ValueKind)
            {
                case JsonValueKind.Number:
                    return val.GetInt32();
                case JsonValueKind.Array:
                {
                    List<int> list = new List<int>();
                    foreach (var item in val.EnumerateArray())
                        list.Add(item.GetInt32());
                    return Rand.Next(list[0], list[1]);
                }
                default:
                    return 0;
            }
        }

        public double ToDouble(object value)
        {
            var val = (value as JsonElement?).Value;

            switch (val.ValueKind)
            {
                case JsonValueKind.Number:
                    return val.GetDouble();
                case JsonValueKind.Array:
                {
                    List<double> list = new List<double>();
                    foreach (var item in val.EnumerateArray())
                        list.Add(item.GetDouble());
                    return Math.Round(Rand.NextDouble(list[0], list[1]), 4);
                }
                default:
                    return double.NaN;
            }
        }

        public bool ToBoolean(object value)
        {
            if (value == null) return false;

            var val = (value as JsonElement?).Value;

            switch (val.ValueKind)
            {
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Number:
                    return Convert.ToBoolean(val.GetDouble());
                case JsonValueKind.Array:
                {
                    List<int> list = new List<int>();
                    foreach (var item in val.EnumerateArray())
                        list.Add(item.GetInt32());
                    return Convert.ToBoolean(Rand.Next(list[0], list[1]));
                }
                default:
                    return false;
            }
        }


    }
}
