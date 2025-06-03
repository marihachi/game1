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
        public class ServerContext(Packet input)
        {
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

        public static void ProcessPacket(ServerContext ctx)
        {
            switch (ctx.Input.Kind)
            {
                case PacketKind.Handshake:
                    ProcessHandshake(ctx);
                    break;
                case PacketKind.Move:
                    ProcessMove(ctx);
                    break;
                default:
                    throw new Exception($"Received unknown packet kind: {ctx.Input.Kind}");
            }
        }

        private static void ProcessHandshake(ServerContext ctx)
        {
            Console.WriteLine("Received Handshake packet.");

            ctx.Success();
        }

        private static void ProcessMove(ServerContext ctx)
        {
            Console.WriteLine("Received Move packet.");
            // Console.WriteLine($"X = {req.Data["X"]}, Y = {req.Data["Y"]}");

            ctx.Success();
        }
    }
}
