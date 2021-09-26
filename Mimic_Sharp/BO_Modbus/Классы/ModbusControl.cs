using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

using Modbus.Device;
using Modbus.Data;
using System.Collections;

namespace BO_Modbus
{
    /// <summary>
    /// 
    /// </summary>
    public class TcpSlave
    {
        TcpListener slaveTcpListener;
        IPAddress address;
        int port;

        ModbusTcpSlave slave;
        byte slaveID;

        // #################################################################################

        /// <summary>
        /// Инициализирует новый экземпляр класса обёртки <see cref="TcpSlave"/>
        /// для класса <see cref="ModbusTcpSlave"/>
        /// </summary>
        /// <param name="ip">IP Адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="slaveID">Адрес slave</param>
        public TcpSlave(string ip, int port = 502, byte slaveID = 1)
        {
            address = IPAddress.Parse(ip);
            this.port = port;
            this.slaveID = slaveID;
        }

        /// <summary>
        /// Запускает slave-client
        /// </summary>
        public void Start()
        {
            // Создание и запуск TCP-соединения
            slaveTcpListener = new TcpListener(address, port);
            slaveTcpListener.Start();

            // Создание и запуск slave-client
            slave = ModbusTcpSlave.CreateTcp(slaveID, slaveTcpListener);
            slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            slave.Listen();
        }

        /// <summary>
        /// Останавливает slave-client
        /// </summary>
        public void Stop()
        {
            slaveTcpListener.Stop();
            slave.Dispose();
        }

        // #################################################################################

        #region Запись данных

        /// <summary>
        /// Записывает значение по адресу в указанную таблицу.
        /// Для целочисленных значений
        /// </summary>
        /// <param name="ind">Адрес записи</param>
        /// <param name="value">Значение</param>
        /// <param name="type">Тип таблицы (куда записать)</param>
        public void Write(int ind, ushort value, RegistersTypes type = RegistersTypes.Holding)
        {
            switch(type)
            {
                case RegistersTypes.Holding:
                    slave.DataStore.HoldingRegisters[ind] = value;
                    break;
                 case RegistersTypes.Input:
                    slave.DataStore.InputRegisters[ind] = value;
                    break;
            }
        }

        /// <summary>
        /// Записывает все значения из массива c начального адреса в указанную таблицу.
        /// Для целочисленных значений
        /// </summary>
        /// <param name="ind">Адрес начала записи</param>
        /// <param name="values">Массив со значениями</param>
        /// <param name="type">Тип таблицы (куда записать)</param>
        public void Write(int ind, ushort[] values, RegistersTypes type = RegistersTypes.Holding)
        {
            switch (type)
            {
                case RegistersTypes.Holding:
                    for (int i = 0; i < values.Length; i++)
                        slave.DataStore.HoldingRegisters[ind + i] = values[i];
                    break;
                case RegistersTypes.Input:
                    for (int i = 0; i < values.Length; i++)
                        slave.DataStore.InputRegisters[ind + i] = values[i];
                    break;
            }
        }

        /// <summary>
        /// Записывает значение по адресу в указанную таблицу.
        /// Для флагов
        /// </summary>
        /// <param name="ind">Адрес записи</param>
        /// <param name="value">Значение</param>
        /// <param name="type">Тип таблицы (куда записать)</param>
        public void Write(int ind, bool value, DiscretesTypes type = DiscretesTypes.Coil)
        {
            switch (type)
            {
                case DiscretesTypes.Coil:
                    slave.DataStore.CoilDiscretes[ind] = value;
                    break;
                case DiscretesTypes.Input:
                    slave.DataStore.InputDiscretes[ind] = value;
                    break;
            }
        }

        /// <summary>
        /// Записывает все значения из массива c начального адреса в указанную таблицу.
        /// Для флагов
        /// </summary>
        /// <param name="ind">Адрес начала записи</param>
        /// <param name="values">Массив со значениями</param>
        /// <param name="type">Тип таблицы (куда записать)</param>
        public void Write(int ind, bool[] values, DiscretesTypes type = DiscretesTypes.Coil)
        {
            switch (type)
            {
                case DiscretesTypes.Coil:
                    for (int i = 0; i < values.Length; i++)
                        slave.DataStore.CoilDiscretes[ind + i] = values[i];
                    break;
                case DiscretesTypes.Input:
                    for (int i = 0; i < values.Length; i++)
                        slave.DataStore.InputDiscretes[ind + i] = values[i];
                    break;
            }
        }

        #endregion Запись данных

        // #################################################################################


    }

    /// <summary>
    /// 
    /// </summary>
    public class TcpMaster
    {
        TcpClient client;
        string address;
        int port;

        ModbusIpMaster master;
        byte slaveID;

        // -----------------------------------------------------------------------

        /// <summary>
        /// Инициализирует новый экземпляр класса обёртки <see cref="TcpMaster"/>
        /// для класса <see cref="ModbusIpMaster"/>
        /// </summary>
        /// <param name="ip">IP Адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="slaveID">Адрес slave</param>
        public TcpMaster(string ip, int port = 502, byte slaveID = 1)
        {
            address = ip;
            this.port = port;
            this.slaveID = slaveID;
        }

        /// <summary>
        /// Запускает slave-client
        /// </summary>
        public void Start()
        {
            // Создание и запуск TCP-соединения
            client = new TcpClient(address, port);

            // Создание и запуск slave-client
            master = ModbusIpMaster.CreateIp(client);
        }

        /// <summary>
        /// Останавливает slave-client
        /// </summary>
        public void Stop()
        {
            client.Close();
            master.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="address"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public ushort[] Read(ushort address, ushort points)
        {
            if (points <= 125)
            {
                return master.ReadInputRegisters(slaveID, address, points);
            }

            else
            {
                ushort[] result = new ushort[points];

                int portion = points / 125;
                int remains = points - (portion * 125);

                for (int n = 0; n < portion; n++)
                {
                    ushort startAddress = (ushort)(125 * n);
                    ushort[] inputs = master.ReadInputRegisters(slaveID, (ushort)(address + startAddress), 125);

                    for (int i = 0; i < 125; i++)
                    {
                        result[startAddress + i] = inputs[i];
                    }
                }

                if (remains != 0)
                {
                    ushort startAddress = (ushort)(125 * portion);
                    ushort[] inputs = master.ReadInputRegisters(slaveID, (ushort)(address + startAddress), (ushort)remains);

                    for (int i = 0; i < remains; i++)
                    {
                        result[startAddress + i] = inputs[i];
                    }
                }

                return result;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RtuMaster
    {

        public string PortName { get; set; } = "COM1";
        public int BauldRate { get; set; } = 38400;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;
        public int Timeout { get; set; } = 1000;

        bool[] alarm = new bool[10];

        SerialPort serialPort;
        ModbusSerialMaster master;

        // -----------------------------------------------------------------------

        /// <summary>
        /// пустой
        /// </summary>
        public RtuMaster()
        {

        }

        /// <summary>
        /// Запускает slave-client
        /// </summary>
        public void Start()
        {
            serialPort = new SerialPort
            {
                PortName = PortName,
                BaudRate = BauldRate,
                DataBits = DataBits,
                Parity = Parity,
                StopBits = StopBits
            };

            serialPort.Open();
            serialPort.WriteTimeout = Timeout;
            serialPort.ReadTimeout = Timeout;

            master = ModbusSerialMaster.CreateRtu(serialPort);
        }

        /// <summary>
        /// Останавливает slave-client
        /// </summary>
        public void Stop()
        {
            serialPort.Close();
            master.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="address"></param>
        /// <param name="value"></param>
        public void Write(byte slaveID, ushort address, ushort value)
        {
            master.WriteSingleRegisterAsync(slaveID, address, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="address"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        public ushort[] Read(byte slaveID, ushort address, ushort points)
        {
            ushort[] result = master.ReadHoldingRegisters(slaveID, address, points);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveID"></param>
        /// <param name="bits"></param>
        public void SetAlarm(byte slaveID, bool[] bits)
        {
            bits = bits ?? new bool[] { };

            Array.Copy(bits, alarm, bits.Length);

            alarm[4] = alarm[0];
            alarm[5] = alarm[1];
            alarm[6] = alarm[2];

            BitArray a = new BitArray(alarm);
            byte[] bytes = new byte[16];
            a.CopyTo(bytes, 0);

            ushort value = BitConverter.ToUInt16(bytes, 0);

            Write(slaveID, 2, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slaveID"></param>
        /// <returns></returns>
        public bool[] GetAlarm(byte slaveID)
        {
            ushort value = Read(slaveID, 2, 1)[0];
            string x2 = Convert.ToString(value, 2).PadLeft(10, '0');

            char[] inputarray = x2.ToCharArray();
            Array.Reverse(inputarray);
            x2 = new string(inputarray);

            for (int i = 0; i < 10; i++)
            {
                alarm[i] = x2[i] == '1' ? true : false;
            }

            return alarm;
        }
    }

    // -----------------------------------------------------------------------

    /// <summary>
    /// Типы целочисленных регистров
    /// </summary>
    public enum RegistersTypes
    {
        /// <summary>
        /// Регистры хранения
        /// </summary>
        Holding = 0,
        /// <summary>
        /// Регистры ввода
        /// </summary>
        Input = 1
    }

    /// <summary>
    /// Типы флаговых регистров
    /// </summary>
    public enum DiscretesTypes
    {
        /// <summary>
        /// Регистры флагов
        /// </summary>
        Coil = 0,
        /// <summary>
        /// Дискретные входы
        /// </summary>
        Input = 1
    }

}
