using System;
using System.IO;
using System.Threading.Tasks;
using Client.UI.ViewModels;
using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Debug;
using EmptyKeys.UserInterface.Generated;
using EmptyKeys.UserInterface.Media;
using EmptyKeys.UserInterface.Media.Effects;
using IdentityModel.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
using Networking;

namespace Client
{
    public class RudeEngineGame : Microsoft.Xna.Framework.Game
    {
        private readonly Options _options;
        
        private int _nativeScreenWidth;
        private int _nativeScreenHeight;

        private readonly PBREnvironment _lightsAndFog = PBREnvironment.CreateDefault();
        private DeviceModelCollection _testModel;
        private readonly ModelInstance[] _test = new ModelInstance[5 * 5];
        private GameUI _gameUi;
        private DebugViewModel _debugUi;
        private readonly GraphicsDeviceManager _graphics;

        public RudeEngineGame(Options options)
        {
            _options = options;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            _graphics.DeviceCreated += graphics_DeviceCreated;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {
            if (_gameUi != null)
            {
                Viewport viewPort = GraphicsDevice.Viewport;
                _gameUi.Resize(viewPort.Width, viewPort.Height);
            }
        }

        void graphics_DeviceCreated(object? sender, EventArgs e)
        {
            Engine engine = new MonoGameEngine(GraphicsDevice, _nativeScreenWidth, _nativeScreenHeight);
        }

        private void graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
        {
            _nativeScreenWidth = _graphics.PreferredBackBufferWidth;
            _nativeScreenHeight = _graphics.PreferredBackBufferHeight;

            _graphics.PreferMultiSampling = true;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        protected override void LoadContent()
        {
            this.IsMouseVisible = true;

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            Viewport viewport = GraphicsDevice.Viewport;
            _gameUi = new GameUI(viewport.Width, viewport.Height);
            var loginViewModel = new LoginViewModel(_options.AuthServer)
            {
                Username = "atomicblom",
                Password = "password"
            };
            loginViewModel.LoggedIn += LoginViewModelOnLoggedIn;
            _gameUi.DataContext = new GameUIViewModel
            {
                LoginViewModel = loginViewModel
            };
            Task.Run(loginViewModel.OnNavigatedTo);
            
            _debugUi = new DebugViewModel(_gameUi);

            FontManager.Instance.LoadFonts(Content);
            ImageManager.Instance.LoadImages(Content);
            SoundManager.Instance.LoadSounds(Content);
            EffectManager.Instance.LoadEffects(Content);

            var gltfFactory = new GltfModelFactory(this.GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
        }

        private async void LoginViewModelOnLoggedIn(object? sender, TokenResponse e)
        {
            _gameUi.Visibility = Visibility.Collapsed;
            var gameConnection = new GameConnection(_options.GameServer, _options.GameServerPort);
            var connectionResult = await gameConnection.Connect(e.AccessToken);

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

            _debugUi.Update();
            _gameUi.UpdateInput(gameTime.ElapsedGameTime.TotalMilliseconds);
            _gameUi.UpdateLayout(gameTime.ElapsedGameTime.TotalMilliseconds);

            for (int z = 0; z < 5; ++z)
            {
                for (int x = 0; x < 5; ++x)
                {
                    var index = z * 5 + x;
                    _test[index] = _testModel.DefaultModel.CreateInstance();
                    _test[index].WorldMatrix = Matrix.CreateTranslation(x * 100, 0, z * 100);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            var camPos = new Vector3(0, 25, 0);
            var modelPosition = new Vector3(50f, 0, 50f);

            var camX = Matrix.CreateWorld(Vector3.UnitY * 10, modelPosition - camPos, Vector3.UnitY);

            var dc = new ModelDrawingContext(this.GraphicsDevice)
            {
                NearPlane = 0.1f
            };
            
            dc.SetCamera(camX);
            
            dc.DrawSceneInstances(_lightsAndFog,
                _test);

            _gameUi.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
            _debugUi.Draw();

            base.Draw(gameTime);
        }
    }
}
