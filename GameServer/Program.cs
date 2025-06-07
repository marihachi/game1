using GameCommon;
using System.Net;
using System.Net.Sockets;

namespace GameServer;

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
        IPEndPoint serverEndpoint = new(NetworkUtilities.ResolveAddress("localhost"), 3000);

        using var udp = new UdpClient(serverEndpoint);

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
        Packet? outputPacket = null;
        ushort? sequence = null;
        try
        {
            if (inputData.Length == 0)
            {
                throw new Exception("Received empty packet.");
            }

            Packet inputPacket = PacketSerializer.Deserialize(inputData);

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

            if (sequence != null)
            {
                var payload = new ErrorPayload(PacketErrorCode.InternalError);
                outputPacket = new Packet(sequence.Value, PacketPayloadSerializer.Serialize(payload));
            }
        }

        // 結果を送信
        if (outputPacket != null)
        {
            try
            {
                var outputData = PacketSerializer.Serialize(outputPacket);
                await udp.SendAsync(outputData, outputData.Length, clientEndpoint);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{clientEndpoint}] Send error: {ex.Message}");
            }
        }  
    }
}
