using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeRoom.Core;
using EscapeRoom.Screens;

namespace EscapeRoom
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Assets.Initialize(Content, GraphicsDevice);
            ScreenManager.Initialize(this, _spriteBatch);

            // Arrancamos en la Pantalla de Inicio
            ScreenManager.Push(new MenuScreen());
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            if (Input.RequestExit) Exit();
            ScreenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            ScreenManager.Draw(gameTime);
            base.Draw(gameTime);
        }
    }
}