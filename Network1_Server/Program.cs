using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Network1_Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Server";
            var ipAddress = IPAddress.Parse("192.168.100.81");
            var port = 27001;

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(new IPEndPoint(ipAddress, port));
                socket.Listen(10);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("========================================");
                Console.WriteLine("              FILE SERVER               ");
                Console.WriteLine("========================================");
                Console.ResetColor();
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Server işə salındı...");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Port: {port}");
                Console.ResetColor();

                Console.WriteLine("Gələn fayllar qəbul edilir...\n");

                while (true)
                {
                    var client = await socket.AcceptAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var remoteEndPoint = client.RemoteEndPoint?.ToString() ?? "Bilinməyən";

                            var clientNameLenBuf = new byte[4];
                            await client.ReceiveAsync(clientNameLenBuf, SocketFlags.None);
                            int clientNameLen = BitConverter.ToInt32(clientNameLenBuf, 0);

                            var clientNameBuf = new byte[clientNameLen];
                            await client.ReceiveAsync(clientNameBuf, SocketFlags.None);
                            string clientName = Encoding.UTF8.GetString(clientNameBuf);

                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.Write("[QOŞULDU] ");
                            Console.ResetColor();
                            Console.WriteLine($"{clientName} ({remoteEndPoint}) serverə qoşuldu.");

                            var lenBuffer = new byte[4];
                            await client.ReceiveAsync(lenBuffer, SocketFlags.None);
                            int fileNameLength = BitConverter.ToInt32(lenBuffer, 0);

                            var nameBuffer = new byte[fileNameLength];
                            await client.ReceiveAsync(nameBuffer, SocketFlags.None);
                            string fileName = Encoding.UTF8.GetString(nameBuffer);

                            var sizeBuffer = new byte[8];
                            await client.ReceiveAsync(sizeBuffer, SocketFlags.None);
                            long fileSize = BitConverter.ToInt64(sizeBuffer, 0);

                            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                            using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                            {
                                var buffer = new byte[8192];
                                long totalReceived = 0;
                                while (totalReceived < fileSize)
                                {
                                    int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                                    if (bytesRead == 0) break;
                                    await fs.WriteAsync(buffer, 0, bytesRead);
                                    totalReceived += bytesRead;
                                }
                            }

                            string sizeStr = fileSize > 1024 * 1024
                                ? $"{fileSize / (1024.0 * 1024.0):F1} MB"
                                : $"{fileSize / 1024.0:F1} KB";

                            string timeStr = DateTime.Now.ToString("HH:mm:ss");

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("[QƏBUL EDİLDİ] ");
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write($"{fileName,-12} ({sizeStr,7}) ");
                            Console.ResetColor();
                            Console.WriteLine($"| Kimdən: {clientName} [{remoteEndPoint}]   {timeStr}");
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Xəta: {ex.Message}");
                            Console.ResetColor();
                        }
                        finally
                        {
                            client.Dispose();
                        }
                    });
                }
            }
        }
    }
}