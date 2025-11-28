using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeRoom.Core;

namespace EscapeRoom.Screens
{
    public class BalloonGameScreen : GameScreen
    {
        // Texturas
        private Texture2D _texPlayer;
        private Texture2D _texBalloon;
        private Texture2D _texCloud;
        private Texture2D _texFrame;
        private Texture2D _texPlatform;
        private SpriteFont _font;

        // Tamaños de colisión
        private const int PlayerW = 60;
        private const int PlayerH = 60;
        private const int BalloonW = 30;
        private const int BalloonH = 45;
        private const int CloudSize = 100;

        // Padding para hitboxes
        private const int PlayerHitboxPadding = 10;
        private const int BalloonHitboxPadding = 5;

        // Límites del marco
        private const int FrameLeft = 40;
        private const int FrameRight = 40;
        private const int FrameTop = 60;
        private const int FrameBottom = 40;

        // Física
        private const float JumpStrength = 15f;
        private const float Gravity = 0.6f;

        // Gameplay
        private const int PointsPerBalloon = 100;
        private const int TotalBalloons = 16;
        private const float InitialTime = 40f;

        // Estado del jugador
        private Vector2 _playerPos;
        private Vector2 _playerVel;
        private bool _onGround;

        // Plataformas y globos
        private List<Rectangle> _platforms;
        private List<Balloon> _balloons;

        // Estado del juego
        private float _timeRemaining;
        private int _score;
        private bool _gameStarted;
        private bool _gameEnded;
        private bool _playerWon;

        private struct Balloon
        {
            public Rectangle Rect;
            public bool Active;
        }

        public BalloonGameScreen()
        {
            _playerPos = Vector2.Zero;
            _playerVel = Vector2.Zero;
            _onGround = false;
            _platforms = new List<Rectangle>();
            _balloons = new List<Balloon>();
            _gameStarted = false;
            _gameEnded = false;
            _playerWon = false;
            _timeRemaining = InitialTime;
            _score = 0;
        }

        protected override void LoadContent()
        {
            _texPlayer = Assets.Load<Texture2D>("BallonBoy");
            _texBalloon = Assets.Load<Texture2D>("Globo");
            _texCloud = Assets.Load<Texture2D>("Nube");
            _texFrame = Assets.Load<Texture2D>("Marco");
            _texPlatform = Assets.Load<Texture2D>("Plataforma");
            _font = Assets.Load<SpriteFont>("fuente");

            InitializePlatforms();
            ResetGame();
        }

        private void InitializePlatforms()
        {
            _platforms.Clear();

            // Plataformas inferiores
            _platforms.Add(new Rectangle(140, 550, 250, 25));
            _platforms.Add(new Rectangle(520, 500, 250, 25));
            _platforms.Add(new Rectangle(900, 550, 250, 25));

            // Plataformas superiores
            _platforms.Add(new Rectangle(140, 350, 250, 25));
            _platforms.Add(new Rectangle(520, 300, 250, 25));
            _platforms.Add(new Rectangle(900, 350, 250, 25));
        }

        private void ResetGame()
        {
            _score = 0;
            _timeRemaining = InitialTime;
            _gameEnded = false;
            _playerWon = false;
            _gameStarted = false;
            _playerVel = Vector2.Zero;
            _onGround = false;

            if (_platforms.Count > 0)
            {
                var firstPlatform = _platforms[0];
                _playerPos = new Vector2(firstPlatform.X + 20, firstPlatform.Y - PlayerH);
            }

            CreateBalloons();
        }

        private void CreateBalloons()
        {
            _balloons.Clear();

            int platformCount = _platforms.Count;
            if (platformCount == 0) return;

            int balloonsPerPlatform = TotalBalloons / platformCount;
            int remaining = TotalBalloons % platformCount;

            for (int i = 0; i < platformCount; i++)
            {
                var platform = _platforms[i];
                int balloonsOnPlatform = balloonsPerPlatform + (i < remaining ? 1 : 0);

                if (balloonsOnPlatform <= 0) continue;

                int margin = 20;
                int usableWidth = platform.Width - margin * 2;
                float spacing = balloonsOnPlatform > 1 ? (float)usableWidth / (balloonsOnPlatform - 1) : 0f;

                for (int j = 0; j < balloonsOnPlatform; j++)
                {
                    int x = balloonsOnPlatform == 1
                        ? platform.X + platform.Width / 2 - BalloonW / 2
                        : (int)(platform.X + margin + spacing * j - BalloonW / 2);

                    int y = platform.Y - BalloonH - 5;

                    _balloons.Add(new Balloon
                    {
                        Rect = new Rectangle(x, y, BalloonW, BalloonH),
                        Active = true
                    });
                }
            }
        }

        private Rectangle GetPlayerRect() =>
            new Rectangle((int)_playerPos.X, (int)_playerPos.Y, PlayerW, PlayerH);

        private Rectangle GetPlayerHitbox() =>
            new Rectangle(
                (int)_playerPos.X + PlayerHitboxPadding,
                (int)_playerPos.Y + PlayerHitboxPadding,
                PlayerW - PlayerHitboxPadding * 2,
                PlayerH - PlayerHitboxPadding * 2
            );

        private Rectangle GetBalloonHitbox(Balloon balloon) =>
            new Rectangle(
                balloon.Rect.X + BalloonHitboxPadding,
                balloon.Rect.Y + BalloonHitboxPadding,
                balloon.Rect.Width - BalloonHitboxPadding * 2,
                balloon.Rect.Height - BalloonHitboxPadding * 2
            );

        public override void Update(GameTime gameTime)
        {
            if (Input.KeyPressed(Keys.Escape))
            {
                ScreenManager.Pop();
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_gameStarted && !_gameEnded)
            {
                if (Input.KeyPressed(Keys.Enter) || Input.KeyPressed(Keys.Space))
                    _gameStarted = true;
            }
            else if (_gameEnded)
            {
                if (Input.KeyPressed(Keys.Enter) || Input.KeyPressed(Keys.Space))
                    ResetGame();
            }
            else
            {
                UpdatePlayer(dt);

                _timeRemaining -= dt;
                if (_timeRemaining <= 0)
                {
                    _timeRemaining = 0;
                    EndGame(false);
                }

                UpdateBalloons();
            }
        }

        private void UpdatePlayer(float dt)
        {
            const float speed = 250f;
            _playerVel.X = 0;

            if (Input.KeyDown(Keys.Left) || Input.KeyDown(Keys.A))
                _playerVel.X = -speed * dt;
            if (Input.KeyDown(Keys.Right) || Input.KeyDown(Keys.D))
                _playerVel.X = speed * dt;

            if (_onGround)
                _playerVel.Y = 0;

            if (_onGround && (Input.KeyDown(Keys.Space) || Input.KeyDown(Keys.Up) || Input.KeyDown(Keys.W)))
            {
                _playerVel.Y = -JumpStrength;
                _onGround = false;
            }

            if (!_onGround)
                _playerVel.Y += Gravity;

            _playerPos += _playerVel;

            // Límites del marco
            float minX = FrameLeft;
            float maxX = Game.Window.ClientBounds.Width - FrameRight - PlayerW;
            float minY = FrameTop;
            float maxY = Game.Window.ClientBounds.Height - FrameBottom - PlayerH;

            _playerPos.X = MathHelper.Clamp(_playerPos.X, minX, maxX);

            if (_playerPos.Y < minY)
            {
                _playerPos.Y = minY;
                _playerVel.Y = 0;
            }

            _onGround = false;

            if (_playerPos.Y > maxY)
            {
                _playerPos.Y = maxY;
                _playerVel.Y = 0;
                _onGround = true;
            }

            // Colisión con plataformas
            foreach (var platform in _platforms)
            {
                if (GetPlayerHitbox().Intersects(platform) && _playerVel.Y >= 0)
                {
                    _playerPos.Y = platform.Y - PlayerH;
                    _playerVel.Y = 0;
                    _onGround = true;
                }
            }
        }

        private void UpdateBalloons()
        {
            int balloonsCollected = 0;

            for (int i = 0; i < _balloons.Count; i++)
            {
                if (!_balloons[i].Active) continue;

                if (GetPlayerHitbox().Intersects(GetBalloonHitbox(_balloons[i])))
                {
                    Balloon b = _balloons[i];
                    b.Active = false;
                    _balloons[i] = b;

                    _score += PointsPerBalloon;

                    if (_score >= TotalBalloons * PointsPerBalloon)
                    {
                        EndGame(true);
                        return;
                    }
                }

                if (_balloons[i].Active)
                    balloonsCollected++;
            }
        }

        private void EndGame(bool won)
        {
            _gameEnded = true;
            _playerWon = won;
        }

        public override void Draw(GameTime gameTime)
        {
            int screenWidth = Game.Window.ClientBounds.Width;
            int screenHeight = Game.Window.ClientBounds.Height;

            SpriteBatch.Begin();

            // Pantalla de inicio
            if (!_gameStarted && !_gameEnded)
            {
                DrawStartScreen(screenWidth, screenHeight);
            }
            // Pantalla de fin
            else if (_gameEnded)
            {
                DrawEndScreen(screenWidth, screenHeight);
            }
            // Juego normal
            else
            {
                DrawGameplay(screenWidth, screenHeight);
            }

            SpriteBatch.End();
        }

        private void DrawStartScreen(int screenWidth, int screenHeight)
        {
            string title = "MINIJUEGO DE GLOBOS";
            string subtitle = "NE GUSTAN LOS GLOBOS";
            string instruction = "Pulsa ENTER o ESPACIO para jugar";

            DrawCenteredText(title, new Vector2(screenWidth / 2f, screenHeight * 0.2f), Color.Red, 1.2f);
            DrawCenteredText(subtitle, new Vector2(screenWidth / 2f, screenHeight / 2f), Color.White, 1.0f);
            DrawCenteredText(instruction, new Vector2(screenWidth / 2f, screenHeight * 0.8f), Color.Red, 1.0f);
        }

        private void DrawEndScreen(int screenWidth, int screenHeight)
        {
            string line1 = _playerWon ? "¡HAS ESCAPADO DE LA PESADILLA!" : "¡GAME OVER!";
            string line2 = _playerWon ? "LOS GLOBOS TE HAN SALVADO..." : "LA PESADILLA TE ALCANZÓ...";
            string line3 = $"PUNTAJE: {_score}/{TotalBalloons * PointsPerBalloon}";
            string line4 = "Pulsa ENTER o ESPACIO para volver";

            DrawCenteredText(line1, new Vector2(screenWidth / 2f, screenHeight * 0.25f), Color.Red, 1.2f);
            DrawCenteredText(line2, new Vector2(screenWidth / 2f, screenHeight * 0.38f),
                new Color(255, 140, 0), 1.0f);
            DrawCenteredText(line3, new Vector2(screenWidth / 2f, screenHeight * 0.5f), Color.White, 1.0f);
            DrawCenteredText(line4, new Vector2(screenWidth / 2f, screenHeight * 0.8f), Color.Red, 1.0f);
        }

        private void DrawGameplay(int screenWidth, int screenHeight)
        {
            // Marco
            SpriteBatch.Draw(_texFrame,
                new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            // Nubes
            SpriteBatch.Draw(_texCloud, new Rectangle(150, 100, CloudSize, CloudSize), Color.White);
            SpriteBatch.Draw(_texCloud, new Rectangle(screenWidth - 250, 80, CloudSize, CloudSize), Color.White);

            // Plataformas
            foreach (var platform in _platforms)
                SpriteBatch.Draw(_texPlatform, platform, Color.White);

            // Globos
            foreach (var balloon in _balloons)
                if (balloon.Active)
                    SpriteBatch.Draw(_texBalloon, balloon.Rect, Color.White);

            // Jugador
            SpriteBatch.Draw(_texPlayer, GetPlayerRect(), Color.White);

            // HUD
            SpriteBatch.DrawString(_font, "EL JUEGO DEL NIÑO",
                new Vector2(screenWidth / 2f - 100, 10), Color.Red);
            SpriteBatch.DrawString(_font, $"Puntos: {_score}",
                new Vector2(30, 30), Color.White);
            SpriteBatch.DrawString(_font, $"Tiempo: {(int)_timeRemaining}",
                new Vector2(screenWidth - 300, 50), Color.White);
        }

        private void DrawCenteredText(string text, Vector2 position, Color color, float scale)
        {
            Vector2 size = _font.MeasureString(text);
            SpriteBatch.DrawString(_font, text, position, color, 0f, size / 2f, scale,
                SpriteEffects.None, 0f);
        }
    }
}