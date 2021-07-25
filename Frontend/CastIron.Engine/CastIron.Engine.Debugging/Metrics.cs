using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CastIron.Engine.Debugging
{
	[PublicAPI]
    public class Metrics : DrawableGameComponent
    {
	    public static void Install(Game game)
	    {
		    game.Components.Add(new Metrics(game));
	    }

        private const int MaxSamples = 100;

        private GraphicsMetrics _lastMetrics;

        private Metrics(Game game) : base(game)
	    {
		    Visible = false;
		    _debugInfoSink = game.Services.GetService<IDebugInfoSink>();
	    }
		
        private readonly Queue<float> _timesPerFrame = new(MaxSamples);
		private float _totalFrameTimes;
		private DateTime _nextUpdateTick;
		public float ManagedMemory { get; private set; }
		public float PeakManagedMemoryUsage { get; private set; }

		public float MaxFPSLastTick { get; private set; }
		public float MinFPSLastTick { get; private set; }
		public float CurrentFPS { get; private set; }
		public float MaxMemoryLastTick { get; private set; }

		private float _maxFPSLastTick = float.MinValue;
		private float _maxMemoryLastTick = float.MinValue;
        private float _minFPSLastTick = float.MaxValue;
		
        private bool _lastKeyboardState;
		private readonly IDebugInfoSink _debugInfoSink;

		public override void Update(GameTime gameTime)
		{
			var isKeyDown = Keyboard.GetState().IsKeyDown(Keys.F3);
			
            if (isKeyDown && !_lastKeyboardState)
			{
				Visible = !Visible;
			}

			_lastKeyboardState = isKeyDown;

            //FPS Metrics
            if (_timesPerFrame.Count == MaxSamples)
			{
				_totalFrameTimes -= _timesPerFrame.Dequeue();
			}

			var frameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

			_totalFrameTimes += frameTime;
			_timesPerFrame.Enqueue(frameTime);

			if (_timesPerFrame.Count < MaxSamples) return;

			CurrentFPS = 1 / (_totalFrameTimes / _timesPerFrame.Count);

			_maxMemoryLastTick = MathHelper.Max(_maxMemoryLastTick, ManagedMemory);
			_maxFPSLastTick = MathHelper.Max(_maxFPSLastTick, CurrentFPS);
			_minFPSLastTick = MathHelper.Min(_minFPSLastTick, CurrentFPS);
			
            //Memory
            // ReSharper disable once PossibleLossOfFraction
            ManagedMemory = GC.GetTotalMemory(false) / (float)(1_024 * 1_024);
			if (ManagedMemory > PeakManagedMemoryUsage)
			{
				PeakManagedMemoryUsage = ManagedMemory;
			}

			if (DateTime.Now > _nextUpdateTick)
			{
				MaxFPSLastTick = _maxFPSLastTick;
				MaxMemoryLastTick = _maxMemoryLastTick;
				MinFPSLastTick = _minFPSLastTick;
				_nextUpdateTick = DateTime.Now + TimeSpan.FromSeconds(1);

				_maxFPSLastTick = float.MinValue;
				_maxMemoryLastTick = float.MinValue;
				_minFPSLastTick = float.MaxValue;
			}

            if (Visible) AddMetrics();
        }

        private void AddMetrics()
        {
			const DebugInfoCorner corner = DebugInfoCorner.TopRight;
            _debugInfoSink.AddDebugInfo(corner, "FPS", string.Empty)
                .Add("Current", $"{CurrentFPS:#0.00}")
                .Add("Min/s", $"{MinFPSLastTick:0.00}")
                .Add("Max/s", $"{MaxFPSLastTick:0.00}");
            _debugInfoSink.AddDebugInfo(corner, "Memory (MB)", string.Empty)
                .Add("Current", $"{ManagedMemory:0.00}")
                .Add("Max/s", $"{MaxMemoryLastTick:0.00}")
                .Add("Peak", $"{PeakManagedMemoryUsage:0.00}");
            _debugInfoSink.AddDebugInfo(corner, "DrawCount", _lastMetrics.DrawCount);
            _debugInfoSink.AddDebugInfo(corner, "SpriteCount", _lastMetrics.SpriteCount);
            _debugInfoSink.AddDebugInfo(corner, "TextureCount", _lastMetrics.TextureCount);
		}

        public override void Draw(GameTime gameTime)
		{
			_lastMetrics = GraphicsDevice.Metrics;
        }
    }
}
