using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = @"C:\Users\Anika\Documents\ПИ\server\data";


            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr, 8888);
            server.Start();    // запускаем сервер
            Console.WriteLine("Server started!");

            while (true)
            {
                // получаем подключение в виде TcpClient
                TcpClient client = server.AcceptTcpClient();

                // получение данных от пользователя
                NetworkStream stream = client.GetStream();

                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string messenger = Encoding.UTF8.GetString(data, 0, bytes);

                string[] requestData = messenger.Split('\n');

                if (requestData[0] == "exit")
                {
                    stream.Close();
                    server.Stop(); // останавливаем сервер

                    return;
                }

                string number = requestData[0];
                string name = requestData[1];

                string filePath = Path.Combine(folderPath, name);
                string response = "";
                switch (number)
                {
                    case "1":
                        try
                        {
                            // Проверяем существование файла
                            if (File.Exists(filePath))
                            {
                                // Читаем все строки из файла
                                string lines = File.ReadAllText(filePath);
                                response = "200\n" + lines;
                                Console.WriteLine("200");
                            }
                            else
                            {
                                response = "403\n ";
                                Console.WriteLine("403");
                            }
                        }
                        catch
                        {
                            response = "403\n ";
                            Console.WriteLine("403");
                        }
                        break;

                    case "2":
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                response = "403";
                                Console.WriteLine("403");
                            }
                            else
                            {
                                string content = requestData[2];
                                File.WriteAllText(filePath, content);

                                response = "200";
                                Console.WriteLine("200");

                            }
                        }
                        catch
                        {
                            response = "403\n ";
                            Console.WriteLine("403");
                        }

                        break;

                    case "3":
                        try
                        {
                            // Проверяем существование файла
                            if (File.Exists(filePath))
                            {
                                // Удаляем файл
                                File.Delete(filePath);
                                response = "200";
                                Console.WriteLine("200");
                            }
                            else
                            {
                                response = "403";
                                Console.WriteLine("403");
                            }
                        }
                        catch
                        {
                            response = "403";
                            Console.WriteLine("403");
                        }
                        break;
                }
                byte[] responseData = Encoding.UTF8.GetBytes(response);
                stream.Write(responseData, 0, responseData.Length);
            }
        }
    }
}
