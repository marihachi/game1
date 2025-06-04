using GameCommon;
using System.Net;
using System.Net.Sockets;

namespace GameClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            using var udp = new UdpClient();

            IPEndPoint serverEndpoint = new(NetworkUtilities.ResolveAddress("localhost"), 3000);

            Packet outputPacket;
            outputPacket = new Packet(0, PacketKind.Handshake, []);
            var outputData = outputPacket.Serialize();
            await udp.SendAsync(outputData, outputData.Length, serverEndpoint);

            // サーバーからの応答を受信（タイムアウト付き）
            var receiveTask = udp.ReceiveAsync();
            if (await Task.WhenAny(receiveTask, Task.Delay(3000)) != receiveTask)
                throw new Exception("timeout");

            var result = receiveTask.Result;
            var inputData = result.Buffer;
            var inputPacket = Packet.Deserialize(inputData);
            switch (inputPacket.Kind)
            {
                case PacketKind.Ok:
                    Console.WriteLine($"[{serverEndpoint}] OK");
                    break;
                case PacketKind.Error:
                    Console.WriteLine($"[{serverEndpoint}] Error");
                    break;
                default:
                    throw new Exception("unknown response");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Root] Exception: {ex.Message}");
        }
    }
}
