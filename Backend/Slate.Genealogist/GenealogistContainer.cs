using System;
using Slate.Backend.Shared;
using StrongInject;

namespace Slate.Genealogist
{
    public partial class GenealogistContainer : CoreServicesModule, 
        IContainer<HeartbeatService>,
        IContainer<GracefulShutdownService>
    {
        public GenealogistContainer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}