namespace GameCommon;

public class Packet(ushort sequence, byte[] payload)
{
    public ushort Sequence { get; } = sequence;
    public byte[] Payload { get; } = payload;
}

public enum PacketPayloadKind : byte
{
    Ok = 0x01,
    Error = 0x02,
    Handshake = 0x03,
    Move = 0x04,
}

public interface IPacketPayload
{
    public PacketPayloadKind PayloadKind { get; }
}

public class OkPayload() : IPacketPayload
{
    public PacketPayloadKind PayloadKind { get; } = PacketPayloadKind.Ok;
}

public class ErrorPayload(PacketErrorCode code) : IPacketPayload
{
    public PacketPayloadKind PayloadKind { get; } = PacketPayloadKind.Error;
    public PacketErrorCode Code { get; } = code;
}

public enum PacketErrorCode : ushort
{
    InvalidPacket = 0x0001,
    InternalError = 0xFFFF,
}

public class HandshakePayload() : IPacketPayload
{
    public PacketPayloadKind PayloadKind { get; } = PacketPayloadKind.Handshake;
}

public class MovePayload() : IPacketPayload
{
    public PacketPayloadKind PayloadKind { get; } = PacketPayloadKind.Move;
}
