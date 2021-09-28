using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    public class SuperRandom : Random
    {
        /// <summary>
        /// Возвращает случайное число с плавающей запятой,
        /// значение которой между нижней и верхней границей
        /// </summary>
        /// <param name="min">Нижняя граница</param>
        /// <param name="max">Верхняя граница</param>
        /// <returns>Число двойной точности с плавающей запятой,
        /// значение которой между нижней и верхней границей</returns>
        public double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }
    }
}
