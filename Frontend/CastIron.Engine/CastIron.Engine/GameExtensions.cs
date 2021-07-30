using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace CastIron.Engine
{
    public static class GameExtensions
    {
        public static TServiceType AddComponentAndService<TServiceType>(this Game game, TServiceType service)
        {
            game.Services.AddService(service.GetType(), service);
            if (service is IGameComponent component)
            {
                game.Components.Add(component);
            }

            return service;
        }

        public static void LoadComponentContent(this Game game, ContentManager contentManager)
        {
            foreach (var loadContent in game.Components.OfType<ILoadContent>())
            {
                loadContent.LoadContent(contentManager);
            }
        }
    }
}
