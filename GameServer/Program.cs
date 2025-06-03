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
                await ListenLoop();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Root] Exception: {ex.Message}");
                return;
            }
        }

        static async Task ListenLoop()
        {
            using var udp = new UdpClient(3000);

            while (true)
            {
                try
                {
                    var udpResult = await udp.ReceiveAsync();

                    // 受信したパケットを非同期で処理
                    _ = Task.Run(async () =>
                    {
                        await ReceiveHandler(udp, udpResult);
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ListenLoop] Exception: {ex.Message}");
                }
            }
        }

        static async Task ReceiveHandler(UdpClient udp, UdpReceiveResult udpResult)
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
        }
    }
}
