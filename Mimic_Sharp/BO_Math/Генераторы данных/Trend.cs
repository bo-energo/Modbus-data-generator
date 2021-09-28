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
        private double TS = 0;

        /// <summary>
        /// Коэффициент роста
        /// </summary>
        public double Slope { get; set; } = 0.1;
        /// <summary>
        /// Константа
        /// </summary>
        public double Zero { get; set; } = 10;

        // #################################################################################

        /// <summary>
        /// Возвращает параметры генератора
        /// </summary>
        /// <returns>Список параметров</returns>
        public override string[] GetParams()
        {
            List<string> paramList = new List<string>();

            paramList.Add("Trend");
            paramList.Add("Slope");
            paramList.Add(Slope.ToString());
            paramList.Add("Zero");
            paramList.Add(Zero.ToString());

            return paramList.ToArray();
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = Slope * TS + Zero;
            TS += 1;
            return value;
        }

    }
}
