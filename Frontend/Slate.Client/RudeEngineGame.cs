using System;
using System.IO;
using System.Linq;
using CastIron.Engine;
using CastIron.Engine.Debugging;
using CastIron.Engine.Graphics.Camera;
using CastIron.Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Font;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Style;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
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
        private UiSystem _uiSystem = null!;
        private GameLifecycle _gameLifecycle = null!;
        private DeviceModelCollection _testModel = null!;
        private InputBindingManager<GameInputState> _playerInput = null!;
        private ICamera _camera = null!;
        private ModelInstance _characterModel;
        private MeshCollection _meshCollection;
        private ModelInstance _test;
        private ModelInstance _box;
        private Container _container;
        private readonly Options _options;

        public RudeEngineGame(Options options)
        {
            _options = options;
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1366,
                PreferredBackBufferHeight = 768
            };
            
            _graphics.PreparingDeviceSettings += Graphics_OnPreparingDeviceSettings;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
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
            //_playerInput = this.AddComponentAndService(GameInputBindings.CreateInputBindings(this));
            //var debugInfoSink = this.AddComponentAndService<IDebugInfoSink>(new DebugInfoSink(this) { Enabled = true });
            //Metrics.Install(this);
            //_camera = this.AddComponentAndService<ICamera>(new Camera(GraphicsDevice, debugInfoSink));
            this.IsMouseVisible = true;
            //this.AddComponentAndService(new DebugMovementComponent(_playerInput, _camera));

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            var uiTexture = Content.Load<Texture2D>("UI/MockUI");
            var uiStyle = new UntexturedStyle(_spriteBatch)
            {
                Font = new GenericSpriteFont(font),
                TextFieldTexture = new NinePatch(new TextureRegion(uiTexture, 128, 216, 128, 32), 8),
                PanelTexture = new NinePatch(new TextureRegion(uiTexture, 384, 128, 128, 128), 20),
                ButtonTexture = new NinePatch(new TextureRegion(uiTexture, 128, 128, 128, 32), 8),
                RadioTexture = new NinePatch(new TextureRegion(uiTexture, 128, 172, 32, 32), 0),
                RadioCheckmark = new TextureRegion(uiTexture, 192, 172, 32, 32)
            };

            uiStyle.Font = new GenericSpriteFont(font);
            _uiSystem = new UiSystem(this, uiStyle);
            _container = new Container(this, _options, _uiSystem);
            var gameComponents = _container.Resolve<IGameComponent[]>();
            foreach (var gameComponent in gameComponents.Value)
            {
                this.AddComponentAndService(gameComponent);
            }
            _gameLifecycle = _container.Resolve<GameLifecycle>().Value;
            _camera = _container.Resolve<ICamera>().Value;

            //var authService = new AuthService(_options.AuthServer);
            //var gameConnection = new GameConnection(_options.GameServer, _options.GameServerPort, authService);
            //_gameLifecycle = new GameLifecycle(_uiSystem, authService, gameConnection);
            _gameLifecycle.Start();
            
            var gltfFactory = new GltfModelFactory(GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
            _characterModel = gltfFactory.LoadModel(Path.Combine("Content", "MaleElfNew.glb")).DefaultModel.CreateInstance();
            _box = gltfFactory.LoadModel(Path.Combine("Content", "BoxAnimated.glb")).DefaultModel.CreateInstance();
            
            this.LoadComponentContent(Content);

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
                foreach (var reloadablePanel in _uiSystem.GetRootElements().Select(re => re.Element).OfType<ReloadablePanel>())
                {
                    reloadablePanel.Rebuild();
                }
            }
            
            _uiSystem.Update(gameTime);

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
            _characterModel.WorldMatrix = _cells[13].WorldMatrix + Matrix.CreateTranslation(Vector3.Up * 25);
            _box.WorldMatrix = _cells[13].WorldMatrix + Matrix.CreateTranslation(Vector3.Up * 25);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _uiSystem.DrawEarly(gameTime, _spriteBatch);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            var dc = new ModelDrawingContext(this.GraphicsDevice)
            {
                NearPlane = 0.1f
            };

            dc.SetProjectionMatrix(_camera.Projection);
            dc.SetCamera(_camera.View);

            dc.DrawSceneInstances(_lightsAndFog, _cells);
            dc.DrawSceneInstances(_lightsAndFog, _characterModel, _box);
            
            _uiSystem.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
