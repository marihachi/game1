using GameCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class PacketProcess
    {
        public static byte[] ProcessPacket(byte[] packetData)
        {
            if (packetData.Length == 0)
            {
                throw new Exception("Received empty packet.");
            }

            var packet = Packet.FromBytes(packetData);
            switch (packet.Kind)
            {
                case PacketKind.Handshake:
                    Console.WriteLine("Received Handshake packet.");
                    break;
                case PacketKind.Move:
                    Console.WriteLine("Received Move packet.");
                    // Console.WriteLine($"X = {req.Data["X"]}, Y = {req.Data["Y"]}");
                    break;
                default:
                    throw new Exception($"Received unknown packet kind: {packet.Kind}");
            }

            var res = new Packet(PacketKind.Ok, []).ToBytes();

            return res;
        }
    }
}
