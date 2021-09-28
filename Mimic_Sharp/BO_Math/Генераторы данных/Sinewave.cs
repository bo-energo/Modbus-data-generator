using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    /// <summary>
    /// Генератор синусоиды
    /// </summary>
    public class Sinewave : Generator
    {
        private double TS = 0;

        /// <summary>
        /// Амплитуда. Сдвиг по оси Y
        /// </summary>
        public double Amplitude { get; set; } = 50;
        /// <summary>
        /// Период. Растяжение по оси X
        /// </summary>
        public double Period { get; set; } = 100;
        /// <summary>
        /// Фаза. Сдвиг по оси X
        /// </summary>
        public double Phase { get; set; } = 0;

        // #################################################################################

        /// <summary>
        /// Возвращает параметры генератора
        /// </summary>
        /// <returns>Список параметров</returns>
        public override string[] GetParams()
        {
            List<string> paramList = new List<string>();

            paramList.Add("Sinewave");
            paramList.Add("Amplitude");
            paramList.Add(Amplitude.ToString());
            paramList.Add("Period");
            paramList.Add(Period.ToString());
            paramList.Add("Phase");
            paramList.Add(Phase.ToString());

            return paramList.ToArray();
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = Amplitude * Math.Sin(2 * Math.PI * TS / Period + Phase);
            TS += 1;
            return value;
        }

    }
}
