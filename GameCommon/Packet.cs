namespace GameCommon
{
    public enum PacketKind : byte
    {
        Ok = 0x01,
        Error = 0x02,
        Handshake = 0x03,
        Move = 0x04,
    }

    public class Packet(PacketKind kind, byte[] payload)
    {
        public PacketKind Kind { get; } = kind;
        public byte[] Payload { get; } = payload;

        public static Packet FromBytes(byte[] source)
        {
            int readValue;

            using var stream = new MemoryStream(source);

            // count (2)
            // count represents the length of the data following itself in the packet.
            ushort count = ReadShortBE(stream, "count");

            if (count < 1)
                throw new Exception("countは1以上である必要があります。");

            // kind (1)
            readValue = stream.ReadByte();
            if (readValue == -1)
                throw new Exception("kindの読み取りに失敗しました。");
            byte kind = (byte)readValue;
            count--;

            // payload (n)
            var payload  = new byte[count];
            int readLength = stream.Read(payload, 0, payload.Length);
            if (readLength != payload.Length)
                throw new Exception("payloadの読み取りに失敗しました。");
            count -= (ushort)payload.Length;

            if (count > 0)
                throw new Exception("パケットの長さが不正です。");

            return new Packet((PacketKind)kind, payload);
        }

        public byte[] ToBytes()
        {
            using var stream = new MemoryStream();

            // count (2)
            ushort count = (ushort)(Payload.Length + 1);
            WriteShortBE(stream, count, "count");

            // kind (1)
            stream.WriteByte((byte)Kind);

            // payload (n)
            stream.Write(Payload, 0, Payload.Length);

            return stream.ToArray();
        }

        public static ushort ReadShortBE(Stream stream, string paramName)
        {
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
            // 上位バイトを先に書き込む（ビッグエンディアン）
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

        public static ErrorPayload FromBytes(byte[] source)
        {
            using var stream = new MemoryStream(source);
            ushort code = Packet.ReadShortBE(stream, "code");
            return new ErrorPayload(ToErrorCode(code));
        }

        private static ErrorCode ToErrorCode(ushort value)
        {
            if (DefinedErrors.Contains(value))
            {
                return (ErrorCode)value;
            }

            throw new ArgumentOutOfRangeException(nameof(value), $"未定義のErrorCode値: 0x{value:X4}");
        }
    }
}
