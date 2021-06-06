using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
using SharpGLTF.Schema2;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;

namespace Client
{
    public class RudeEngineGame : Game
    {
        VertexBuffer vertexBuffer;
        //BasicEffect basicEffect;
        //Matrix world = Matrix.CreateTranslation(0, 0, 0);
        //Matrix view = Matrix.CreateLookAt(new Vector3(25, 25, 25), new Vector3(50, 0, 50), new Vector3(0, 0, 1));
        //Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        private int nativeScreenWidth;
        private int nativeScreenHeight;

        //private GraphicsDeviceManager _graphics;
        //private SpriteBatch _spriteBatch;
        //private ModelRoot _test;
        //private MeshCollection _meshCollection;

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();
        private DeviceModelCollection _testModel;
        private ModelInstance[] _test = new ModelInstance[5 * 5];
        private GameUI _gameUI;
        private DebugViewModel _debugUI;
        private GraphicsDeviceManager graphics;

        public RudeEngineGame()
        {
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

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //_spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //basicEffect = new BasicEffect(GraphicsDevice);

            //VertexPositionColor[] vertices = new VertexPositionColor[6];
            //vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Gray);
            //vertices[1] = new VertexPositionColor(new Vector3(100f, 0, 0), Color.White);
            //vertices[2] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.White);
            //vertices[3] = new VertexPositionColor(new Vector3(100f, 0, 0), Color.White);
            //vertices[4] = new VertexPositionColor(new Vector3(100f, 0, 100f), Color.Gray);
            //vertices[5] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.White);

            //vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
            //vertexBuffer.SetData(vertices);

            this.IsMouseVisible = true;

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");
            FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            Viewport viewport = GraphicsDevice.Viewport;
            _gameUI = new GameUI(viewport.Width, viewport.Height);
            //viewModel = new BasicUIViewModel();
            //_gameUI.DataContext = viewModel;
            _debugUI = new DebugViewModel(_gameUI);

            FontManager.Instance.LoadFonts(Content);
            ImageManager.Instance.LoadImages(Content);
            SoundManager.Instance.LoadSounds(Content);
            EffectManager.Instance.LoadEffects(Content);

            var gltfFactory = new GltfModelFactory(this.GraphicsDevice);
            _testModel = gltfFactory.LoadModel(Path.Combine($"Content", "Cell100.glb"));
            //var pbrFactory = new PBRMeshFactory(this.GraphicsDevice);

            //var modelPath = ModelRoot.Load(Path.Combine($"Content", "BoxAnimated.glb"));
            //var contentMeshes = gltfFactory.ReadMeshContent(modelPath.LogicalMeshes.Take(1));

            //_meshCollection = pbrFactory.CreateMeshCollection(contentMeshes.Materials, contentMeshes.Meshes);

            Task.Run(() => Login());

        }

        private async Task Login()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            var result = await client.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,

                ClientId = "Launcher",
                ClientSecret = "secret",
                Scope = "account",

                UserName = "atomicblom",
                Password = "password",

            });
            
            Console.WriteLine(result.Json);
            Console.WriteLine("\n\n");
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

            //if (_test is null) _test = _testModel.DefaultModel.CreateInstance();
            
            //_test.Armature.SetAnimationFrame((0, 0.5f, 0.5f), (1, 0.5f, 0.5f));
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            var camPos = new Vector3(0, (float)Math.Sin((float)gameTime.TotalGameTime.TotalSeconds) * -25, 0);
            var modelPosition = new Vector3(50f, 0, 50f);

            var camX = Matrix.CreateWorld(Vector3.UnitY * 10, modelPosition - camPos, Vector3.UnitY);
            //_test.WorldMatrix = Matrix.CreateRotationY(0.25f * (float)gameTime.TotalGameTime.TotalSeconds) * Matrix.CreateTranslation(modelPosition);

            var dc = new ModelDrawingContext(this.GraphicsDevice)
            {
                NearPlane = 0.1f
            };
            
            dc.SetCamera(camX);
            //dc.DrawMesh(_LightsAndFog, _meshCollection[0], modelX);
            dc.DrawSceneInstances(_LightsAndFog,
                _test);

            // TODO: Add your drawing code here
            //basicEffect.World = world;
            //basicEffect.View = view;
            //basicEffect.Projection = projection;
            //basicEffect.VertexColorEnabled = true;

            //GraphicsDevice.SetVertexBuffer(vertexBuffer);

            //RasterizerState rasterizerState = new RasterizerState();
            //rasterizerState.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = rasterizerState;

            //foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            //}

            _gameUI.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);
            _debugUI.Draw();

            base.Draw(gameTime);
        }
    }
}
