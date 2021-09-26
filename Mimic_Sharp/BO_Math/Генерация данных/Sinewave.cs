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
        double amplitude;
        double period;
        double phase;
        double TS;

        // #################################################################################

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Sinewave"/>
        /// </summary>
        /// <param name="amplitude">Амплитуда. Сдвиг по оси Y</param>
        /// <param name="period">Период. Растяжение по оси X</param>
        /// <param name="phase">Фаза. Сдвиг по оси X</param>
        public Sinewave(double amplitude, double period, double phase)
        {
            this.amplitude = amplitude;
            this.period = period;
            this.phase = phase;
            TS = 0;
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = amplitude * Math.Sin(2 * Math.PI * TS / period + phase);
            TS += 1;
            return value;
        }

    }
}
