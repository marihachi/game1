using GameCommon;
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
                    var udpResult = await udp.ReceiveAsync();

                    // 受信したパケットを非同期で処理
                    _ = Task.Run(async () =>
                    {
                        var udpData = udpResult.Buffer;
                        var udpAddress = udpResult.RemoteEndPoint;

                        // パケットを処理
                        Packet outputPacket;
                        try
                        {
                            if (udpData.Length == 0)
                            {
                                throw new Exception("Received empty packet.");
                            }

                            Packet inputPacket = Packet.Deserialize(udpData);

                            PacketProcess.ServerContext ctx = new(inputPacket);

                            PacketProcess.ProcessPacket(ctx);

                            if (ctx.Output == null)
                            {
                                throw new Exception("output is empty");
                            }
                            outputPacket = ctx.Output;
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[{udpAddress}] Exception: {ex.Message}");
                            var payload = new ErrorPayload(ErrorCode.InternalError);
                            outputPacket = new Packet(0, PacketKind.Error, payload.Serialize());
                        }

                        // 結果を送信
                        try
                        {
                            var outputData = outputPacket.Serialize();
                            await udp.SendAsync(outputData, outputData.Length, udpAddress);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[{udpAddress}] Send error: {ex.Message}");
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
