using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CastIron.Engine
{
    public class TaskDispatcher : UpdateableGameComponent
    {
        private static TaskCompletionSource<GameTime> thisUpdateSource = new();
        public static Task<GameTime> NextUpdate = thisUpdateSource.Task;
        private static ConcurrentQueue<Action> _mainThreadActions = new();

        public override void Update(GameTime gameTime)
        {
            var thisUpdate = thisUpdateSource;
            thisUpdateSource = new();
            NextUpdate = thisUpdateSource.Task;
            thisUpdate.SetResult(gameTime);

            while (_mainThreadActions.Any() && _mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    // LOG THIS
                }
            }
        }

        public static void FireOnUIAndForget(Action action)
        {
            _mainThreadActions.Enqueue(() => action());
        }

        public static Task FireOnUIAndForgetAsync(Action action)
        {
            var tcs = new TaskCompletionSource();

            _mainThreadActions.Enqueue(() =>
            {
                try
                {
                    action();
                    tcs.SetResult();
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            return tcs.Task;
        }

        public static async void FireAndForget(Func<Task> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            try
            {
                await Task.Run(async () => await task());
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