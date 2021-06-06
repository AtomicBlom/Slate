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

namespace Client
{
    public class RudeEngineGame : Game
    {
        private readonly Options _options;
        
        private int nativeScreenWidth;
        private int nativeScreenHeight;

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();
        private DeviceModelCollection _testModel;
        private ModelInstance[] _test = new ModelInstance[5 * 5];
        private GameUI _gameUI;
        private DebugViewModel _debugUI;
        private GraphicsDeviceManager graphics;

        public RudeEngineGame(Options options)
        {
            _options = options;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            graphics.DeviceCreated += graphics_DeviceCreated;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (_gameUI != null)
            {
                Viewport viewPort = GraphicsDevice.Viewport;
                _gameUI.Resize(viewPort.Width, viewPort.Height);
            }
        }

        void graphics_DeviceCreated(object sender, EventArgs e)
        {
            Engine engine = new MonoGameEngine(GraphicsDevice, nativeScreenWidth, nativeScreenHeight);
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            nativeScreenWidth = graphics.PreferredBackBufferWidth;
            nativeScreenHeight = graphics.PreferredBackBufferHeight;

            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
        }

        protected override void LoadContent()
        {
            this.IsMouseVisible = true;

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            Viewport viewport = GraphicsDevice.Viewport;
            _gameUI = new GameUI(viewport.Width, viewport.Height);
            var loginViewModel = new LoginViewModel(_options.AuthServer);
            loginViewModel.LoggedIn += LoginViewModelOnLoggedIn;
            _gameUI.DataContext = new GameUIViewModel()
            {
                LoginViewModel = loginViewModel
            };
            Task.Run(loginViewModel.OnNavigatedTo);
            
            _debugUI = new DebugViewModel(_gameUI);

            FontManager.Instance.LoadFonts(Content);
            ImageManager.Instance.LoadImages(Content);
            SoundManager.Instance.LoadSounds(Content);
            EffectManager.Instance.LoadEffects(Content);

            var gltfFactory = new GltfModelFactory(this.GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
        }

        private void LoginViewModelOnLoggedIn(object? sender, TokenResponse e)
        {
            _gameUI.Visibility = Visibility.Collapsed;
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _testModel?.Dispose();
            _testModel = null;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _debugUI.Update();
            _gameUI.UpdateInput(gameTime.ElapsedGameTime.TotalMilliseconds);
            _gameUI.UpdateLayout(gameTime.ElapsedGameTime.TotalMilliseconds);

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
            
            dc.DrawSceneInstances(_LightsAndFog,
                _test);

            _gameUI.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
            _debugUI.Draw();

            base.Draw(gameTime);
        }
    }
}
