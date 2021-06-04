using System.IO;
using System.Linq;
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
        BasicEffect basicEffect;
        Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(25, 25, 25), new Vector3(50, 0, 50), new Vector3(0, 0, 1));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ModelRoot _test;
        private MeshCollection _meshCollection;

        private PBREnvironment _LightsAndFog = PBREnvironment.CreateDefault();

        public RudeEngineGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            basicEffect = new BasicEffect(GraphicsDevice);

            VertexPositionColor[] vertices = new VertexPositionColor[6];
            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Gray);
            vertices[1] = new VertexPositionColor(new Vector3(100f, 0, 0), Color.White);
            vertices[2] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.White);
            vertices[3] = new VertexPositionColor(new Vector3(100f, 0, 0), Color.White);
            vertices[4] = new VertexPositionColor(new Vector3(100f, 0, 100f), Color.Gray);
            vertices[5] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.White);
            
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            var gltfFactory = new GltfModelFactory(this.GraphicsDevice);
            var pbrFactory = new PBRMeshFactory(this.GraphicsDevice);

            var modelPath = ModelRoot.Load(Path.Combine($"Content", "MaleElf.glb"));
            var contentMeshes = gltfFactory.ReadMeshContent(modelPath.LogicalMeshes.Take(1));

            _meshCollection = pbrFactory.CreateMeshCollection(contentMeshes.Materials, contentMeshes.Meshes);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _meshCollection?.Dispose();
            _meshCollection = null!;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            var camPos = Vector3.Zero;
            var modelPosition = new Vector3(100f, 0, 0);

            var camX = Matrix.CreateWorld(Vector3.Zero, modelPosition - camPos, Vector3.UnitY);
            var modelX = Matrix.CreateRotationY(0.25f * (float)gameTime.TotalGameTime.TotalSeconds) * Matrix.CreateTranslation(modelPosition);

            var dc = new ModelDrawingContext(this.GraphicsDevice);
            dc.NearPlane = 0.1f;
            dc.SetCamera(camX);
            dc.DrawMesh(_LightsAndFog, _meshCollection[0], modelX);

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

            base.Draw(gameTime);
        }
    }
}
