namespace EscapeSinRetorno.Source.Boot
{
    public sealed class LaunchOptions
    {
        public bool AutoStart { get; set; }
        public bool AsClient { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 7777;

        public static LaunchOptions Default() => new LaunchOptions();
    }
}