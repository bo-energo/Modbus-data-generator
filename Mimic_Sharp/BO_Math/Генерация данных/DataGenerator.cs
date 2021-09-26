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
        public List<Generator> generators = new List<Generator>();

        // #################################################################################

        /// <summary>
        /// Генерация следующего значения. Обходит все генераторы в списке
        /// </summary>
        /// <returns>Сгенерированное значение</returns>
        public override double Next()
        {
            double value = 0;

            foreach (var generator in generators)
            {
                value += generator.Next();
            }

            return value;
        }
    }

}
