using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CastIron.Engine;
using CastIron.Engine.Graphics.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
using Myra;
using Myra.Graphics2D.UI;
using Serilog;
using Serilog.Core;
using Slate.Client.UI;
using Slate.Client.UI.Views;
using StrongInject;
using ICamera = CastIron.Engine.Graphics.Camera.ICamera;

namespace Slate.Client
{
    public class RudeEngineGame : Game
    {
        private readonly PBREnvironment _lightsAndFog = PBREnvironment.CreateDefault();
        private readonly ModelInstance[] _cells = new ModelInstance[5 * 5];
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;
        private GameLifecycle _gameLifecycle = null!;
        private DeviceModelCollection _testModel = null!;
        private ICamera _camera = null!;
        private ModelInstance _characterModel;
        private ModelInstance _box;
        private Container _container;
        private readonly Options _options;
        private ChaseCamera _followCamera;
        private static Desktop _desktop;


        public RudeEngineGame(Options options)
        {
            Content.RootDirectory = "Content";

            _options = options;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1366,
                PreferredBackBufferHeight = 768,
                SynchronizeWithVerticalRetrace = false
            };
            IsFixedTimeStep = false;

            _graphics.PreparingDeviceSettings += Graphics_OnPreparingDeviceSettings;

            IsMouseVisible = true;
        }

        private (Logger logger, IUserLogEnricher UserLogEnricher) ConfigureLogging(Options options)
        {
            var slateLocalDir =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Slate");
            if (!Directory.Exists(slateLocalDir))
            {
                Directory.CreateDirectory(slateLocalDir);
            }

            var userLogEnricher = new UserLogEnricher();
            var logConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Slate Client")
                .Enrich.With(userLogEnricher)
                .WriteTo.File(Path.Combine(slateLocalDir, "log.txt"), rollingInterval: RollingInterval.Day);

            if (options.LogToConsole) logConfig = logConfig.WriteTo.Console();
            if (!string.IsNullOrWhiteSpace(options.SeqUrl)) logConfig = logConfig.WriteTo.Seq(options.SeqUrl);

            var log = logConfig.CreateLogger();
            return (log, userLogEnricher);
        }

        private void Graphics_OnPreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
        {
            _graphics.PreferMultiSampling = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        protected override void LoadContent()
        {
            MyraEnvironment.Game = this;
            _desktop = new Desktop();
            _desktop.Root = new Grid();

            var (log, userLogEnricher) = ConfigureLogging(_options);
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                log.Fatal(args.ExceptionObject as Exception, "Unhandled Exception");
                Log.CloseAndFlush();
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                log.Error(args.Exception, "Unobserved Exception");
            };

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _container = new Container(this, _options, _desktop, log, userLogEnricher);

            var gameComponents = _container.Resolve<IGameComponent[]>();
            foreach (var gameComponent in gameComponents.Value)
            {
                Components.Add(gameComponent);
            }
            _camera = _container.Resolve<ICamera>().Value;
            _camera.CameraBehaviour = _followCamera = new ChaseCamera();
            
            _gameLifecycle = _container.Resolve<GameLifecycle>().Value;
            _gameLifecycle.Start();
            
            var gltfFactory = new GltfModelFactory(GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
            _characterModel = gltfFactory.LoadModel(Path.Combine("Content", "MaleElfNew.glb")).DefaultModel.CreateInstance();
            _box = gltfFactory.LoadModel(Path.Combine("Content", "BoxAnimated.glb")).DefaultModel.CreateInstance();
            
            this.LoadComponentContent(Content);

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");

        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _testModel?.Dispose();
            _testModel = null!;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                if (_desktop?.Root is IMultipleItemsContainer container)
                {
                    foreach (var reloadablePanel in container.Widgets.OfType<ReloadablePanel>())
                    {
                        reloadablePanel.Rebuild();
                    }
                }
            }

            for (int z = 0; z < 5; ++z)
            {
                for (int x = 0; x < 5; ++x)
                {
                    var index = z * 5 + x;
                    _cells[index] = _testModel.DefaultModel.CreateInstance();
                    _cells[index].WorldMatrix = Matrix.CreateTranslation(x * 100, -25, z * 100);
                }
            }

            var delta = (float)gameTime.TotalGameTime.TotalSeconds;

            _characterModel.WorldMatrix = _cells[13].WorldMatrix
                                          + Matrix.CreateTranslation(new Vector3(5, 25, 5))
                                          + Matrix.CreateScale(0.5f + MathF.Sin(delta) * 1.5f);
            
            _characterModel.Armature.SetAnimationFrame((0, 0.5f, 0.5f), (1, 0.5f, 0.5f));
            _characterModel.WorldMatrix = _camera.World * _cells[13].WorldMatrix * Matrix.CreateTranslation(Vector3.Up * 1);
            _followCamera.FollowDistance = 5.0f;
            _followCamera.VerticalOffset = 5.0f;
            _followCamera.TargetLocation = _characterModel.WorldMatrix.Translation;
            _followCamera.Angle = (float)(MathF.PI * gameTime.TotalGameTime.TotalSeconds * 0.05f) ;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            var dc = new ModelDrawingContext(this.GraphicsDevice)
            {
                NearPlane = 0.1f
            };
            dc._View = _camera.View;
            dc._Projection = _camera.Projection;

            dc.DrawSceneInstances(_lightsAndFog, _cells);
            dc.DrawSceneInstances(_lightsAndFog, _characterModel, _box);
            
            _desktop.Render();

            base.Draw(gameTime);
        }
    }
}
