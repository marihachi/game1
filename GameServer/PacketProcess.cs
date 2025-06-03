using GameCommon;

namespace GameServer
{
    internal class PacketProcess
    {
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
            Console.WriteLine($"[{ctx.ClientEndpoint}] Received Handshake packet.");

            ctx.Success();
        }

        private static void ProcessMove(ServerContext ctx)
        {
            Console.WriteLine($"[{ctx.ClientEndpoint}] Received Move packet.");
            // Console.WriteLine($"X = {req.Data["X"]}, Y = {req.Data["Y"]}");

            ctx.Success();
        }
    }
}
