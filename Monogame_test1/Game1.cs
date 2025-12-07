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
        
        private bool CheckCollisions = false;

        Texture2D circle {get; set;}

        Random rand = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Thread CollisionsSolver = new(Collisions);
            CollisionsSolver.Start();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            circle = Content.Load<Texture2D>("circle_texture");
            // TODO: use this.Content to load your game content here
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

            CheckCollisions = true;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            
            _spriteBatch.Begin();

            _spriteBatch.Draw(circle, new Rectangle(curMouseState.X- BrushSize * 10 / 2, curMouseState.Y- BrushSize * 10 / 2, BrushSize*10, BrushSize*10), Color.Black);

            foreach (var particle in particles)
            {
                particle.Velocity = OwnMath.Normalize(new Vector2(curMouseState.X, curMouseState.Y) - particle.Position);
                _spriteBatch.Draw(circle, new Rectangle((int)particle.Position.X - particle.Size / 2, (int)particle.Position.Y - particle.Size / 2, particle.Size, particle.Size), Color.Black);
            }
            
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        private void Collisions()
        {
            while (true)
            {
                try
                {
                    foreach (var particle1 in particles)
                    {
                        foreach (var particle2 in particles)
                        {
                            if (particle1.Intersects(particle2))
                            {
                                particle1.Position -= OwnMath.Normalize(particle2.Position - particle1.Position)*2;
                            }
                        }
                    }
                }
                catch
                {

                }
                
            }
        }
    }
}
