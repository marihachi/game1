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
        var payload = new OkPayload();
        Output = new Packet(Input.Sequence, PacketPayloadSerializer.Serialize(payload));
    }

    public void Fail(PacketErrorCode code)
    {
        var payload = new ErrorPayload(code);
        Output = new Packet(Input.Sequence, PacketPayloadSerializer.Serialize(payload));
    }
}
