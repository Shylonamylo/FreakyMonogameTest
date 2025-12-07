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
        int gridSize = 20;

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

            Thread CollisionsSolver = new(() => CalcCollisions(cancellationTokenSource.Token));
            CollisionsSolver.IsBackground = true;
            CollisionsSolver.Start();

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
                BrushSize = Math.Clamp(BrushSize + 1, 0, 2);
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
                particle.Velocity += OwnMath.Normalize(new Vector2(curMouseState.X, curMouseState.Y) - particle.Position)*0.01f;
                particle.Position += particle.Velocity;
                try
                {
                    particlesGrid[(int)particle.Position.X / gridSize, (int)particle.Position.Y / gridSize].Add(particle);
                }
                catch
                {

                }
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
                _spriteBatch.Draw(circle, new Rectangle((int)particle.Position.X - particle.Size / 2, (int)particle.Position.Y - particle.Size / 2, particle.Size, particle.Size), Color.Black);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CalcCollisions(CancellationToken token)
        {
            while (!token.IsCancellationRequested) {
                try
                {
                    if (particles.Count > 1)
                    {
                        for(int gridX = 1; gridX < gridWidth; gridX++)
                        {
                            for (int gridY = 1; gridY < gridHeight; gridY++)
                            {
                                if (particlesGrid[gridX, gridY].Count == 0) { continue; }
                                for (int gridCheckX = -1; gridCheckX < 2; gridCheckX++)
                                {
                                    for (int gridCheckY = -1; gridCheckY < 2; gridCheckY++)
                                    {
                                        if (particlesGrid[gridX + gridCheckX, gridY + gridCheckY].Count == 0) { continue; }
                                        foreach (var particle1 in particlesGrid[gridX, gridY])
                                        {
                                            foreach (var particle2 in particlesGrid[gridX + gridCheckX, gridY + gridCheckY])
                                            {
                                                if (particle1 != null && particle2 != null)
                                                {
                                                    ResolveCollision(particle1, particle2);
                                                }
                                                else
                                                {
                                                    break;
                                                }
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
                Thread.Sleep(1);
            }
        }

        private void ResolveCollision(Particle a, Particle b)
        {
            if (a.Intersects(b, out Vector2 normal, out float depth))
            {
                float radiusA = a.Size / 2f;
                float radiusB = b.Size / 2f;
                float totalRadius = radiusA + radiusB;

                float weightA = radiusB / totalRadius;
                float weightB = radiusA / totalRadius;

                Vector2 correction = normal * depth * 0.5f;
                a.Position -= -correction * -weightA;
                b.Position += -correction * -weightB;

                Vector2 relativeVelocity = b.Velocity - a.Velocity;
                float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

                if (velocityAlongNormal > 0) return;

                float elasticity = 0.8f;
                float impulseScalar = -(1 + elasticity) * velocityAlongNormal;
                impulseScalar /= (1 / radiusA + 1 / radiusB);

                Vector2 impulse = normal * impulseScalar;
                a.Velocity -= impulse / radiusA * 0.5f;
                b.Velocity += impulse / radiusB * 0.5f;
                CheckBorder(a);

                CheckBorder(b);
            }

        }

        private void CheckBorder(Particle particle)
        {
            if (particle.Position.X - particle.Size*2 < 0)
            {
                particle.Position = new Vector2(particle.Size*2, particle.Position.Y);
                particle.Velocity = new Vector2(-particle.Velocity.X, particle.Velocity.Y);
            }
            if (particle.Position.X + particle.Size * 2 > Window.ClientBounds.Width)
            {
                particle.Position = new Vector2(Window.ClientBounds.Width - particle.Size * 2, particle.Position.Y);
                particle.Velocity = new Vector2(-particle.Velocity.X, particle.Velocity.Y);
            }
            if (particle.Position.Y - particle.Size * 2 < 0)
            {
                particle.Position = new Vector2(particle.Position.X, particle.Size * 2);
                particle.Velocity = new Vector2(particle.Velocity.X, -particle.Velocity.Y);
            }
            if (particle.Position.Y + particle.Size * 2 > Window.ClientBounds.Height)
            {
                particle.Position = new Vector2(particle.Position.X, Window.ClientBounds.Height - particle.Size * 2);
                particle.Velocity = new Vector2(particle.Velocity.X, -particle.Velocity.Y);
            }
        }
    }
}
