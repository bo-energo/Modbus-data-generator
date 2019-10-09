# -*- coding: utf-8 -*-

"""
Автор: Альфред Дж. Киттелл

Организация: ООО «БО-Энерго», ООО «МТК Бизнес.Оптима»

Россия г.Москва 2019

------------------------------

Модуль для эмитации прибора по протоколу modbus
"""

# Стандартные и системные модули
import os
import sys
import struct
import socket
import time as t
from datetime import datetime
from copy import deepcopy
from ctypes import *

# Модули для работы с данными
import json
import numpy as np

# Протокол Modbus
import modbus_tk
from modbus_tk import modbus_tcp


### Эмулятор прибора
class Mimic:
    """
    Эмулирует работу прибора по протоколу modbus
    """

    def __init__(self, config='config.json', logPath='log.txt'):
        """
        Конфиг:
         * Если указан путь (str), читает файл и заменяет прочитанное.
           Если путь не существует или не удалось прочитать,
           использует настройки по умолчанию

         * Если указан словарь настроек (dict), заменяет указанное

         * Иначе, использует настройки по умолчанию

        Лог:
         * Если указан путь (str), дублирует консольный вывод в файл
           Не дублирует серверный вывод (например: подключение, отключение, ошибки мастера...)

         * Иначе вывод только в консоль

        :param config: Путь к файлу конфигурации или словарь с настройками
        :type config: dict, str
        :param str logPath: Путь к логу

        """

        # handle консоли
        self.stdHandle = windll.Kernel32.GetStdHandle(c_ulong(0xfffffff5))

        # параметры лога
        self.logPath = logPath

        # коды ошибок
        self.ERROR = {
            'NO_CONFIG': 'Ошибка при чтении файла конфигурации!\n'
                         '\t Будут использованы параметры по умолчанию!',
            'READ_FAIL': 'Не удалось прочитать файл -',
            # генерация параметров
            'RAND_FAIL': 'Не удалось сгенерировать параметры модели!\n'
                         '\t Проверьте ввод параметров генерации\n'
                         '\t  - 1) "параметр": число,\t\t\t - фиксированый параметр\n'
                         '\t  - 2) "параметр": [число, число],\t - диапазон\n'
                         '\t  Текст ошибки -',
            # неправильный тип границ генерации
            'TYPE_FAIL': ' не может быть использован для указания границ генерации!'
        }

        # типы блоков для регистров
        self.BlockType = {
            # modbus exception codes
            "ILLEGAL_FUNCTION": 1,
            "ILLEGAL_DATA_ADDRESS": 2,
            "ILLEGAL_DATA_VALUE": 3,
            "SLAVE_DEVICE_FAILURE": 4,
            "COMMAND_ACKNOWLEDGE": 5,
            "SLAVE_DEVICE_BUSY": 6,
            "MEMORY_PARITY_ERROR": 8,
            # supported modbus functions
            "READ_COILS": 1,
            "READ_DISCRETE_INPUTS": 2,
            "READ_HOLDING_REGISTERS": 3,
            "READ_INPUT_REGISTERS": 4,
            "WRITE_SINGLE_COIL": 5,
            "WRITE_SINGLE_REGISTER": 6,
            "READ_EXCEPTION_STATUS": 7,
            "DIAGNOSTIC": 8,
            "REPORT_SLAVE_ID": 17,
            "WRITE_MULTIPLE_COILS": 15,
            "WRITE_MULTIPLE_REGISTERS": 16,
            "READ_WRITE_MULTIPLE_REGISTERS": 23,
            "DEVICE_INFO": 43,
            # supported block types
            "COILS": 1,
            "DISCRETE_INPUTS": 2,
            "HOLDING_REGISTERS": 3,
            "ANALOG_INPUTS": 4
        }

        # лог сервера
        messFormat = "### [%(asctime)s]: %(message)s\n"
        self.logger = modbus_tk.utils.create_logger(name="console", record_format=messFormat)

        # информация о сборке
        self._info_base()

        # конфиг
        self.serverSet, self.genSet = self._create_config(config)
        self._gen_params()

    ## Запуск сервера
    def run(self):

        # настройки
        address = self.serverSet['address']
        blockType = self.serverSet['register_type']
        start = self.serverSet['write_start']

        # расчёт времени
        time = self.serverSet['gen_period']
        period = time[2] + time[1] * 60 + time[0] * 3600
        time = self.serverSet['gen_end']
        timeEnd = time[2] + time[1] * 60 + time[0] * 3600

        # сервер
        server = modbus_tcp.TcpServer()
        server.start()
        self._info_server()

        # генераторы
        gens = [self._data_generator(genSet, period) for genSet in self.genSet]

        # устройство
        slave_1 = server.add_slave(address)
        slave_1.add_block('0', self.BlockType[blockType], start, len(gens) * 2)
        slave = server.get_slave(address)
        self._info_generate()

        # выполнение
        self._loop(slave, gens, period, timeEnd)
        server.stop()

    ## Цикл выполнения
    def _loop(self, slave, gens, period, timeEnd):
        """
        Цикл работы

        :param slave: Устройство
        :param gens: Генераторы сигналов
        :param period: Период генерации
        :param timeEnd: Время остановки
        :return:
        """

        timeStart = datetime.now()  # точка запуска
        i = 0
        while True:
            orgVal = [round(gen.__next__(), 2) for gen in gens]
            convVal = []
            for val in orgVal:
                convVal.extend(self._float_converter(val))
            slave.set_values('0', 0, convVal)

            mess = f'Запись № {i}\n' \
                   f'\t - Оригинальные данные [{len(orgVal)}]: {orgVal}\n' \
                   f'\t - Записанные в регистры [{len(convVal)}]: {convVal}'
            self._trace(mess, 11, 'REC')

            timeNow = datetime.now()
            timePass = timeNow - timeStart

            if timePass.seconds >= timeEnd:
                self._trace('Время работы сервера закончилось...', 2, 'END')

                break

            i += 1
            t.sleep(period)  # пауза на 5 секунд

    ## Создание конфига
    def _create_config(self, config=''):
        """
        Генерирует настройки сервера и генерации.

        :param config: Путь к файлу конфигурации или словарь с настройками
        :type config: dict, str
        """

        ## Чтение и замещение параметров из файла
        def read_set():
            self._trace('Чтение файла конфигурации', 6, 'READ')

            try:
                with open(config, "r") as read_file:
                    data = json.load(read_file)
            except Exception as err:
                errMess = f'{self.ERROR["READ_FAIL"]} {config}\n' \
                          f'\t Текст ошибки - {err}\n' \
                          f'\t Будут использованы настройки по умолчанию'
                self._trace(errMess, 14, 'WARN')
                return {}

            # проверка корректности файла конфигурации
            if not data:
                self._trace(f'{self.ERROR["NO_CONFIG"]}', 14, 'ERROR')

            return data

        ## Финализация настроек
        def finalization_set(data):
            # замещение параметров сервера на прочитаные значения
            sSet = deepcopy(serverSet)
            for item in data['server_set'].items():
                if item[0] in serverSet:
                    sSet[item[0]] = item[1]

            # клонирование шаблонов
            if data.get('generation_set'):
                patterns = data['generation_set']
            else:
                patterns = generationSet
            newGenSet = []
            for pat in patterns:
                [newGenSet.append(deepcopy(pat))
                 for _ in range(pat['patterns_followed'])]

            return sSet, newGenSet

        # настройки сервера
        serverSet = {
            "address": 1,
            "write_start": 0,
            "data_type": "float32",
            "register_type": "HOLDING_REGISTERS",
            "gen_period": [0, 0, 20],
            "gen_end": [48, 0, 0]
        }
        # настройки генерации
        generationSet = [
            {
                "patterns_followed": 11,

                "harm_use": [0, 2],
                "harm_count": [2, 11],
                "harm_amplitude": [0.0, 10.0],
                "harm_period_sec": [10.0, 250.0],
                "harm_phase": [0.0, 100.0],

                "randWalk_use": [0, 2],
                "randWalk_start": [0.0, 2.0],
                "randWalk_factor": [0.0, 2.0],

                "trend_use": [0, 2],
                "trend_slope": [0.1, 1.0],
                "trend_zero": [0.0, 2.0]
            }
        ]

        newSet = False
        # чтение параметров из файла
        if isinstance(config, str):
            newSet = read_set()
        # получение параметров из кода
        if isinstance(config, dict):
            newSet = config
        # использование по умолчанию
        if not newSet:
            newSet = {'server_set': serverSet, 'generation_set': generationSet}
        serverSet, generationSet = finalization_set(newSet)

        return serverSet, generationSet

    ## Генерация параметров
    def _gen_params(self):

        ## Генерация значения параметру
        def get_random_value(value):
            if isinstance(value, list):
                if isinstance(value[0], int):
                    return np.random.randint(value[0], value[1])
                elif isinstance(value[0], float):
                    return np.random.uniform(value[0], value[1])
                else:
                    errMess = f"Тип: {type(value[0])} или {type(value[1])} - " \
                              f"{self.ERROR['TYPE_FAIL']}"
                    self._trace(errMess, 12, 'ERROR')
                    sys.exit()
            else:
                return value

        ## Генерация компонентов
        def gen_components(genSet):
            # параметры генерации
            while True:
                harm_use = get_random_value(genSet['harm_use'])
                randWalk_use = get_random_value(genSet['randWalk_use'])
                trend_use = get_random_value(genSet['harm_use'])
                # перебросить, если нечего отдавать
                if not (trend_use + harm_use + randWalk_use):
                    continue
                # понижение вероятности "голого" тренда
                if not (harm_use + randWalk_use):
                    mercy = np.random.randint(0, 11)
                    if mercy < 9:
                        continue

                genSet['harm_use'] = harm_use
                genSet['randWalk_use'] = randWalk_use
                genSet['randWalk_start'] = get_random_value(genSet['randWalk_start'])
                genSet['randWalk_factor'] = get_random_value(genSet['randWalk_factor'])
                genSet['trend_use'] = trend_use
                genSet['trend_slope'] = get_random_value(genSet['trend_slope'])
                genSet['trend_zero'] = get_random_value(genSet['trend_zero'])

                return

        ## Генерация параметров гармоник
        def gen_harm_set(genSet):
            harm_set = []
            harm_count = np.random.randint(2, 11)
            for i in range(harm_count):
                harm_amplitude = get_random_value(genSet['harm_amplitude'])
                harm_period = get_random_value(genSet['harm_period_sec'])
                harm_phase = get_random_value(genSet['harm_phase'])
                harm_set.append([harm_amplitude, harm_period, harm_phase])
            genSet['harm_set'] = harm_set

        # генерация значений
        try:
            for reg in self.genSet:
                gen_components(reg)
                gen_harm_set(reg)
        except Exception as err:
            self._trace(f'{self.ERROR["RAND_FAIL"]} {err}', 12, 'ERROR')
            sys.exit()

    ## Преобразование float в несколько int (без потери дробной части)
    def _float_converter(self, x):
        if self.serverSet['data_type'] == 'float16':  # x16 в 1x16
            xx = min(max(x, 0), 65535)
            return struct.unpack('H', struct.pack('e', xx))
        if self.serverSet['data_type'] == 'float32':  # x32 в 2x16
            xx = min(max(x, 0), 4294967295)
            return struct.unpack('HH', struct.pack('f', xx))[::-1]
        return [0]

    ## Вывод информации по сборке
    def _info_base(self):
        version = '0.1'
        build = '2019-09-30'

        mess = f'Информация о сборке:\n' \
               f'\t - Версия: {version}\n' \
               f'\t - Билд: {build}'

        self._trace(mess, 2, 'INFO')

    ## Вывод информации по серверу
    def _info_server(self):
        ip = socket.gethostbyname(socket.gethostname())

        mess = f'Сервер запущен...\n' \
               f'\t Информация о сервере:\n' \
               f'\t   - IP: {ip}\n' \
               f'\t   - Адрес устройства: {self.serverSet["address"]}\n' \
               f'\t   - Начальный регистр: {self.serverSet["write_start"]}\n' \
               f'\t   - Тип данных: {self.serverSet["data_type"]}\n' \
               f'\t   - Тип регистра: {self.serverSet["register_type"]}\n' \
               f'\t   - Период генерации: {self.serverSet["gen_period"]}\n' \
               f'\t   - Время работы: {self.serverSet["gen_end"]}'

        self._trace(mess, 2, 'INFO')

    ## Вывод информации по серверу
    def _info_generate(self):
        mess = f'Устройство начинает работу...\n' \
               f'\t Информация о генерации:'
        for i, param in enumerate(self.genSet, 1):
            mess += f'\n\n\t {"-"*10} Сигнал № {i}'
            if param['harm_use']:
                mess += f'\n\t   Гармоники:'
                for j, h in enumerate(param['harm_set'], 1):
                    mess += f'\n\t\t {j}: {h}'
            if param['randWalk_use']:
                mess += f'\n\t   Random Walk:'
                mess += f'\n\t\t "start": {param["randWalk_start"]}'
                mess += f'\n\t\t "factor": {param["randWalk_factor"]}'
            if param['trend_use']:
                mess += f'\n\t   Тренд:'
                mess += f'\n\t\t "slope": {param["trend_slope"]}'
                mess += f'\n\t\t "zero": {param["trend_zero"]}'

        self._trace(mess, 2, 'INFO')

    ## Вывод сообщения
    def _trace(self, text, color=13, typeMess='INFO', logRecord=True):
        """
        Трасировка сообщений в консоль и дублирование в лог

        :param text: Текст для записи
        :param int color: номер цвета отображения в консоли
        :param str typeMess: Тип сообщения
        :param bool logRecord: Дублировать в лог
        """

        ## Настройка цвета
        def set_color(_color=7):
            """
            Задание цвета вывода консоли.
            Применяется для последующего вывода в консоль.
            Покрась свою консоль в нескучные цвета!!

            Цвета:
             - 1 - Темно-синий
             - 2 - Зелёный
             - 3 - Светло-синий
             - 4 - Красный
             - 5 - Фиолетовый
             - 6 - Золотой
             - 7 - Белый (блекло-белый)
             - 8 - Серый
             - 9 - Синий
             - 10 - Светло-зелёный
             - 11 - Сиреневый (светло-голубенький)
             - 12 - Светло-красный
             - 13 - Светло-фиолетовый
             - 14 - Светло-желтый
             - 15 - Бело-белый (ярко-белый)

            :param _color: Нужный цвет
            """

            windll.Kernel32.SetConsoleTextAttribute(self.stdHandle, _color)

        ## вывод в консоль
        def color_print():
            set_color(color)
            self.logger.info(text)
            set_color()

        ## запись в лог
        def record_to_log():
            out_text = f'\n### [{datetime.now()}][{typeMess}]: {text}\n'
            self._write_to_file(out_text, self.logPath, 'a')

        color_print()

        if self.logPath and logRecord:
            record_to_log()

    ## Запись данных в файл (универсальный)
    def _write_to_file(self, data, path, mode='w'):
        """
        Запись полученных данные в файл

        Модификаторы чтения:
         * r   - чтение, указатель в начале
         * rb  - тоже самое только в бинарном виде
         * r+  - чтение и запись, указатель в начале
         * rb+ - тоже самое только в бинарном виде

        Модификаторы записи:
         * w   - запись, указатель в начале, создаёт файл
         * wb  - тоже самое только в бинарном виде
         * w+  - запись и чтение, указатель в начале, создаёт файл
         * wb+ - тоже самое только в бинарном виде

        Модификаторы добавления:
         * a   - добавление, указатель в конце, создаёт файл
         * ab  - тоже самое только в бинарном виде
         * a+  - добавление и чтение, указатель в конце, создаёт файл
         * ab+ - тоже самое только в бинарном виде

        :param str  data: Данные для записи
        :param str path: Путь файла для записи
        :param str mode: Модификатор открытия файла
        """

        base = os.path.dirname(path)
        if base and not os.path.exists(base):
            self._create_dir(base)

        with open(path, mode) as f:
            f.write(data)

    ## Генератор данных
    @staticmethod
    def _data_generator(genSet, period):

        # гармоники
        harm_use = genSet['harm_use']
        harm_set = genSet['harm_set']

        # случайная часть
        randWalk_use = genSet['randWalk_use']
        randWalk_factor = genSet['randWalk_factor']
        randWalk_start = genSet['randWalk_start']

        # тренд
        trend_use = genSet['trend_use']
        trend_slope = genSet['trend_slope']
        trend_zero = genSet['trend_zero']

        # генерация данных
        TS = 0
        while True:
            x = 0
            if harm_use:
                x += np.sum([s[0]*np.sin(2*np.pi*period*TS / s[1] + 2*np.pi / 100*s[2])
                             for s in harm_set])
            if randWalk_use:
                x += randWalk_start + randWalk_factor * (np.random.rand() - 0.5)
            if trend_use:
                x += trend_slope * TS + trend_zero
            yield x
            TS += 1

    ## Создание папок вывода
    @staticmethod
    def _create_dir(path):
        """
        Создаёт полный указанный путь,
        если его не существет

        :param str path: путь для создания
        """

        if not os.path.exists(path):
            os.makedirs(path)


if __name__ == "__main__":

    # пример настроек
    testConfig = {
        # Настройки сервера
        "server_set": {
            "address": 1,                           # адрес устройства
            "write_start": 0,                       # начальный регистр
            "data_type": "float32",                 # тип записываемых данных
            "register_type": "HOLDING_REGISTERS",   # тип регистра (блока)
            "gen_period": [0, 0, 20],               # период генерации (интервал между записями)
            "gen_end": [666, 0, 0]                  # время работы сервера
            # время задаётся: [часов, минут, секунд]
        },
        # Настройки генерации
        "generation_set": [
            # шаблон настроек сигнала
            {
                "patterns_followed": 1,            # число сигналов по шаблону
                # Настройки гармоник
                "harm_use": [0, 2],                 # добавлять к сигналу гармоники
                "harm_count": [2, 11],              # число гармоник
                "harm_amplitude": [0.0, 10.0],      #
                "harm_period_sec": [10.0, 250.0],   #
                "harm_phase": [0.0, 100.0],         #
                # Настройки случайной части
                "randWalk_use": [0, 2],             # добавлять к сигналу случайную часть
                "randWalk_start": [0.0, 2.0],       # начальный шаг
                "randWalk_factor": [0.0, 2.0],      # коэффициент шага
                # Настройки тренда
                "trend_use": [0, 2],                # добавлять к сигналу тренд
                "trend_slope": [0.1, 1.0],          #
                "trend_zero": [0.0, 2.0]            #
            }
        ]
    }

    # Использование настроек по умолчанию
    # mimic = Mimic()

    # Использование настроек из кода
    # mimic = Mimic(config=testConfig, logPath='log.txt')

    # Использование настроек из файла
    mimic = Mimic(config='config.json', logPath='log.txt')

    # Запуск
    mimic.run()
    os.system("pause")
