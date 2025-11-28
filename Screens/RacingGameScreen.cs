using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using EscapeRoom.Core;

namespace EscapeRoom.Screens
{
    public enum RacingGameState
    {
        Menu,
        Countdown,
        Playing,
        Jumpscare,
        GameOver,
        Victory
    }

    public class RacingGameScreen : GameScreen
    {
        private const int TARGET_SCORE = 700;
        private const int LANE_COUNT = 4;
        private const float INITIAL_TIME = 30f;
        private const float BASE_SPEED = 10f;
        private const int MAX_ENEMIES_PER_SPAWN = 2;
        private const float VICTORY_STOP_DELAY = 1f;
        private const float JUMPSCARE_DURATION = 0.5f;
        private const float INVULNERABILITY_TIME = 2f;
        private const float BLINK_RATE = 0.1f;
        private const float DAMAGE_FLASH_DURATION = 0.5f;

        // Recursos
        private Texture2D _playerTex;
        private Texture2D _enemyTex;
        private Texture2D _jumpscareTex;
        private Texture2D _finishLineTex;
        private Texture2D _pixelTex;
        private SpriteFont _font;
        private Song _bgMusic;
        private SoundEffect _jumpscareSound;

        // Estado
        private RacingGameState _gameState = RacingGameState.Menu;
        private int _laneHeight;
        private Countdown _countdown;
        private Random _random;

        // Jugador
        private RacingPlayer _player;
        private List<RacingEnemy> _enemies;

        // Gameplay
        private float _gameTimer;
        private int _score;
        private int _lives = 3;
        private float _roadSpeed;
        private float _roadSegmentOffset;
        private int _speedLevel;
        private float _enemySpawnTimer;
        private float _enemySpawnInterval = 0.6f;

        // Temporizadores y efectos
        private float _damageFlashTimer;
        private float _jumpscareTimer;
        private float _victoryStopTimer;
        private bool _jumpscareSoundPlayed;
        private bool _blinkVisible;
        private float _blinkTimer;

        public RacingGameScreen()
        {
            _laneHeight = ScreenManager.Game.Window.ClientBounds.Height / LANE_COUNT;
            _enemies = new List<RacingEnemy>();
            _random = new Random();
            _countdown = new Countdown(5);
        }

        protected override void LoadContent()
        {
            try
            {
                _playerTex = Assets.Load<Texture2D>("auto_evan");
                _enemyTex = Assets.Load<Texture2D>("auto_enemigo");
                _jumpscareTex = Assets.Load<Texture2D>("jumpscare");
                _font = Assets.Load<SpriteFont>("fuente");

                try
                {
                    _bgMusic = Assets.Load<Song>("minijuegocar_sonido");
                    _jumpscareSound = Assets.Load<SoundEffect>("jumpscare_sonido");
                }
                catch { /* Audio opcional */ }

                _finishLineTex = CreateFinishLineTexture();
                _pixelTex = CreatePixelTexture();

                InitializeGame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando recursos: {ex.Message}");
                _pixelTex = CreatePixelTexture();
                InitializeGame();
            }
        }

        private Texture2D CreatePixelTexture()
        {
            var tex = new Texture2D(Game.GraphicsDevice, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }

        private Texture2D CreateFinishLineTexture()
        {
            int width = 50;
            int height = Game.Window.ClientBounds.Height;
            var tex = new Texture2D(Game.GraphicsDevice, width, height);
            var data = new Color[width * height];
            int cellSize = 25;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isWhite = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                    data[y * width + x] = isWhite ? Color.White : Color.Black;
                }
            }
            tex.SetData(data);
            return tex;
        }

        private void InitializeGame()
        {
            _score = 0;
            _lives = 3;
            _gameTimer = INITIAL_TIME;
            _roadSpeed = BASE_SPEED;
            _speedLevel = 0;
            _enemySpawnTimer = 0f;
            _roadSegmentOffset = 0f;
            _jumpscareSoundPlayed = false;
            _victoryStopTimer = 0f;
            _gameState = RacingGameState.Menu;

            _player = new RacingPlayer(_playerTex, 1, _laneHeight, Game.Window.ClientBounds.Width);
            _enemies.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.KeyPressed(Keys.Escape))
            {
                ScreenManager.Pop();
                return;
            }

            switch (_gameState)
            {
                case RacingGameState.Menu:
                    if (Input.KeyPressed(Keys.Enter) || Input.KeyPressed(Keys.Space))
                    {
                        InitializeGame();
                        _countdown.Reset();
                        _gameState = RacingGameState.Countdown;
                    }
                    break;

                case RacingGameState.Countdown:
                    _countdown.Update(gameTime);
                    if (_countdown.IsFinished)
                    {
                        _gameState = RacingGameState.Playing;
                        if (_bgMusic != null) MediaPlayer.Play(_bgMusic);
                        _enemySpawnTimer = _enemySpawnInterval;
                    }
                    break;

                case RacingGameState.Playing:
                    UpdateGameplay(gameTime);
                    break;

                case RacingGameState.Jumpscare:
                    UpdateJumpscare(gameTime);
                    break;

                case RacingGameState.GameOver:
                case RacingGameState.Victory:
                    if (Input.KeyPressed(Keys.Enter) || Input.KeyPressed(Keys.Space))
                        _gameState = RacingGameState.Menu;
                    break;
            }
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int screenWidth = Game.Window.ClientBounds.Width;

            // Victoria
            if (_score >= TARGET_SCORE)
            {
                _roadSpeed = 0f;
                _enemySpawnTimer = 9999f;
                _enemies.Clear();
                _victoryStopTimer += dt;

                if (_victoryStopTimer >= VICTORY_STOP_DELAY)
                {
                    _gameState = RacingGameState.Victory;
                    MediaPlayer.Stop();
                }
                return;
            }

            // Carretera
            float moveDistance = _roadSpeed * dt * 60;
            _roadSegmentOffset += moveDistance;
            if (_roadSegmentOffset >= 60) _roadSegmentOffset -= 60;

            // Tiempo y dificultad
            _gameTimer -= dt;
            int newSpeedLevel = (int)Math.Floor((_gameTimer <= 0 ? INITIAL_TIME : INITIAL_TIME - _gameTimer) / 7.5f);

            if (newSpeedLevel > _speedLevel)
            {
                _speedLevel = newSpeedLevel;
                _roadSpeed = BASE_SPEED + (2.5f * _speedLevel);
                _enemySpawnInterval = Math.Max(0.3f, _enemySpawnInterval - 0.05f);
            }

            if (_gameTimer <= 0)
            {
                _gameTimer = 0;
                _gameState = RacingGameState.GameOver;
                MediaPlayer.Stop();
                return;
            }

            // Jugador
            _player.Update(gameTime);

            if (_damageFlashTimer > 0)
                _damageFlashTimer -= dt;

            // Enemigos
            _enemySpawnTimer -= dt;
            if (_enemySpawnTimer <= 0)
            {
                AddNewEnemy(screenWidth);
                _enemySpawnTimer = _enemySpawnInterval;
            }

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Update(gameTime, _roadSpeed);

                if (_enemies[i].Position.X < -_enemies[i].Texture.Width)
                {
                    _enemies.RemoveAt(i);
                    if (_score < TARGET_SCORE)
                        _score += 10;
                }
            }

            // Colisiones
            if (!_player.IsInvulnerable)
            {
                Rectangle playerRect = _player.CollisionRect;

                for (int i = _enemies.Count - 1; i >= 0; i--)
                {
                    if (playerRect.Intersects(_enemies[i].CollisionRect))
                    {
                        _lives--;
                        _damageFlashTimer = DAMAGE_FLASH_DURATION;
                        _enemies.RemoveAt(i);

                        if (_lives <= 0)
                        {
                            _gameState = RacingGameState.Jumpscare;
                            MediaPlayer.Stop();
                            _jumpscareTimer = 0f;
                            break;
                        }

                        _player.TakeDamage();
                    }
                }
            }
        }

        private void AddNewEnemy(int screenWidth)
        {
            const float MIN_SPAWN_GAP = 600f;
            var spawnableLanes = new List<int>();

            for (int lane = 0; lane < LANE_COUNT; lane++)
            {
                var nearestEnemy = _enemies
                    .Where(e => e.Lane == lane)
                    .OrderByDescending(e => e.Position.X)
                    .FirstOrDefault();

                if (nearestEnemy == null || nearestEnemy.Position.X < screenWidth - MIN_SPAWN_GAP)
                    spawnableLanes.Add(lane);
            }

            int enemiesToSpawn = _random.Next(1, MAX_ENEMIES_PER_SPAWN + 1);
            int count = 0;
            spawnableLanes = spawnableLanes.OrderBy(x => _random.Next()).ToList();

            foreach (int lane in spawnableLanes)
            {
                if (count >= enemiesToSpawn) break;

                if (_random.NextDouble() < 0.6)
                {
                    float enemySpeed = _roadSpeed * (_random.NextDouble() < 0.1 ? 1.5f : 1f);
                    float offsetX = _random.Next(20, 100);

                    _enemies.Add(new RacingEnemy(_enemyTex, lane, _laneHeight, enemySpeed, offsetX));
                    count++;
                }
            }
        }

        private void UpdateJumpscare(GameTime gameTime)
        {
            if (!_jumpscareSoundPlayed && _jumpscareSound != null)
            {
                _jumpscareSound.Play();
                _jumpscareSoundPlayed = true;
            }

            _jumpscareTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_jumpscareTimer >= JUMPSCARE_DURATION)
            {
                _gameState = RacingGameState.GameOver;
                _jumpscareTimer = 0f;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            int sw = Game.Window.ClientBounds.Width;
            int sh = Game.Window.ClientBounds.Height;

            SpriteBatch.Begin();

            if (_gameState != RacingGameState.Jumpscare)
            {
                DrawRoad(sw, sh);

                if (_gameState == RacingGameState.Playing || _gameState == RacingGameState.Victory)
                {
                    DrawFinishLine(sw);
                    foreach (var enemy in _enemies)
                        enemy.Draw(SpriteBatch);
                    _player.Draw(SpriteBatch);
                    DrawHUD(sw, sh);
                    DrawDamageFlash(sw, sh);
                }
            }

            switch (_gameState)
            {
                case RacingGameState.Menu:
                    DrawMenuScreen(sw, sh);
                    break;
                case RacingGameState.Countdown:
                    DrawCountdownScreen(sw, sh);
                    break;
                case RacingGameState.Jumpscare:
                    DrawJumpscareScreen(sw, sh);
                    break;
                case RacingGameState.GameOver:
                    DrawEndScreen(sw, sh, "¡GAME OVER!", Color.Red, "LA PESADILLA TE ALCANZÓ...");
                    break;
                case RacingGameState.Victory:
                    DrawEndScreen(sw, sh, "¡VICTORIA!", Color.LimeGreen, "¡OBTUVISTE LA PIEZA!");
                    break;
            }

            SpriteBatch.End();
        }

        private void DrawRoad(int screenWidth, int screenHeight)
        {
            Color lineColor = Color.Red;
            int dashLength = 30, spaceLength = 30, lineThickness = 5;
            int dashTotal = dashLength + spaceLength;

            for (int i = 0; i < LANE_COUNT - 1; i++)
            {
                float yPos = _laneHeight * (i + 1);
                for (int xOffset = -dashTotal; xOffset < screenWidth + dashTotal; xOffset += dashTotal)
                {
                    float x = xOffset - _roadSegmentOffset;
                    SpriteBatch.Draw(_pixelTex,
                        new Rectangle((int)x, (int)yPos - lineThickness / 2, dashLength, lineThickness),
                        lineColor);
                }
            }

            SpriteBatch.Draw(_pixelTex, new Rectangle(0, 0, screenWidth, 10), Color.DarkRed);
            SpriteBatch.Draw(_pixelTex, new Rectangle(0, screenHeight - 10, screenWidth, 10), Color.DarkRed);
        }

        private void DrawFinishLine(int screenWidth)
        {
            if (_score < 650) return;

            float targetX = screenWidth - _finishLineTex.Width;

            if (_roadSpeed == 0)
            {
                SpriteBatch.Draw(_finishLineTex, new Vector2(targetX, 0), Color.White);
                return;
            }

            float maxScore = 650f;
            float scoreRange = TARGET_SCORE - maxScore;
            float progress = Math.Min(1f, (_score - maxScore) / scoreRange);
            float initialX = screenWidth * 1.5f;
            float currentX = initialX - (initialX - targetX) * progress;

            SpriteBatch.Draw(_finishLineTex, new Vector2(currentX, 0), Color.White);
        }

        private void DrawHUD(int screenWidth, int screenHeight)
        {
            if (_font == null) return;

            string livesText = $"VIDAS: {_lives}";
            string scoreText = $"PUNTAJE: {_score}/{TARGET_SCORE}";
            string timeText = $"TIEMPO: {Math.Max(0, (int)Math.Ceiling(_gameTimer))}s";
            string speedText = _speedLevel > 0 ? $"VELOCIDAD x{_speedLevel + 1}" : "";

            int pad = 30;
            SpriteBatch.DrawString(_font, livesText, new Vector2(pad, pad), Color.Red);
            SpriteBatch.DrawString(_font, scoreText, new Vector2(screenWidth - 350, pad), Color.White);
            SpriteBatch.DrawString(_font, speedText, new Vector2(screenWidth / 2 - 100, pad), Color.Orange);
            SpriteBatch.DrawString(_font, timeText, new Vector2(screenWidth / 2 - 100, screenHeight - 60), Color.Yellow);
        }

        private void DrawDamageFlash(int screenWidth, int screenHeight)
        {
            if (_damageFlashTimer <= 0 || _font == null) return;

            float ratio = _damageFlashTimer / DAMAGE_FLASH_DURATION;
            float scale = 1.0f + (1.0f - ratio) * 1.5f;
            Color flashColor = Color.Red * ratio * 2f;

            string text = "¡DAÑO CRÍTICO!";
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 pos = new Vector2(screenWidth / 2 - size.X / 2, screenHeight / 2 - size.Y / 2);

            SpriteBatch.DrawString(_font, text, pos, flashColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawMenuScreen(int screenWidth, int screenHeight)
        {
            if (_font == null) return;

            SpriteBatch.Draw(_pixelTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.9f);

            DrawCenteredText("CARRERAS FURIOSAS", screenWidth / 2, screenHeight * 0.2f, 1.2f, Color.Red);
            DrawCenteredText("EL MIEDO ES EL CAMINO", screenWidth / 2, screenHeight * 0.35f, 0.8f, Color.White);
            DrawCenteredText($"Meta: {TARGET_SCORE} puntos en {(int)INITIAL_TIME}s", screenWidth / 2, screenHeight * 0.5f, 0.6f, Color.Yellow);
            DrawCenteredText("Presiona ENTER para comenzar", screenWidth / 2, screenHeight * 0.75f, 0.6f, Color.LimeGreen);
            DrawCenteredText("Usa ARRIBA/ABAJO o W/S para cambiar carril", screenWidth / 2, screenHeight * 0.85f, 0.6f, Color.LimeGreen);
        }

        private void DrawCountdownScreen(int screenWidth, int screenHeight)
        {
            if (_font == null) return;

            SpriteBatch.Draw(_pixelTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.7f);

            string text = _countdown.CurrentValue == 0 ? "¡SALIDA!" : _countdown.CurrentValue.ToString();
            DrawCenteredText(text, screenWidth / 2, screenHeight / 2 - 50, 4f, Color.Yellow);
        }

        private void DrawJumpscareScreen(int screenWidth, int screenHeight)
        {
            if (_jumpscareTex != null)
                SpriteBatch.Draw(_jumpscareTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
        }

        private void DrawEndScreen(int screenWidth, int screenHeight, string title, Color titleColor, string subtitle)
        {
            if (_font == null) return;

            SpriteBatch.Draw(_pixelTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.Black);

            DrawCenteredText(title, screenWidth / 2, screenHeight * 0.25f, 1.3f, titleColor);
            DrawCenteredText(subtitle, screenWidth / 2, screenHeight * 0.4f, 1f, Color.OrangeRed);
            DrawCenteredText($"PUNTAJE: {_score}/{TARGET_SCORE}", screenWidth / 2, screenHeight * 0.55f, 1f, Color.White);
            DrawCenteredText("Pulsa ENTER para volver", screenWidth / 2, screenHeight * 0.8f, 0.8f, Color.Red);
        }

        private void DrawCenteredText(string text, float centerX, float centerY, float scale, Color color)
        {
            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 pos = new Vector2(centerX - size.X / 2, centerY - size.Y / 2);
            SpriteBatch.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    // --- CLASES DE SOPORTE ---

    public class RacingPlayer
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public Rectangle CollisionRect =>
            new Rectangle((int)(Position.X + Texture.Width * 0.1f), (int)(Position.Y + Texture.Height * 0.1f),
                (int)(Texture.Width * 0.8f), (int)(Texture.Height * 0.8f));

        public bool IsInvulnerable => _invulnerabilityTimer > 0;

        private int _currentLane;
        private int _laneHeight;
        private int _screenWidth;
        private float _invulnerabilityTimer;
        private bool _isVisible = true;

        private const int LANE_COUNT = 4;
        private const int HORIZONTAL_POS = 100;

        public RacingPlayer(Texture2D texture, int startLane, int laneHeight, int screenWidth)
        {
            Texture = texture;
            _laneHeight = laneHeight;
            _currentLane = startLane;
            _screenWidth = screenWidth;
            Position = CalculateLanePosition(startLane);
        }

        private Vector2 CalculateLanePosition(int lane)
        {
            float yCenter = _laneHeight * lane + (_laneHeight / 2);
            return new Vector2(HORIZONTAL_POS, yCenter - (Texture.Height / 2));
        }

        public void TakeDamage()
        {
            _invulnerabilityTimer = 2f; // corrected to assign directly to _invulnerabilityTimer
            _isVisible = false;
        }

        public void Update(GameTime gameTime)
        {
            if (Input.KeyPressed(Keys.Up) || Input.KeyPressed(Keys.W))
                _currentLane = Math.Max(0, _currentLane - 1);

            if (Input.KeyPressed(Keys.Down) || Input.KeyPressed(Keys.S))
                _currentLane = Math.Min(LANE_COUNT - 1, _currentLane + 1);

            Position = CalculateLanePosition(_currentLane);

            if (IsInvulnerable)
            {
                _invulnerabilityTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _isVisible = !_isVisible;
                if (_invulnerabilityTimer <= 0)
                    _isVisible = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isVisible)
                spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

    public class RacingEnemy
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public int Lane { get; private set; }
        public Rectangle CollisionRect =>
            new Rectangle((int)(Position.X + Texture.Width * 0.1f), (int)(Position.Y + Texture.Height * 0.1f),
                (int)(Texture.Width * 0.8f), (int)(Texture.Height * 0.8f));

        private float _enemySpeed;
        private int _laneHeight;

        public RacingEnemy(Texture2D texture, int lane, int laneHeight, float enemySpeed, float startXOffset = 0)
        {
            Texture = texture;
            Lane = lane;
            _laneHeight = laneHeight;
            _enemySpeed = enemySpeed;
            Position = new Vector2(1280 + startXOffset, CalculateYPos(lane));
        }

        private float CalculateYPos(int lane)
        {
            float yCenter = _laneHeight * lane + (_laneHeight / 2);
            return yCenter - (Texture.Height / 2);
        }

        public void Update(GameTime gameTime, float roadSpeed)
        {
            float speed = _enemySpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            Position = new Vector2(Position.X - speed, Position.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

    public class Countdown
    {
        private float _currentTime;
        private float _initialTime;

        public int CurrentValue => (int)Math.Ceiling(_currentTime);
        public bool IsFinished => _currentTime <= 0;

        public Countdown(int initialSeconds)
        {
            _initialTime = initialSeconds;
            _currentTime = _initialTime;
        }

        public void Reset() => _currentTime = _initialTime;

        public void Update(GameTime gameTime)
        {
            if (_currentTime > 0)
                _currentTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}