using Slate.Networking.RabbitMQ.StrongInject;
using StrongInject;

namespace Slate.Overseer
{
    [Register(typeof(ApplicationLauncher))]
    [RegisterModule(typeof(RabbitMQModule))]
    internal partial class OverseerContainer : IContainer<ApplicationLauncher>
    {
    }
}
