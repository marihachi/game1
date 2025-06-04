namespace GameCommon;

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
