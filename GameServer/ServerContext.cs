using GameCommon;
using System.Net;

namespace GameServer;

public class ServerContext(Packet input, IPEndPoint clientEndpoint)
{
    public IPEndPoint ClientEndpoint { get; } = clientEndpoint;
    public Packet Input { get; } = input;
    public Packet? Output { get; set; }

    public ushort NextSequence => (ushort)(Input.Sequence + 1u);

    public void Success()
    {
        Output = new Packet(NextSequence, PacketKind.Ok, []);
    }

    public void Fail(ErrorCode code)
    {
        var payload = new ErrorPayload(code);
        Output = new Packet(NextSequence, PacketKind.Error, payload.Serialize());
    }
}
