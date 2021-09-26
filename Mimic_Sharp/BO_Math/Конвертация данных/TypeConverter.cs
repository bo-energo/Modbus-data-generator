using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO_Math
{
    public static class TypeConverter
    {

        // #################################################################################

        #region int32 to uint16 | uint16 to int32

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort[] I32_to_UI16(int[] data)
        {
            int size = data.Length * 2;
            ushort[] pack = new ushort[size];

            // Упаковка
            for (int i = 0, j = 0; i < size; i += 2, j++)
            {
                byte[] bites = BitConverter.GetBytes(data[j]);

                pack[i] = BitConverter.ToUInt16(bites, 0);
                pack[i+1] = BitConverter.ToUInt16(bites, 2);

                //pack[i] = (ushort)(data[j] >> 16);
                //pack[i + 1] = (ushort)(data[j] & 0xffff);
            }

            return pack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort[] P_to_UI16(int[] data)
        {
            int size = data.Length / 4;
            ushort[] pack = new ushort[size];

            // Упаковка
            for (int i = 0, j = 0; i < size; i++, j += 4)
            {
                ushort x1 = (ushort)(data[j + 0] << 12);
                ushort x2 = (ushort)(data[j + 1] << 8);
                ushort x3 = (ushort)(data[j + 2] << 4);
                ushort x4 = (ushort)(data[j + 3]);
                pack[i] = (ushort)(x1 + x2 + x3 + x4);
            }

            return pack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] UI16_to_I32(ushort[] data)
        {
            int size = data.Length / 2;
            int[] unpack = new int[size];

            // Распаковка
            for (int i = 0, j = 0; i < size; i++, j += 2)
            {
                unpack[i] = (data[j + 0] << 16) + (data[j + 1]);
            }

            return unpack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[] UI16_to_P(ushort[] data)
        {
            int size = data.Length * 4;
            int[] unpack = new int[size];

            // Распаковка
            for (int i = 0, j = 0; i < data.Length; i++, j += 4)
            {
                unpack[j + 0] = (data[i] & 61440) >> 12;
                unpack[j + 1] = (data[i] & 3840) >> 8;
                unpack[j + 2] = (data[i] & 240) >> 4;
                unpack[j + 3] = (data[i] & 15);
            }

            return unpack;
        }

        #endregion int32 to uint16 | uint16 to int32

        // #################################################################################

        #region float32 to uint16 | uint16 to float32

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort[] F32_to_UI16(float[] data)
        {
            int size = data.Length * 2;
            ushort[] pack = new ushort[size];

            // Упаковка
            for (int i = 0, j = 0; i < size; i += 2, j++)
            {
                byte[] bites = BitConverter.GetBytes(data[j]);

                pack[i] = BitConverter.ToUInt16(bites, 0);
                pack[i+1] = BitConverter.ToUInt16(bites, 2);
            }

            return pack;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static float[] UI16_to_F32(ushort[] data)
        {
            int size = data.Length / 2;
            float[] unpack = new float[size];

            // Распаковка
            for (int i = 0, j = 0; i < size; i++, j += 2)
            {
                List<byte> bytes = new List<byte>();
                bytes.AddRange(BitConverter.GetBytes(data[j]));
                bytes.AddRange(BitConverter.GetBytes(data[j+1]));
                unpack[i] = BitConverter.ToSingle(bytes.ToArray(), 0);
            }

            return unpack;
        }

        #endregion float32 to ushort | ushort to float32

    }
}
