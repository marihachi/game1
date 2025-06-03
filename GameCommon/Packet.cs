namespace GameCommon;

public enum PacketKind : byte
{
    Ok = 0x01,
    Error = 0x02,
    Handshake = 0x03,
    Move = 0x04,
}

public class Packet(ushort sequence, PacketKind kind, byte[] payload)
{
    public ushort Sequence { get; } = sequence;
    public PacketKind Kind { get; } = kind;
    public byte[] Payload { get; } = payload;

    private static readonly HashSet<byte> DefinedKind =
        new(Enum.GetValues<PacketKind>().Select(e => (byte)e));

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();

        // kind (1 byte)
        stream.WriteByte((byte)Kind);

        // payloadLength (2 bytes)
        PacketUtilities.WriteShortBE(stream, (ushort)Payload.Length, "payloadLength");

        // payload (n bytes)
        stream.Write(Payload, 0, Payload.Length);

        return stream.ToArray();
    }

    public static Packet Deserialize(byte[] source)
    {
        int readValue;

        using var stream = new MemoryStream(source);

        // sequence (2 bytes)
        var sequence = PacketUtilities.ReadShortBE(stream, "sequence");

        // packet kind (1 byte)
        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new Exception("kindの読み取りに失敗しました。");
        byte kind = (byte)readValue;

        // payload length (2 bytes)
        ushort payloadLength = PacketUtilities.ReadShortBE(stream, "payloadLength");

        // payload (n bytes)
        var payload  = new byte[payloadLength];
        int readLength = stream.Read(payload, 0, payload.Length);
        if (readLength != payload.Length)
            throw new Exception("payloadの読み取りに失敗しました。");
        payloadLength -= (ushort)payload.Length;

        if (payloadLength > 0)
            throw new Exception("ペイロードの長さが一致しません。");

        return new Packet(sequence, ToPacketKindEnum(kind), payload);
    }

    private static PacketKind ToPacketKindEnum(byte value)
    {
        if (!DefinedKind.Contains(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"未定義のPacketKind値: 0x{value:X2}");
        }

        return (PacketKind)value;
    }
}

public enum ErrorCode : ushort
{
    InvalidPacket = 0x0001,
    InternalError = 0xFFFF,
}

public class ErrorPayload(ErrorCode code)
{
    public ErrorCode Code { get; } = code;

    private static readonly HashSet<ushort> DefinedErrors =
        new(Enum.GetValues<ErrorCode>().Select(e => (ushort)e));

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();

        // code (2 bytes)
        PacketUtilities.WriteShortBE(stream, (ushort)Code, "code");

        return stream.ToArray();
    }

    public static ErrorPayload Deserialize(byte[] source)
    {
        using var stream = new MemoryStream(source);

        // code (2 bytes)
        var errorCode = ToErrorCodeEnum(PacketUtilities.ReadShortBE(stream, "code"));

        return new ErrorPayload(errorCode);
    }

    private static ErrorCode ToErrorCodeEnum(ushort value)
    {
        if (!DefinedErrors.Contains(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"未定義のErrorCode値: 0x{value:X4}");
        }

        return (ErrorCode)value;
    }
}

public class PacketUtilities
{
    public static ushort ReadShortBE(Stream stream, string paramName)
    {
        // ネットワークバイトオーダーで読み取る
        int readValue;

        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new Exception($"{paramName}の読み取りに失敗しました。");
        ushort value = (ushort)(readValue << 8);
        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new Exception($"{paramName}の読み取りに失敗しました。");
        value |= (ushort)readValue;

        return value;
    }

    public static void WriteShortBE(Stream stream, ushort value, string paramName)
    {
        // ネットワークバイトオーダーで書き込む
        try
        {
            int high = (value >> 8) & 0xFF;
            stream.WriteByte((byte)high);

            int low = value & 0xFF;
            stream.WriteByte((byte)low);
        }
        catch (Exception ex)
        {
            throw new Exception($"{paramName}の書き込みに失敗しました。", ex);
        }
    }
}
