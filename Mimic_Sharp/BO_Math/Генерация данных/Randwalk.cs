using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    /// <summary>
    /// Генератор случайного блуждания
    /// </summary>
    public class Randwalk : Generator
    {
        double start;
        double factor;
        double last;
        Random rand;

        // #################################################################################

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Randwalk"/>
        /// </summary>
        /// <param name="start">Начальное положение</param>
        /// <param name="factor">Коэффициент шага</param>
        public Randwalk(double start, double factor, int seed)
        {
            this.start = start;
            this.factor = factor;
            last = start;
            rand = new Random(seed);
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            last += factor * (rand.NextDouble() - 0.5);
            return last;
            
            //return start + factor * (rand.NextDouble() - 0.5);
        }

    }
}
