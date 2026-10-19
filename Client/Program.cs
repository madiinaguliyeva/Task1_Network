using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Client";
            var ipAddress = IPAddress.Parse("192.168.100.81");
            var port = 27001;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Client adınızı daxil edin: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string clientName = Console.ReadLine()?.Trim();
            Console.ResetColor();
            if (string.IsNullOrEmpty(clientName)) clientName = "Mədinə";

            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(new IPEndPoint(ipAddress, port));

                byte[] clientNameBytes = Encoding.UTF8.GetBytes(clientName);
                byte[] clientNameLenBytes = BitConverter.GetBytes(clientNameBytes.Length);
                await socket.SendAsync(clientNameLenBytes, SocketFlags.None);
                await socket.SendAsync(clientNameBytes, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Serverə qoşulmaq olmadı: {ex.Message}");
                Console.ResetColor();
                Thread.Sleep(3000);
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.WriteLine($"          FILE CLIENT: {clientName}       ");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("1 - Fayl göndər (şəkil/səs)");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("0 - Çıxış");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.Write("Seçim edin: ");

                var choice = Console.ReadLine();

                if (choice == "0")
                {
                    socket?.Dispose();
                    break;
                }

                if (choice == "1")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("\nFaylın yolunu daxil edin:\n> ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    string filePath = Console.ReadLine()?.Trim('\"');
                    Console.ResetColor();

                    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Fayl tapılmadı!");
                        Console.ResetColor();
                        Thread.Sleep(1500);
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\nGöndərilir...");
                    Console.ResetColor();

                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);
                        byte[] nameLenBytes = BitConverter.GetBytes(nameBytes.Length);

                        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                        byte[] fileSizeBytes = BitConverter.GetBytes((long)fileBytes.Length);

                        await socket.SendAsync(nameLenBytes, SocketFlags.None);
                        await socket.SendAsync(nameBytes, SocketFlags.None);
                        await socket.SendAsync(fileSizeBytes, SocketFlags.None);
                        await socket.SendAsync(fileBytes, SocketFlags.None);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Fayl uğurla göndərildi!");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Xəta: {ex.Message}");
                        Console.ResetColor();
                    }

                    Console.WriteLine("\nDavam etmək üçün bir düyməyə basın...");
                    Console.ReadKey();
                }
            }
        }
    }
}