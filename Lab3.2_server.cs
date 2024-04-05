using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Lab3._2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePathServer = @"C:\Users\Anika\Documents\LAB3.2\server\data";
            string filePathClient = @"C:\Users\Anika\Documents\LAB3.2\client\data";
            string filePathIDofFiles = @"C:\Users\Anika\Documents\LAB3.2\IDofFiles";

            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr, 8888);

            try
            {
                server.Start();    // запускаем сервер
                Console.WriteLine("Server started!");

                while (true)
                {
                    try
                    {
                        // получаем подключение в виде TcpClient
                        var client = await server.AcceptTcpClientAsync();


                        // создаем новый поток для обслуживания нового клиента
                        new Task(() => ProcessClientAsync(client)).Start();
                    }
                    catch
                    {
                        break;
                    }

                }
            }
            finally
            {
                server.Stop();
            }
            // обрабатываем клиент
            async Task ProcessClientAsync(TcpClient client)
            {             
                Dictionary<string, string> serverFiles = new Dictionary<string, string>();

                var stream = client.GetStream();
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(stream);
                
                //заполнение словаря файлами хранящимися на сервере
                ReadAllServerFiles(filePathIDofFiles, serverFiles);
                while (true)
                {
                    string command = reader.ReadString();
                    string serverFileName = "";
                    string userFileName = "";
                    string serverFilePath = "";
                    switch (command)
                    {
                        case "1": 
                            string choice = reader.ReadString();

                            if (choice == "1")
                            {
                                serverFileName = reader.ReadString();
                                serverFilePath = Path.Combine(filePathServer, serverFileName);
                                if (File.Exists(serverFilePath))
                                {
                                    byte[] fileBytes = File.ReadAllBytes(serverFilePath);
                                    writer.Write("200");
                                    writer.Flush();
                                    writer.Write(fileBytes.Length); 
                                    writer.Flush();
                                    writer.Write(fileBytes); 
                                    writer.Flush();
                                }
                                else
                                {
                                    writer.Write("404");
                                    writer.Flush();
                                }
                            }
                            else if (choice == "2")
                            {
                                string fileId = reader.ReadString();
                                serverFileName = GetFileNameById(fileId, serverFiles);
                                serverFilePath = Path.Combine(filePathServer, serverFileName);
                                if (File.Exists(serverFilePath))
                                {
                                    byte[] fileBytes = File.ReadAllBytes(serverFilePath);
                                    writer.Write("200"); 
                                    writer.Flush();
                                    writer.Write(fileBytes.Length); 
                                    writer.Flush();
                                    writer.Write(fileBytes); 
                                    writer.Flush();
                                }
                                else
                                {
                                    writer.Write("404");
                                    writer.Flush();
                                }
                            }
                            break;
                        case "2": 
                            string fileName = reader.ReadString();
                            int fileSize = reader.ReadInt32();

                            // Читаем данные файла
                            byte[] fileData = new byte[fileSize];
                            int bytesRead = 0;
                            while (bytesRead < fileSize)
                            {
                                int bytesReceived = stream.Read(fileData, bytesRead, fileSize - bytesRead);
                                if (bytesReceived == 0)
                                {
                                    // Если не удалось прочитать файл полностью, отправляем клиенту ошибку
                                    writer.Write("404");
                                    writer.Flush();
                                    return;
                                }
                                bytesRead += bytesReceived;
                            }

                            // Сохраняем файл на сервере
                            string serverFilePathSave = Path.Combine(filePathServer, fileName);
                            File.WriteAllBytes(serverFilePathSave, fileData);

                            // Генерируем ID файла и отправляем его клиенту
                            string id = Guid.NewGuid().ToString();
                            serverFiles.Add(id, fileName);
                            using (StreamWriter sw = new StreamWriter(filePathIDofFiles, true))
                            {
                                sw.WriteLine($"{id}|{fileName}");
                            }

                            // Отправляем клиенту подтверждение сохранения файла и его ID
                            writer.Write("200");
                            writer.Flush();
                            writer.Write(id);
                            writer.Flush();
                            break;

                        case "3":
                            string nameorIdDel = reader.ReadString(); 

                            if (nameorIdDel == "1")
                            {
                                serverFileName = reader.ReadString();
                            }
                            else if (nameorIdDel == "2")
                            {
                                string serverFileId = reader.ReadString();
                                serverFileName = GetFileNameById(serverFileId, serverFiles);
                                if (serverFileName == "")
                                {
                                    writer.Write("404");
                                    writer.Flush();
                                    break;
                                }
                            }
                            serverFilePath = Path.Combine(filePathServer, serverFileName);

                            if (!File.Exists(serverFilePath))
                            {
                                writer.Write("404");
                                writer.Flush();
                            }
                            else
                            {
                                try
                                {
                                    File.Delete(serverFilePath);
                                    using (StreamWriter sw = new StreamWriter(File.Open(filePathIDofFiles, FileMode.Create)))
                                    {
                                        foreach (KeyValuePair<string, string> kvp in serverFiles)
                                        {
                                            sw.WriteLine($"{kvp.Key}--{kvp.Value}");
                                        }
                                    }
                                    writer.Write("200");
                                    writer.Flush();
                                }
                                catch
                                {
                                    writer.Write("404");
                                    writer.Flush();
                                }
                            }
                            break;

                        case "exit":
                            client.Close();
                            stream.Close();
                            reader.Close();
                            writer.Close();
                            server.Stop();
                            return;

                    }
                }
                

                static string GetFileNameById(string fileId, Dictionary<string, string> serverFile)
                {
                    for (int i = 0; i < serverFile.Count; i++)
                    {
                        var pair = serverFile.ElementAt(i);
                        if (pair.Key == fileId)
                        {
                            return pair.Value;
                        }
                    }
                    return "";

                }

                static void ReadAllServerFiles(string filePathIDofFiles, Dictionary<string, string> serverFiles)
                {
                    if (!File.Exists(filePathIDofFiles))
                    {
                        Console.WriteLine($"Нету filePathFilesDB {filePathIDofFiles}");
                        File.Create(filePathIDofFiles);
                    }
                    var lines = File.ReadAllLines(filePathIDofFiles);//массив строк 

                    foreach (var line in lines)
                    {
                        var parts = line.Split("--");
                        serverFiles.Add(parts[0], parts[1]);
                        Console.WriteLine($"{line} , {parts[0]}, {parts[1]}");
                    }
                }

            }
        }
    }
}
