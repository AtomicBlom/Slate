using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StopMMOping
{
    public class RudeEngineGame : Game
    {
        VertexBuffer vertexBuffer;

        BasicEffect basicEffect;
        Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(25, 25, 25), new Vector3(50, 0, 50), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

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
            vertices[2] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.Green);
            vertices[3] = new VertexPositionColor(new Vector3(100f, 0, 0), Color.Purple);
            vertices[4] = new VertexPositionColor(new Vector3(100f, 0, 100f), Color.Red);
            vertices[5] = new VertexPositionColor(new Vector3(0, 0, 100f), Color.Blue);
            
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            Vector3 cameraPosition = new Vector3(30.0f, 30.0f, 30.0f);
            Vector3 cameraTarget = new Vector3(0.0f, 0.0f, 0.0f); // Look back at the origin

            float fovAngle = MathHelper.ToRadians(45);  // convert 45 degrees to radians
            float aspectRatio = _graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight;
            float near = 0.01f; // the near clipping plane distance
            float far = 100f; // the far clipping plane distance

            Matrix world = Matrix.CreateTranslation(10.0f, 0.0f, 10.0f);
            Matrix view = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(fovAngle, aspectRatio, near, far);


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;

            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}
