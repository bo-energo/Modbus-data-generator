using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    /// <summary>
    /// Генератор тренда
    /// </summary>
    public class Trend : Generator
    {
        double slope;
        double zero;
        double TS;

        // #################################################################################

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Trend"/>
        /// </summary>
        /// <param name="slope">Коэффициент роста</param>
        /// <param name="zero">Константа</param>
        public Trend(double slope, double zero)
        {
            this.slope = slope;
            this.zero = zero;
            TS = 0;
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = slope * TS + zero;
            TS += 1;
            return value;
        }

    }
}
