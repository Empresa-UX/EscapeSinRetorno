using System;
using EscapeSinRetorno.Source.Boot;

namespace EscapeSinRetorno
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var opts = ParseArgs(args);

            Console.WriteLine("==============================================");
            Console.WriteLine(" Escape Sin Retorno - Cliente");
            Console.WriteLine("==============================================");
            Console.WriteLine($" Args: {(opts.AutoStart ? (opts.AsClient ? $"client {opts.Host}:{opts.Port}" : "offline") : "menu")}");
            Console.WriteLine(" Tips: --offline | --client --host 127.0.0.1 --port 7777");
            Console.WriteLine("==============================================");

            using var game = new Game1(opts);
            game.Run();
        }

        static LaunchOptions ParseArgs(string[] args)
        {
            var lo = new LaunchOptions(); // clase normal; set directo

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i].ToLowerInvariant();
                if (a == "--offline") { lo.AutoStart = true; lo.AsClient = false; }
                else if (a == "--client") { lo.AutoStart = true; lo.AsClient = true; }
                else if (a == "--host" && i + 1 < args.Length) { lo.Host = args[++i]; }
                else if (a == "--port" && i + 1 < args.Length && int.TryParse(args[i + 1], out var p) && p > 0 && p < 65536)
                { lo.Port = p; i++; }
            }

            return lo;
        }
    }
}