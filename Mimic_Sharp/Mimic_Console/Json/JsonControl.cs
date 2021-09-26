using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mimic_Console
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
        Random rand = new Random();

        // #################################################################################

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
                    return rand.Next(list[0], list[1]);
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
                    return NextFloat(list[0], list[1]);
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
                    return Convert.ToBoolean(rand.Next(list[0], list[1]));
                }
                default:
                    return false;
            }
        }

        private double NextFloat(double min, double max)
        {
            double val = (rand.NextDouble() * (max - min) + min);
            return (double)val;
        }
    }
}
