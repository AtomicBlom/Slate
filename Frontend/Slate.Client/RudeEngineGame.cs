using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MonoScene.Graphics;
using MonoScene.Graphics.Pipeline;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;

namespace Slate.Client
{
    public class RudeEngineGame : Microsoft.Xna.Framework.Game
    {
        private readonly Options _options;
        
        private int _nativeScreenWidth;
        private int _nativeScreenHeight;

        private readonly PBREnvironment _lightsAndFog = PBREnvironment.CreateDefault();
        private DeviceModelCollection _testModel;
        private readonly ModelInstance[] _test = new ModelInstance[5 * 5];
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private UiSystem _uiSystem;

        public RudeEngineGame(Options options)
        {
            _options = options;
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private void Window_ClientSizeChanged(object? sender, EventArgs e)
        {

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
            //Viewport viewport = GraphicsDevice.Viewport;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            var uiText = Content.Load<Texture2D>("UI/RPG_GUI_v1");
            var background = Content.Load<Texture2D>("UI/Paper_Background");
            var uiStyle = new UntexturedStyle(_spriteBatch)
            {
                Font = new GenericSpriteFont(font),
                TextFieldTexture = new NinePatch(new TextureRegion(uiText, 812, 612, 168, 34), 12, NinePatchMode.Tile),
                PanelTexture = new NinePatch(background, 0, NinePatchMode.Tile)
            };

            uiStyle.Font = new GenericSpriteFont(font);
            _uiSystem = new UiSystem(this, uiStyle);
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);

            var panel = new Panel(Anchor.Center, new Vector2(400, 100), Vector2.Zero, setHeightBasedOnChildren: true);
            panel.DrawColor.Set(Color.AliceBlue);
            _uiSystem.Add("LoginUI", panel);

            panel.AddChild(new Paragraph(Anchor.TopLeft, 100, "Username: "));
            panel.AddChild(new Paragraph(Anchor.AutoLeft, 100, "Password: ")
            {
                PositionOffset = new Vector2(0, 40)
            });

            var usernameField = new TextField(Anchor.TopRight, new Vector2(200, 48));
            usernameField.SetText("atomicblom");
            usernameField.TextOffsetX = 12;
            panel.AddChild(usernameField);
            
            var passwordField = new TextField(Anchor.AutoRight, new Vector2(200, 48));
            passwordField.SetText("password");
            passwordField.TextOffsetX = 12;
            panel.AddChild(passwordField);

            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(200, 48), "Log in")
            {
                OnPressed = (a) =>
                {
                    Console.WriteLine("Beep");
                }
            });

            //var loginViewModel = new LoginViewModel(_options.AuthServer)
            //{
            //    Username = "atomicblom",
            //    Password = "password"
            //};
            //loginViewModel.LoggedIn += LoginViewModelOnLoggedIn;
            //Task.Run(loginViewModel.OnNavigatedTo);

            var gltfFactory = new GltfModelFactory(GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
        }

        //private async void LoginViewModelOnLoggedIn(object? sender, TokenResponse e)
        //{
        //    //_gameUi.Visibility = Visibility.Collapsed;
        //    var gameConnection = new GameConnection(_options.GameServer, _options.GameServerPort);
        //    var connectionResult = await gameConnection.Connect(e.AccessToken);

        //    if (connectionResult.WasSuccessful)
        //    {
        //        var existingScreen = _gameUi.CurrentScreen.Children.FirstOrDefault();
        //        if (existingScreen != null)
        //        {
        //            existingScreen.Visibility = Visibility.Collapsed; // Animate off?
        //        }

        //        _gameUi.CurrentScreen.Children.Remove(existingScreen);
        //        var characterListViewModel = new CharacterListViewModel(gameConnection)
        //        {
        //        };
        //        _gameUi.CurrentScreen.Children.Add(
        //            new CharacterListScreen()
        //            {
        //                DataContext = characterListViewModel
        //            });

        //        await Task.Run(characterListViewModel.OnNavigatedTo);
        //    }

        //}

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
            
            _uiSystem.Update(gameTime);

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
            _uiSystem.DrawEarly(gameTime, _spriteBatch);

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
            
            this._uiSystem.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
