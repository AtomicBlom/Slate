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
using MonoGame.Extended;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
using Slate.Client.Services;
using Slate.Client.UI;
using Slate.Client.UI.Views;

namespace Slate.Client
{
    internal class DebugMovementComponent : SimpleGameComponent
    {
        private readonly IInputBindingManager<GameInputState> _inputManager;
        private readonly ICamera _camera;
        private readonly FreeRoamingCamera _debugCameraBehaviour;

        public DebugMovementComponent(IInputBindingManager<GameInputState> inputManager, ICamera camera)
        {
            _inputManager = inputManager;
            _camera = camera;
            _debugCameraBehaviour = new FreeRoamingCamera();
        }

        public override void Update(GameTime gameTime)
        {
            if (_inputManager.Button(MainMenuAction.EnterDebugMode).Released)
            {
                _inputManager.CurrentState = GameInputState.Debug;
                _camera.CameraBehaviour = _debugCameraBehaviour;
            }

            if (_inputManager.Button(DebugControls.ExitDebugMode).Released)
            {
                _inputManager.CurrentState = GameInputState.MainMenu;
                _camera.CameraBehaviour = null;
            }

            if (_inputManager.CurrentState == GameInputState.Debug)
            {
                _debugCameraBehaviour.MoveLeftRight(_inputManager.GetAxis(DebugControls.MoveLeftRightAxis).Value, gameTime);
                _debugCameraBehaviour.MoveForwardBackward(_inputManager.GetAxis(DebugControls.MoveForwardBackAxis).Value, gameTime);
                _debugCameraBehaviour.MoveUpDown(_inputManager.GetAxis(DebugControls.MoveUpDownAxis).Value, gameTime);

                _debugCameraBehaviour.RotateUpOrDown(_inputManager.GetAxis(DebugControls.PitchAxis).Value, gameTime);
                _debugCameraBehaviour.RotateLeftOrRight(_inputManager.GetAxis(DebugControls.YawAxis).Value, gameTime);
                _debugCameraBehaviour.RotateSideToSide(_inputManager.GetAxis(DebugControls.RollAxis).Value, gameTime);
            }
        }
    }

    public class RudeEngineGame : Game
    {
        private readonly Options _options;
        
        private readonly PBREnvironment _lightsAndFog = PBREnvironment.CreateDefault();
        private readonly ModelInstance[] _test = new ModelInstance[5 * 5];
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch = null!;
        private UiSystem _uiSystem = null!;
        private GameLifecycle _gameLifecycle = null!;
        private DeviceModelCollection _testModel = null!;
        private InputBindingManager<GameInputState> _playerInput = null!;
        private ICamera _camera = null!;

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
            _playerInput = this.AddComponentAndService(GameInputBindings.CreateInputBindings(this));
            var debugInfoSink = this.AddComponentAndService<IDebugInfoSink>(new DebugInfoSink(this) { Enabled = true });
            Metrics.Install(this);
            _camera = this.AddComponentAndService<ICamera>(new Camera(GraphicsDevice, debugInfoSink));
            this.IsMouseVisible = true;
            this.AddComponentAndService(new DebugMovementComponent(_playerInput, _camera));

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

            var authService = new AuthService(_options.AuthServer);
            var gameConnection = new GameConnection(_options.GameServer, _options.GameServerPort, authService);
            _gameLifecycle = new GameLifecycle(_uiSystem, authService, gameConnection);
            _gameLifecycle.Start();
            
            var gltfFactory = new GltfModelFactory(GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
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
                    _test[index] = _testModel.DefaultModel.CreateInstance();
                    _test[index].WorldMatrix = Matrix.CreateTranslation(x * 100, -25, z * 100);
                }
            }

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

            
            dc.SetCamera(_camera.View);
            dc.SetProjectionMatrix(_camera.Projection);
            
            dc.DrawSceneInstances(_lightsAndFog, _test);
            
            _uiSystem.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
