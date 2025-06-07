using GameCommon;

namespace GameServer;

internal class PacketProcess
{
    public static void ProcessPacket(ServerContext ctx)
    {
        var payload = PacketPayloadSerializer.Deserialize(ctx.Input.Payload);

        if (payload is HandshakePayload)
        {
            ProcessHandshake(ctx);
        }
        else if (payload is MovePayload)
        {
            ProcessMove(ctx);
        }
        else
        {
            throw new Exception($"Received unknown payload type: {(byte)payload.PayloadKind}");
        }
    }

    private static void ProcessHandshake(ServerContext ctx)
    {
        Console.WriteLine($"[{ctx.ClientEndpoint}] Handshake");

        ctx.Success();
    }

    private static void ProcessMove(ServerContext ctx)
    {
        Console.WriteLine($"[{ctx.ClientEndpoint}] Move");
        // Console.WriteLine($"X = {req.Data["X"]}, Y = {req.Data["Y"]}");

        ctx.Success();
    }
}
