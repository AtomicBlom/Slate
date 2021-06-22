using System;
using Slate.Backend.Shared;
using StrongInject;

namespace Slate.FakeCDN
{
    public partial class FakeCDNContainer : CoreServicesModule, IContainer<HeartbeatService>
    {
        public FakeCDNContainer(IServiceProvider serviceProvider) : base(serviceProvider) { }
    }
}