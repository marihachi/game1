using System.Net.Sockets;

namespace GameServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var udp = new UdpClient(3000);

            // 受信ループ
            while (true)
            {
                try
                {
                    var receivedResult = await udp.ReceiveAsync();

                    // 受信したパケットを非同期で処理
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var receivedData = receivedResult.Buffer;
                            var replyData = PacketProcess.ProcessPacket(receivedData);
                            await udp.SendAsync(replyData, replyData.Length, receivedResult.RemoteEndPoint);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[{receivedResult.RemoteEndPoint}] Exception: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Exception: {ex.Message}");
                }
            }
        }
    }
}
