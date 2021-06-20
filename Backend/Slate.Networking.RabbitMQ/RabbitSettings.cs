namespace Slate.Networking.RabbitMQ
{
    public class RabbitSettings : IRabbitSettings
    {
        public const string SectionName = "RabbitMQ";

        public string Hostname { get; set; } = "localhost";
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string ClientName { get; set;  } = "RabbitMQ client";
        public bool IncludeMessageContentsInLogs { get; } = true;
    }
}