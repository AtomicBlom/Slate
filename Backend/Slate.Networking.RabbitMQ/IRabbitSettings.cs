namespace Slate.Networking.RabbitMQ
{
    public interface IRabbitSettings
    {
        string Hostname { get; }
        string VirtualHost { get; }
        int Port { get; }
        string Username { get; }
        string Password { get; }
        string ClientName { get; }
    }
}