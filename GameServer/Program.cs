using GameCommon;
using System.Net.Sockets;

namespace GameServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
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
                            var inputData = udpResult.Buffer;
                            var clientEndpoint = udpResult.RemoteEndPoint;

                            // パケットを処理
                            Packet outputPacket;
                            try
                            {
                                if (inputData.Length == 0)
                                {
                                    throw new Exception("Received empty packet.");
                                }

                                Packet inputPacket = Packet.Deserialize(inputData);

                                ServerContext ctx = new(inputPacket, clientEndpoint);

                                PacketProcess.ProcessPacket(ctx);

                                if (ctx.Output == null)
                                {
                                    throw new Exception("output is empty");
                                }
                                outputPacket = ctx.Output;
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"[{clientEndpoint}] Exception: {ex.Message}");
                                var payload = new ErrorPayload(ErrorCode.InternalError);
                                outputPacket = new Packet(0, PacketKind.Error, payload.Serialize());
                            }

                            // 結果を送信
                            try
                            {
                                var outputData = outputPacket.Serialize();
                                await udp.SendAsync(outputData, outputData.Length, clientEndpoint);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"[{clientEndpoint}] Send error: {ex.Message}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[MainLoop] Exception: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Root] Exception: {ex.Message}");
                return;
            }
        }
    }
}
