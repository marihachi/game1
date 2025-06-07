namespace GameCommon;

public class PacketSerializer
{
    public static byte[] Serialize(Packet packet)
    {
        if (packet == null)
            throw new ArgumentNullException(nameof(packet), "Packet cannot be null.");

        using var stream = new MemoryStream();

        // sequence (2 bytes)
        WriteShortBE(stream, packet.Sequence, "sequence");

        // payload length (2 bytes)
        WriteShortBE(stream, (ushort)packet.Payload.Length, "payloadLength");

        // payload (n bytes)
        stream.Write(packet.Payload, 0, packet.Payload.Length);

        return stream.ToArray();
    }

    public static Packet Deserialize(byte[] source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");

        using var stream = new MemoryStream(source);

        // sequence (2 bytes)
        var sequence = ReadShortBE(stream, "sequence");

        // payload length (2 bytes)
        ushort payloadLength = ReadShortBE(stream, "payloadLength");

        // payload (n bytes)
        var payload = new byte[payloadLength];
        int readLength = stream.Read(payload, 0, payload.Length);
        if (readLength != payload.Length)
            throw new Exception("payloadの読み取りに失敗しました。");
        payloadLength -= (ushort)payload.Length;

        if (payloadLength > 0)
            throw new Exception("ペイロードの長さが一致しません。");

        return new Packet(sequence, payload);
    }

    public static ushort ReadShortBE(Stream stream, string paramName)
    {
        // ネットワークバイトオーダーで読み取る
        int readValue;

        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new PacketSerializationException($"{paramName}の読み取りに失敗しました。");
        ushort value = (ushort)(readValue << 8);
        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new PacketSerializationException($"{paramName}の読み取りに失敗しました。");
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

public class PacketPayloadSerializer
{
    public static byte[] Serialize(IPacketPayload payload)
    {
        using var stream = new MemoryStream();

        // packet kind (1 byte)
        stream.WriteByte((byte)payload.PayloadKind);

        if (payload is OkPayload)
        {
        }
        else if (payload is ErrorPayload errorPayload)
        {
            // code (2 bytes)
            PacketSerializer.WriteShortBE(stream, (ushort)errorPayload.Code, "code");
        }
        else if (payload is HandshakePayload)
        {
        }
        else if (payload is MovePayload)
        {
        }

        return stream.ToArray();
    }

    public static IPacketPayload Deserialize(byte[] source)
    {
        using var stream = new MemoryStream(source);

        int readValue;

        // packet kind (1 byte)
        readValue = stream.ReadByte();
        if (readValue == -1)
            throw new PacketSerializationException("kindの読み取りに失敗しました。");
        byte kind = (byte)readValue;

        switch (kind)
        {
            case (byte)PacketPayloadKind.Ok:
                return new OkPayload();

            case (byte)PacketPayloadKind.Error:
                // code (2 bytes)
                var errorCode = ToErrorCodeEnum(PacketSerializer.ReadShortBE(stream, "code"));
                return new ErrorPayload(errorCode);

            case (byte)PacketPayloadKind.Handshake:
                return new HandshakePayload();

            case (byte)PacketPayloadKind.Move:
                return new MovePayload();

            default:
                throw new PacketSerializationException($"未定義のPacketKind: 0x{kind:X2}");
        }
    }

    private static readonly HashSet<ushort> DefinedErrors =
        new(Enum.GetValues<PacketErrorCode>().Select(e => (ushort)e));

    private static PacketErrorCode ToErrorCodeEnum(ushort value)
    {
        if (!DefinedErrors.Contains(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"未定義のエラーコード: 0x{value:X4}");
        }

        return (PacketErrorCode)value;
    }
}

public class PacketSerializationException : Exception
{
    public ushort? Sequence { get; }

    public PacketSerializationException(string message) : base(message) { }

    public PacketSerializationException(string message, ushort sequence) : base(message)
    {
        Sequence = sequence;
    }
}
