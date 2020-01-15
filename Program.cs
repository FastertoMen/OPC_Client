using System;
using System.Threading;
using System.Data.SQLite;
using System.IO;
using Modbus.Device;
using System.IO.Ports;

namespace OPC_Client
{    
    class Program
    {
        public class GlobalVariables
        {
            public static int totalDone { get; set; }
            public static string settingsFilePath = @"settings.txt";
            public static string logFilePath = @"log.txt";
        }
        static void Main(string[] args)
        {
            TitaniumAS.Opc.Client.Bootstrap.Initialize();            

            int incr = 0, sendRequestToAT1, sendRequestToAT2, sendRequestToAT3, sendRequestToAT4, sendRequestToAT5, totalAT;
            bool doneAT1, doneAT2, doneAT3, doneAT4, doneAT5, processGo = false;                      

            if (!File.Exists(GlobalVariables.settingsFilePath))
                CreateTXT(0);
            
            if (!File.Exists(GlobalVariables.logFilePath))
                CreateTXT(1);

            string[] settings = File.ReadAllLines(GlobalVariables.settingsFilePath, System.Text.Encoding.Default);

            string queryString = @"CREATE TABLE IF NOT EXISTS
                                    [MainTable](
                                    [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                    [RequestDate] DATETIME NOT NULL,
                                    [PeriodDateFrom] DATETIME NOT NULL,
                                    [PeriodDateTo] DATETIME NOT NULL,
                                    [AT_Number] NVARCHAR(10) NOT NULL,
                                    [ProductVolume] REAL NOT NULL)";

            ConnectToDB(queryString, 0);

            sendRequestToAT1 = Convert.ToInt32(settings[7]);
            sendRequestToAT2 = Convert.ToInt32(settings[8]);
            sendRequestToAT3 = Convert.ToInt32(settings[9]);
            sendRequestToAT4 = Convert.ToInt32(settings[10]);
            sendRequestToAT5 = Convert.ToInt32(settings[11]);
            totalAT = Convert.ToInt32(settings[7]) + Convert.ToInt32(settings[8]) + Convert.ToInt32(settings[9]) + Convert.ToInt32(settings[10]) + Convert.ToInt32(settings[11]);

            var dateArray = DateSetUp(settings, incr);
           
            while (true)
            {
                if (totalAT == GlobalVariables.totalDone)
                {
                    incr++;
                    dateArray = DateSetUp(settings, incr);
                    GlobalVariables.totalDone = 0;
                    processGo = false;
                }

                if ((DateTime.Now >= dateArray.Item2 && DateTime.Now <= dateArray.Item3) && (processGo == false))                
                {                    
                    doneAT1 = false;
                    doneAT2 = false;
                    doneAT3 = false;
                    doneAT4 = false;
                    doneAT5 = false;

                    if (sendRequestToAT1 == 1)
                    {
                        processGo = true;

                        new Thread(() =>
                        {
                            DoJob(doneAT1, dateArray.Item1[0], dateArray.Item1[1], dateArray.Item1[2], settings[12], Convert.ToByte(settings[17]),
                                                settings[6], dateArray.Item4, dateArray.Item5, "AT-1", 1);                            
                        }
                        ).Start();                       
                    }

                    if (sendRequestToAT2 == 1)
                    {
                        processGo = true;

                        new Thread(() =>
                        {
                            DoJob(doneAT2, dateArray.Item1[0], dateArray.Item1[1], dateArray.Item1[2], settings[13], Convert.ToByte(settings[18]),
                                                settings[6], dateArray.Item4, dateArray.Item5, "AT-2", 2);
                        }
                        ).Start();                        
                    }

                    if (sendRequestToAT3 == 1)
                    {
                        processGo = true;

                        new Thread(() =>
                        {
                           DoJob(doneAT3, dateArray.Item1[0], dateArray.Item1[1], dateArray.Item1[2], settings[14], Convert.ToByte(settings[19]),
                                                settings[6], dateArray.Item4, dateArray.Item5, "AT-3", 3);                          

                        }
                        ).Start();
                    }

                    if (sendRequestToAT4 == 1)
                    {
                        processGo = true;

                        new Thread(() =>
                        {
                            DoJob(doneAT4, dateArray.Item1[0], dateArray.Item1[1], dateArray.Item1[2], settings[15], Convert.ToByte(settings[20]), 
                                                settings[6], dateArray.Item4, dateArray.Item5, "AT-4", 4);
                        }
                        ).Start();
                    }

                    if (sendRequestToAT5 == 1)
                    {
                        processGo = true;

                        new Thread(() =>
                        {
                            DoJob(doneAT5, dateArray.Item1[0], dateArray.Item1[1], dateArray.Item1[2], settings[16], Convert.ToByte(settings[21]), 
                                                settings[6], dateArray.Item4, dateArray.Item5, "AT-5", 5);
                        }
                        ).Start();
                    }
                }
            }
        }
        static void CreateTXT(int param)
        {
            string[] lines = new string[] { "01", "00", "00", "01", "05", "00", "60000", "1", "1", "1", "1", "1", "COM10", "COM11", "COM12", "COM13",
                                            "COM14", "1", "1", "2", "1", "1"};

            switch (param)
            {
                case 0:
                    Console.WriteLine("Файл с настройками settings.txt отсуствует. Будет создан новый файл, с настройками по умолчанию: " +
                                    "\n Время опроса 01:00:00 - 01:05:00 " +
                                    "\n Таймаут - 60000 " +
                                    "\n Опрашиваются АТ1, АТ2, АТ3, АТ4, АТ5" +
                                    "\n Порты: COM10, COM11, COM12, COM13, COM14" +
                                    "\n ID ModusSlave (на УВП): 1, 1, 2, 1, 1");

                    try
                    {
                        StreamWriter file = new StreamWriter("settings.txt");

                        for (int j = 0; j < lines.Length; j++)
                        {
                            file.WriteLine(lines[j].ToString());
                        }

                        file.Close();

                        Console.WriteLine("Создан новый файл settings.txt \n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    break;
                case 1:
                    Console.WriteLine("Файл с журналом событий log.txt отсуствует. Будет создан новый файл.");
                    try
                    {
                        StreamWriter file = new StreamWriter("log.txt"); 
                        file.Close();

                        Console.WriteLine("Создан новый файл log.txt \n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    break;
            }
        }
        static void Messaging(string Message)
        {           
            Console.WriteLine(Message);

            try
            {
                using (StreamWriter sw = new StreamWriter(GlobalVariables.logFilePath, true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static int DoJob(bool done, int Day, int Month, int Year, string serialPortNumber, byte slaveID, string sleepTime, DateTime PeriodDateFrom,
                            DateTime PeriodDateTo, string numberAT, int param)
        {
            int RequestID;
            string queryString;            
            Random rnd = new Random();

            while (done == false)
            {
                RequestID = rnd.Next(100, 999);                
                SendRequest_Mod(Day, Month, Year, RequestID, serialPortNumber, slaveID);

                Messaging($"Информация по { numberAT } запрошена { DateTime.Now.ToString() }. ID запроса { RequestID.ToString() }\n");
                Thread.Sleep(Convert.ToInt32(sleepTime));
                
                var result = GetResponse_Mod(RequestID, serialPortNumber, slaveID);
                
                if (result.Item1 == 2)                
                {
                    try
                    {
                        queryString = $@"INSERT INTO MainTable(RequestDate, PeriodDateFrom, PeriodDateTo, At_Number, ProductVolume) 
                                            VALUES ('{ DateTime.Now.ToString() }', '{ PeriodDateFrom.ToString() }', '{ PeriodDateTo.ToString() }', 
                                                    '{numberAT }', { result.Item2 })";                        

                        ConnectToDB(queryString, param);
                        done = true;
                        GlobalVariables.totalDone++;                        
                    }
                    catch (Exception ex)
                    {
                        Messaging(ex.ToString());
                    }
                }
                else
                {
                    done = false;
                }
            }
            
            return 0;
        }
        static Tuple<int[], DateTime, DateTime, DateTime, DateTime>
        DateSetUp(string[] settings, int incr)
        {
            DateTime dateTime = DateTime.UtcNow.Date;
            DateTime yesterdayDate;
            int[] intArray = new int[3];
            int Day, Month, Year;

            var periodFrom = new DateTime(Convert.ToInt32(dateTime.ToString("yyyy")), Convert.ToInt32(dateTime.ToString("MM")), Convert.ToInt32(dateTime.ToString("dd")), Convert.ToInt32(settings[0]), Convert.ToInt32(settings[1]), Convert.ToInt32(settings[2]));
            var periodTo = new DateTime(Convert.ToInt32(dateTime.ToString("yyyy")), Convert.ToInt32(dateTime.ToString("MM")), Convert.ToInt32(dateTime.ToString("dd")), Convert.ToInt32(settings[3]), Convert.ToInt32(settings[4]), Convert.ToInt32(settings[5]));

            periodFrom = periodFrom.AddDays(incr);
            periodTo = periodTo.AddDays(incr);

            Messaging($"Данные будут запрошены в промежуток времени { periodFrom.ToString() } - { periodTo.ToString() }\n");

            yesterdayDate = periodFrom.AddDays(-1);

            Day = Convert.ToInt32(yesterdayDate.ToString("dd"));
            Month = Convert.ToInt32(yesterdayDate.ToString("MM"));
            Year = Convert.ToInt32(yesterdayDate.ToString("yyyy"));

            intArray[0] = Day;
            intArray[1] = Month;
            intArray[2] = Year;

            var requestPeriodFrom = new DateTime(Convert.ToInt32(yesterdayDate.ToString("yyyy")), Convert.ToInt32(yesterdayDate.ToString("MM")), Convert.ToInt32(yesterdayDate.ToString("dd")), 00, 00, 0);
            var requestPeriodTo = new DateTime(Convert.ToInt32(yesterdayDate.ToString("yyyy")), Convert.ToInt32(yesterdayDate.ToString("MM")), Convert.ToInt32(yesterdayDate.ToString("dd")), 23, 59, 59);

            Messaging($"Данные будут запрошены за период { requestPeriodFrom.ToString() } - { requestPeriodTo.ToString() }\n");
            
            return Tuple.Create(intArray, periodFrom, periodTo, requestPeriodFrom, requestPeriodTo);
        }
        static void ConnectToDB(string queryString, int param)
        {
            using (SQLiteConnection conn = new SQLiteConnection("DATA SOURCE = UVP.db3"))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    try
                    {
                        conn.Open();
                        cmd.CommandText = queryString;
                        cmd.ExecuteNonQuery();
                        switch (param)
                        {
                            case 0:
                                Messaging("Начало работы \n");
                                break;
                            case 1:
                                Messaging("Запись АТ-1 успешно добавлена в базу данных \n");
                                break;
                            case 2:
                                Messaging("Запись АТ-2 успешно добавлена в базу данных \n");
                                break;
                            case 3:
                                Messaging("Запись АТ-3 успешно добавлена в базу данных \n");
                                break;
                            case 4:
                                Messaging("Запись АТ-4 успешно добавлена в базу данных \n");
                                break;
                            case 5:
                                Messaging("Запись АТ-5 успешно добавлена в базу данных \n");
                                break;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        Messaging($"SQL: { ex }");
                    }
                }
            }
        }
        static void SendRequest_Mod(int Day, int Month, int Year, int RequestID, string serialPortNumber, byte slaveID)
        {
            SerialPort serialPort = new SerialPort(serialPortNumber, 9600, Parity.None, 8, StopBits.One);

            try
            {
                if (serialPort.IsOpen == false)
                    serialPort.Open();

                var master = ModbusSerialMaster.CreateRtu(serialPort);

                master.WriteSingleRegister(slaveID, 7900, (ushort)RequestID);
                master.WriteSingleRegister(slaveID, 7901, 1);
                master.WriteSingleRegister(slaveID, 7902, (ushort)Day);
                master.WriteSingleRegister(slaveID, 7903, (ushort)Month);
                master.WriteSingleRegister(slaveID, 7904, (ushort)Year);
                master.WriteSingleRegister(slaveID, 7905, 00);
                master.WriteSingleRegister(slaveID, 7906, 00);
                master.WriteSingleRegister(slaveID, 7907, 00);
                master.WriteSingleRegister(slaveID, 7908, (ushort)Day);
                master.WriteSingleRegister(slaveID, 7909, (ushort)Month);
                master.WriteSingleRegister(slaveID, 7910, (ushort)Year);
                master.WriteSingleRegister(slaveID, 7911, 23);
                master.WriteSingleRegister(slaveID, 7912, 59);
                master.WriteSingleRegister(slaveID, 7913, 59);
                master.WriteSingleRegister(slaveID, 7914, 0);

                Messaging("Запрос был отправлен");
                
                serialPort.Close();
            }
            catch (Exception ex)
            {
                Messaging(ex.ToString());
                serialPort.Close();
            }

        }
        static Tuple<int, string>
        GetResponse_Mod(int RequestID, string serialPortNumber, byte slaveID)
        {
            string response = "";
            int status = 0;
            ushort requestStatus, requestID, resultLength, resultData;

            SerialPort serialPort = new SerialPort(serialPortNumber, 9600, Parity.None, 8, StopBits.One);

            try
            {
                if (serialPort.IsOpen == false)
                    serialPort.Open();

                var master = ModbusSerialMaster.CreateRtu(serialPort);

                requestStatus = master.ReadHoldingRegisters(slaveID, 8000, 1)[0];
                requestID = master.ReadHoldingRegisters(slaveID, 8001, 1)[0];
                resultLength = master.ReadHoldingRegisters(slaveID, 8002, 1)[0];
                resultData = master.ReadHoldingRegisters(slaveID, 8040, 1)[0];

                switch (Convert.ToInt32(requestStatus))
                {
                    case 0:
                        status = 0;
                        Messaging("Результат запроса уничтожен по таймауту или запрос не был воспринят вычислителем \n");
                        break;
                    case 1:
                        status = 1;
                        Messaging("Идет сбор данных \n");
                        break;
                    case 2:
                        status = 2;
                        Messaging("Данные готовы \n");

                        if (RequestID == Convert.ToInt32(requestID))
                        {
                            response = resultData.ToString();
                        }
                        else
                        {
                            status = 8;
                            Messaging($"ID запроса не равен ID ответа { RequestID.ToString() } <> { requestID.ToString() } \n");
                        }
                        break;
                    case 3:
                        status = 3;
                        Messaging("За выбранный промежуток времени в архиве нет ни одной записи \n");
                        break;
                    case 4:
                        status = 4;
                        Messaging("Ошибка в начальном времени, дата или время некорректны \n");
                        break;
                    case 5:
                        status = 5;
                        Messaging("Ошибка в конечном времени, дата или время некорректны \n");
                        break;
                    case 6:
                        status = 6;
                        Messaging("Ошибка в задании номера трубопровода \n");
                        break;
                    case 7:
                        status = 7;
                        Messaging("Выбран слишком большой период для сбора данных, сбор данных продолжался больше 15 секунд \n");
                        break;
                }

                serialPort.Close();
            }
            catch (Exception ex)
            {
                Messaging(ex.ToString());
                serialPort.Close();
            }

            return Tuple.Create<int, string>(status, response);
        }
    }
}
