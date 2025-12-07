using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogame_test1.Classes;
using Monogame_test1.Classes.OwnMathForVectors;
using System;
using System.Collections.Generic;
using System.Threading;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Monogame_test1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private List<Particle> particles = new();
        public int BrushSize { get; set; } = 0;

        private MouseState prevMouseState {  get; set; }
        private MouseState curMouseState { get; set; }

        private CancellationTokenSource cancellationTokenSource;
        private bool isExiting = false;


        private List<Particle>[,] particlesGrid;
        int gridHeight;
        int gridWidth;
        int gridSize = 10;

        Texture2D circle {get; set;}

        Random rand = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            cancellationTokenSource = new CancellationTokenSource();

            gridWidth = Window.ClientBounds.Width / gridSize;
            gridHeight = Window.ClientBounds.Height / gridSize;

            particlesGrid = new List<Particle>[gridWidth,gridHeight];

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    particlesGrid[i, j] = new List<Particle>();
                }
            }

            //Thread CollisionsSolver = new(() => CollisionsThread(cancellationTokenSource.Token));
            //CollisionsSolver.IsBackground = true;
            //CollisionsSolver.Start();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            circle = Content.Load<Texture2D>("circle_texture");
        }

        protected override void Update(GameTime gameTime)
        {
            curMouseState = Mouse.GetState(Window);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (prevMouseState.ScrollWheelValue < curMouseState.ScrollWheelValue)
            {
                BrushSize++;
            }
            else if(curMouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
            {
                BrushSize = BrushSize == 0 ? BrushSize : BrushSize - 1;
            }

            if (BrushSize > 0)
            {
                if (curMouseState.LeftButton == ButtonState.Pressed)
                {
                    particles.Add(new Particle(new System.Numerics.Vector2(curMouseState.X, curMouseState.Y), BrushSize * 10));
                }
            }

            Window.Title = $"{BrushSize.ToString()}:{curMouseState.ScrollWheelValue}:{prevMouseState.ScrollWheelValue}:{particles.Count}";

            foreach (var particle in particles)
            {
                particle.Position += particle.Velocity;
                
            }

            prevMouseState = curMouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            
            _spriteBatch.Begin();

            _spriteBatch.Draw(circle, new Rectangle(curMouseState.X- BrushSize * 10 / 2, curMouseState.Y- BrushSize * 10 / 2, BrushSize*10, BrushSize*10), Color.Black);

            foreach (var particle in particles)
            {
                try
                {
                    particlesGrid[(int)particle.Position.X / gridSize, (int)particle.Position.Y / gridSize].Add(particle);
                }
                catch
                {

                }
                particle.Velocity = OwnMath.Normalize(new Vector2(curMouseState.X, curMouseState.Y) - particle.Position);
                _spriteBatch.Draw(circle, new Rectangle((int)particle.Position.X - particle.Size / 2, (int)particle.Position.Y - particle.Size / 2, particle.Size, particle.Size), Color.Black);
            }

            CalcCollisions();

            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void CalcCollisions()
        {
            int gridX;
            int gridY;
            Vector2 normal;
            float depth;
            try
            {
                if (particles.Count > 1)
                {
                    foreach(var particle in particles)
                    {
                        gridX = (int)particle.Position.X / gridSize;
                        gridY = (int)particle.Position.Y / gridSize;
                        for (int gridCheckX = -1; gridCheckX < 2; gridCheckX++)
                        {
                            for (int gridCheckY = -1; gridCheckY < 2; gridCheckY++)
                            {
                                foreach (var particle1 in particlesGrid[gridX, gridY])
                                {
                                    foreach (var particle2 in particlesGrid[gridX + gridCheckX, gridY + gridCheckY])
                                    {
                                        if (particle1 == particle2) continue;
                                        if (particle1.Intersects(particle2))
                                        {
                                            normal = OwnMath.Normalize(particle1.Position-particle2.Position);
                                            depth = Vector2.Distance(particle1.Position, particle2.Position);
                                            particle1.Position += normal;
                                            particle2.Position += (normal*-1);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (var Cell in particlesGrid)
                    {
                        Cell.Clear();
                    }

                }
            }
            catch
            {

            }
        }
        private void CalcCollisionForParticle(Particle particle)
        {
            int gridX;
            int gridY;
            Vector2 normal;
            float depth;
            try
            {
                gridX = (int)particle.Position.X / gridSize;
                gridY = (int)particle.Position.Y / gridSize;
                for (int gridCheckX = -1; gridCheckX < 2; gridCheckX++)
                {
                    for (int gridCheckY = -1; gridCheckY < 2; gridCheckY++)
                    {
                        foreach (var particle1 in particlesGrid[gridX, gridY])
                        {
                            foreach (var particle2 in particlesGrid[gridX + gridCheckX, gridY + gridCheckY])
                            {

                                if (particle1.Intersects(particle2))
                                {
                                    normal = OwnMath.Normalize(particle1.Position - particle2.Position);
                                    depth = Vector2.Distance(particle1.Position, particle2.Position);
                                    particle1.Position += normal;
                                    particle2.Position += (normal * -1);
                                }
                            }
                        }
                    }
                }

                foreach (var Cell in particlesGrid)
                {
                    Cell.Clear();
                }

            }
            catch
            {

            }
        }

    }
}
