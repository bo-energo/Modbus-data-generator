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
        private double start = 0;
        private double last = 0;

        /// <summary>
        /// Начальнаое значение
        /// </summary>
        public double Start { get => start; set => start = last; }
        /// <summary>
        /// Коэффициент шага
        /// </summary>
        public double Factor { get; set; } = 10;
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        public Random Rand { get; set; } = new Random();
        /// <summary>
        /// Предыдущее значение
        /// </summary>
        public double Last { get => last; }

        // #################################################################################

        /// <summary>
        /// Возвращает параметры генератора
        /// </summary>
        /// <returns>Список параметров</returns>
        public override string[] GetParams()
        {
            List<string> paramList = new List<string>();

            paramList.Add("Randwalk");
            paramList.Add("Start");
            paramList.Add(Start.ToString());
            paramList.Add("Factor");
            paramList.Add(Factor.ToString());

            return paramList.ToArray();
        }

        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            last += Factor * (Rand.NextDouble() - 0.5);
            return last;
        }

    }
}
