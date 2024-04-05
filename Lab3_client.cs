using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Lab3_client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 8888);

                NetworkStream stream = client.GetStream();

                Console.Write("Enter action (1 - get a file, 2 - create a file, 3 - delete a file): > ");
                string command = Console.ReadLine();
                string message;

                if (command == "exit")
                {
                    message = $"{command}";
                    byte[] data1 = Encoding.UTF8.GetBytes(message);
                    stream.Write(data1, 0, data1.Length);
                    Console.WriteLine("The request was sent.");

                    // Закрываем соединение
                    stream.Close();
                    client.Close();
                    return;
                }


                Console.Write("Enter filename: > ");
                string name = Console.ReadLine();


                message = $"{command}\n{name}";
                if (command == "2")
                {
                    Console.Write("Enter the content: > ");
                    string content = Console.ReadLine();
                    message += $"\n{content}";
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("The request was sent.");


                // Получение ответа от сервера
                byte[] responseData = new byte[256];
                int bytes = stream.Read(responseData, 0, responseData.Length);
                string response = Encoding.UTF8.GetString(responseData, 0, bytes);
                switch (command)
                {
                    case "1":

                        string[] requestData = response.Split('\n');
                        if (requestData[0] == "200") { Console.WriteLine("The content of the file is: " + requestData[1]); }
                        else { Console.WriteLine("The response says that the file was not found!"); }
                        break;

                    case "2":

                        if (response == "200") { Console.WriteLine("The response says that the file was created!"); }
                        else { Console.WriteLine("The response says that creating the file was forbidden!"); }
                        break;

                    case "3":

                        if (response == "200") { Console.WriteLine("The response says that the file was successfully deleted!"); }
                        else { Console.WriteLine("The response says that the file was not found!"); }
                        break;

                }



                stream.Close();
                // Закрываем соединение
                client.Close();

            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
