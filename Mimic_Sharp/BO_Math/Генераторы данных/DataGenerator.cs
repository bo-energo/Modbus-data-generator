using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{

    /// <summary>
    /// Класс для объединения множества генераторов данных
    /// </summary>
    public class DataGenerator : Generator
    {
        /// <summary>
        /// Список генераторов для обхода
        /// </summary>
        public List<Generator> Generators { get; set; } = new List<Generator>();

        // #################################################################################

        /// <summary>
        /// Генерация следующего значения. Обходит все генераторы в списке
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = 0;

            foreach (var generator in Generators)
            {
                value += generator.Next();
            }

            return value;
        }
    }
}
