using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CastIron.Engine
{
    public class TaskDispatcher : UpdateableGameComponent
    {
        private static TaskCompletionSource<GameTime> thisUpdateSource = new();
        public static Task<GameTime> NextUpdate = thisUpdateSource.Task;

        public override void Update(GameTime gameTime)
        {
            var thisUpdate = thisUpdateSource;
            thisUpdateSource = new();
            NextUpdate = thisUpdateSource.Task;
            thisUpdate.SetResult(gameTime);
        }

        public static async void FireAndForget(Func<Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            try
            {
                await task();
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1)
            {
                var exception = e.InnerExceptions.Single();
                //FIXME: Log this
            }
            catch (Exception e)
            {
                //FIXME: Log this

            }
        }

        public static async void FireAndForget<T>(Func<Task<T>> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            try
            {
                await task();
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1)
            {
                var exception = e.InnerExceptions.Single();
                //FIXME: Log this
            }
            catch (Exception e)
            {
                //FIXME: Log this

            }
        }
    }
}