using DpdtInject.Injector.Src;
using Slate.Client.Services;
using Slate.Client.ViewModel.Services;

namespace Slate.Client
{
    public partial class Container : DefaultCluster
    {
        [DpdtBindingMethod]
        public void Bind()
        {
            //Bind<IAuthService>().To<AuthService>().WithSingletonScope();
            //Bind<GameConnection>().To<GameConnection>().WithSingletonScope();
            //Bind<ICharacterService>().To<CharacterService>().WithTransientScope();
        }
    }
}
