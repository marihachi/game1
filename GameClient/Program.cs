using GameCommon;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace GameClient;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            using var udp = new UdpClient(0);
            IPEndPoint serverEndpoint = new(NetworkUtilities.ResolveAddress("localhost"), 3000);

            var pendingResponses = new ConcurrentDictionary<ushort, TaskCompletionSource<Packet>>();

            // 受信タスク
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var result = await udp.ReceiveAsync();
                    var packet = PacketSerializer.Deserialize(result.Buffer);
                    if (pendingResponses.TryRemove(packet.Sequence, out var tcs))
                    {
                        tcs.SetResult(packet);
                    }
                }
            });

            async Task Send(ushort seq, byte[] packetData)
            {
                var completion = new TaskCompletionSource<Packet>();
                pendingResponses[seq] = completion;

                await udp.SendAsync(packetData, packetData.Length, serverEndpoint);

                // タイムアウト付きで応答を待機
                var completed = await Task.WhenAny(completion.Task, Task.Delay(10000));
                if (completed != completion.Task)
                    throw new Exception($"timeout (seq={seq})");

                var response = await completion.Task;

                var payloadData = response.Payload;
                var payload = PacketPayloadSerializer.Deserialize(payloadData);

                if (payload.PayloadKind == PacketPayloadKind.Ok)
                    Console.WriteLine($"[{serverEndpoint}] OK (seq={seq})");
                else if (payload.PayloadKind == PacketPayloadKind.Error)
                    Console.WriteLine($"[{serverEndpoint}] Error (seq={seq})");
                else
                    throw new Exception($"unknown response (seq={seq})");
            }

            // 通信を開始
            ushort seq = 1;

            var handshakePayload = PacketPayloadSerializer.Serialize(new HandshakePayload());
            var handshakePacket = PacketSerializer.Serialize(new Packet(seq, handshakePayload));
            await Send(seq, handshakePacket);
            seq++;

            var tasks = Enumerable.Range(seq, 5).Select(async seq =>
            {
                try
                {
                    var movePayload = PacketPayloadSerializer.Serialize(new MovePayload());
                    var movePacket = PacketSerializer.Serialize(new Packet((ushort)seq, movePayload));
                    await Send((ushort)seq, movePacket);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[SendTask] Exception: {ex.Message}");
                    return;
                }
            });
            await Task.WhenAll(tasks);
            seq += 5;

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Root] Exception: {ex.Message}");
        }
    }
}
