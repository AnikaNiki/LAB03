using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Lab3._2_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePathServer = @"C:\Users\Anika\Documents\LAB3.2\server\data";
            string filePathClient = @"C:\Users\Anika\Documents\LAB3.2\client\data";
            string filePathIDofFiles = @"C:\Users\Anika\Documents\LAB3.2\IDofFiles";
            TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 8888);

            NetworkStream stream = client.GetStream();
            using (stream)
            using (BinaryReader reader = new BinaryReader(stream))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                while (true)
                {
                    Console.Write("Enter action (1 - get a file, 2 - save a file, 3 - delete a file): > ");
                    string command = Console.ReadLine();
                    writer.Write(command);
                    writer.Flush();
                    switch (command)
                    {
                        case "1"://get
                            Console.Write("Do you want to get the file by name or by id (1 - name, 2 - id): > ");
                            string choise = Console.ReadLine();
                            writer.Write(choise);
                            writer.Flush();

                            if (choise == "1")
                            {
                                //отправка
                                Console.Write("Enter name: > ");
                                string serverFileName = Console.ReadLine();
                                writer.Write(serverFileName);
                                Console.WriteLine("The request was sent.");

                                //получение
                                string responseGetName = reader.ReadString();
                                if (responseGetName == "200")
                                {
                                    Console.Write("The file was downloaded! Specify a name for it: > ");
                                    string fileNameGet = Console.ReadLine();
                                    int fileSize = reader.ReadInt32();
                                    byte[] fileBytesOutput = reader.ReadBytes(fileSize);
                                    File.WriteAllBytes(fileNameGet, fileBytesOutput);
                                }
                                else if (responseGetName == "404")
                                {
                                    Console.WriteLine("The response says that this file is not found!");
                                }
                            }
                            else if (choise == "2")
                            {
                                //отправка
                                Console.Write("Enter ID: > ");
                                string serverFileID = Console.ReadLine();
                                writer.Write(serverFileID);
                                writer.Flush();
                                Console.WriteLine("The request was sent.");

                                //получение
                                string responseGetID = reader.ReadString();
                                if (responseGetID == "200")
                                {
                                    Console.Write("The file was downloaded! Specify a name for it: > ");
                                    string fileNameGet = Console.ReadLine();
                                    int fileSize = reader.ReadInt32();
                                    byte[] fileBytesID = reader.ReadBytes(fileSize);
                                    File.WriteAllBytes(fileNameGet, fileBytesID);
                                }
                                else if (responseGetID == "404")
                                {
                                    Console.WriteLine("The response says that this file is not found!");
                                }
                            }
                            break;

                        case "2": //save a file
                                  //отправка
                            Console.Write("Enter name of the file: >");
                            string fileName = Console.ReadLine();
                            string filePath = Path.Combine(filePathClient, fileName);
                            byte[] fileBytes = File.ReadAllBytes(filePath);
                            writer.Write(Path.GetFileName(filePath));
                            writer.Write(fileBytes.Length);
                            writer.Write(fileBytes);
                            Console.WriteLine("The request was sent.");

                            //получение
                            string response = reader.ReadString();
                            if (response == "200")
                            {
                                string IdOfFile = reader.ReadString();
                                Console.WriteLine($"Response says that file is saved! ID = {IdOfFile}");
                            }
                            else if (response == "404")
                            {
                                Console.WriteLine("The response says that the file can't be created");
                            }
                            break;
                        case "3": //delete
                                  //отправка
                            Console.Write("Do you want to delete the file by name or by id (1 - name, 2 - id): > ");
                            string choiseDel = Console.ReadLine();
                            writer.Write(choiseDel);
                            writer.Flush();
                            if (choiseDel == "1")
                            {
                                Console.Write("Enter name: > ");
                                string serverFileName = Console.ReadLine();
                                writer.Write(serverFileName);
                                writer.Flush();
                                Console.WriteLine("The request was sent.");
                            }
                            else if (choiseDel == "2")
                            {
                                Console.Write("Enter ID: > ");
                                string serverFileID = Console.ReadLine();
                                writer.Write(serverFileID);
                                writer.Flush();
                                Console.WriteLine("The request was sent.");
                            }

                            //получение
                            string responseDel = reader.ReadString();
                            if (responseDel == "200")
                            {
                                Console.WriteLine("The response says that this file was deleted successfully!");
                            }
                            else
                            {
                                Console.WriteLine("The response says that this file is not found!");
                            }
                            break;

                        case "exit":
                            client.Close();
                            stream.Close();
                            reader.Close();
                            writer.Close();
                            break;
                    }
                    if (client.Connected == false) { break; }
                }
            }
        }
    }
}
