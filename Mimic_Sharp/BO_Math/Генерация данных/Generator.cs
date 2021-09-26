using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    /// <summary>
    /// Базовый класс для генераторов
    /// </summary>
    public abstract class Generator
    {
        /// <summary>
        /// Генерация следующего значения
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public virtual double Next()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация нескольких значений
        /// </summary>
        /// <param name="count">Число значений на генерацию</param>
        /// <returns>Массив сгенерированных значений</returns>
        public virtual double[] Next(int count)
        {
            double[] values = new double[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = Next();
            }

            return values;
        }
    }
}
