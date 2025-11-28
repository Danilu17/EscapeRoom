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
    public class MazeGameScreen : GameScreen
    {
        private const int TILE_SIZE = 40;
        private const int MAZE_WIDTH = 30;
        private const int MAZE_HEIGHT = 18;
        private const int KEYS_NEEDED = 3;
        private const float INITIAL_DELAY = 3f;
        private const float JUMPSCARE_DURATION = 0.5f;
        private const float CHAR_SCALE = 0.7f;

        // Recursos
        private Texture2D _playerTex;
        private Texture2D _enemyTex;
        private Texture2D _keyTex;
        private Texture2D _jumpscareTex;
        private Texture2D _pixelTex;

        // Entities
        private MazePlayer _player;
        private MazeEnemy _enemy;
        private MazeKey[] _keys;
        private MazeLayout _maze;

        // Estado
        private MazeGameState _gameState = MazeGameState.Menu;
        private int _keysCollected;
        private float _jumpscareTimer;
        private float _startDelayTimer;

        private enum MazeGameState { Menu, Starting, Playing, Jumpscare, Defeat, Victory }

        protected override void LoadContent()
        {
            _playerTex = Assets.Player;
            _enemyTex = Assets.Enemigo;
            _keyTex = Assets.Llave;
            _jumpscareTex = Assets.Jumpscare;

            _pixelTex = CreatePixelTexture();
            _maze = new MazeLayout();

            _player = new MazePlayer(_playerTex, Vector2.Zero, 250f);
            _enemy = new MazeEnemy(_enemyTex, Vector2.Zero, 180f);
            _enemy.SetMaze(_maze);

            _keys = new MazeKey[KEYS_NEEDED];
            _keys[0] = new MazeKey(_keyTex, new Vector2(5 * TILE_SIZE, 5 * TILE_SIZE));
            _keys[1] = new MazeKey(_keyTex, new Vector2(15 * TILE_SIZE, 12 * TILE_SIZE));
            _keys[2] = new MazeKey(_keyTex, new Vector2(27 * TILE_SIZE, 15 * TILE_SIZE));

            ResetGame();
        }

        private Texture2D CreatePixelTexture()
        {
            var tex = new Texture2D(Game.GraphicsDevice, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }

        private void ResetGame()
        {
            _player.Position = GetTilePosition(1, 1);
            _enemy.Position = GetTilePosition(2, 1);
            _enemy.ResetRoute();

            _keysCollected = 0;
            foreach (var key in _keys)
                key.Activate();

            _gameState = MazeGameState.Menu;
            _startDelayTimer = 0f;
        }

        private Vector2 GetTilePosition(int x, int y) => new Vector2(x * TILE_SIZE, y * TILE_SIZE);

        public override void Update(GameTime gameTime)
        {
            if (Input.KeyPressed(Keys.Escape))
            {
                ScreenManager.Pop();
                return;
            }

            switch (_gameState)
            {
                case MazeGameState.Menu:
                    if (Input.KeyPressed(Keys.Space) || Input.KeyPressed(Keys.Enter))
                    {
                        ResetGame();
                        _gameState = MazeGameState.Starting;
                    }
                    break;

                case MazeGameState.Starting:
                    _startDelayTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    UpdatePlayer(gameTime);
                    if (_startDelayTimer >= INITIAL_DELAY)
                        _gameState = MazeGameState.Playing;
                    break;

                case MazeGameState.Playing:
                    UpdatePlayer(gameTime);
                    UpdateEnemy(gameTime);

                    // Colisión: Llave
                    foreach (var key in _keys)
                    {
                        if (key.IsActive && _player.Bounds.Intersects(key.Bounds))
                        {
                            key.Collect();
                            _keysCollected++;
                        }
                    }

                    // Colisión: Enemigo (Jumpscare)
                    if (_player.Bounds.Intersects(_enemy.Bounds))
                    {
                        _gameState = MazeGameState.Jumpscare;
                        _jumpscareTimer = 0f;
                        break;
                    }

                    // Colisión: Salida
                    if (IsAtExit(_player.Bounds) && _keysCollected >= KEYS_NEEDED)
                        _gameState = MazeGameState.Victory;
                    break;

                case MazeGameState.Jumpscare:
                    _jumpscareTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (_jumpscareTimer >= JUMPSCARE_DURATION)
                        _gameState = MazeGameState.Defeat;
                    break;

                case MazeGameState.Defeat:
                case MazeGameState.Victory:
                    if (Input.KeyPressed(Keys.Enter) || Input.KeyPressed(Keys.Space))
                        _gameState = MazeGameState.Menu;
                    break;
            }
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 movement = Vector2.Zero;

            if (Input.KeyDown(Keys.W)) movement.Y -= 1;
            if (Input.KeyDown(Keys.S)) movement.Y += 1;
            if (Input.KeyDown(Keys.A)) movement.X -= 1;
            if (Input.KeyDown(Keys.D)) movement.X += 1;

            if (movement != Vector2.Zero)
            {
                _player.UpdateDirection(movement);
                movement.Normalize();
                Vector2 moveOffset = movement * _player.Speed * dt;

                // Movimiento por eje
                Vector2 newPosX = _player.Position + new Vector2(moveOffset.X, 0);
                if (!CheckCollision(CreateBounds(newPosX, _player.Bounds.Width, _player.Bounds.Height)))
                    _player.Position = newPosX;

                Vector2 newPosY = _player.Position + new Vector2(0, moveOffset.Y);
                if (!CheckCollision(CreateBounds(newPosY, _player.Bounds.Width, _player.Bounds.Height)))
                    _player.Position = newPosY;
            }
        }

        private void UpdateEnemy(GameTime gameTime)
        {
            if (_gameState == MazeGameState.Playing)
            {
                _enemy.Chase(_player.Position, gameTime);
                _enemy.Position += _enemy.LastMovement;
            }
        }

        private bool CheckCollision(Rectangle bounds)
        {
            int leftTile = bounds.Left / TILE_SIZE;
            int rightTile = (bounds.Right - 1) / TILE_SIZE;
            int topTile = bounds.Top / TILE_SIZE;
            int bottomTile = (bounds.Bottom - 1) / TILE_SIZE;

            if (leftTile < 0 || rightTile >= MAZE_WIDTH || topTile < 0 || bottomTile >= MAZE_HEIGHT)
                return true;

            return _maze.IsWall(leftTile, topTile) || _maze.IsWall(rightTile, topTile) ||
                   _maze.IsWall(leftTile, bottomTile) || _maze.IsWall(rightTile, bottomTile);
        }

        private Rectangle CreateBounds(Vector2 pos, int width, int height)
        {
            int size = (int)(TILE_SIZE * CHAR_SCALE);
            int offset = (TILE_SIZE - size) / 2;
            return new Rectangle((int)pos.X + offset, (int)pos.Y + offset, size, size);
        }

        private bool IsAtExit(Rectangle bounds)
        {
            int tileX = bounds.Center.X / TILE_SIZE;
            int tileY = bounds.Center.Y / TILE_SIZE;
            return tileX >= 0 && tileX < MAZE_WIDTH && tileY >= 0 && tileY < MAZE_HEIGHT &&
                   _maze.grid[tileY, tileX] == 2;
        }

        public override void Draw(GameTime gameTime)
        {
            int sw = Game.Window.ClientBounds.Width;
            int sh = Game.Window.ClientBounds.Height;

            SpriteBatch.Begin();

            if (_gameState == MazeGameState.Playing || _gameState == MazeGameState.Starting)
            {
                DrawMaze();
                foreach (var key in _keys)
                    key.Draw(SpriteBatch, TILE_SIZE);
                _player.Draw(SpriteBatch, TILE_SIZE);
                _enemy.Draw(SpriteBatch, TILE_SIZE);
                DrawHUD(sw);
                DrawStartCountdown(sw, sh);
            }

            if (_gameState == MazeGameState.Jumpscare && _jumpscareTex != null)
                SpriteBatch.Draw(_jumpscareTex, Game.Window.ClientBounds, Color.White);

            DrawMenuOverlay(sw, sh);

            SpriteBatch.End();
        }

        private void DrawMaze()
        {
            for (int y = 0; y < MAZE_HEIGHT; y++)
            {
                for (int x = 0; x < MAZE_WIDTH; x++)
                {
                    Color color = _maze.grid[y, x] == 1 ? Color.DarkRed * 0.9f :
                                  _maze.grid[y, x] == 2 ? Color.GreenYellow : Color.DarkGray * 0.1f;

                    var rect = new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    SpriteBatch.Draw(_pixelTex, rect, color);
                }
            }
        }

        private void DrawHUD(int screenWidth)
        {
            int barWidth = 150;
            var barRect = new Rectangle(10, 10, barWidth, 30);
            SpriteBatch.Draw(_pixelTex, barRect, Color.Black * 0.7f);

            // Mostrar llaves recogidas
            for (int i = 0; i < KEYS_NEEDED; i++)
            {
                var slotRect = new Rectangle(15 + i * 40, 15, 30, 20);
                SpriteBatch.Draw(_pixelTex, slotRect, i < _keysCollected ? Color.Gold : Color.White);
            }

            // Aviso en salida
            if (IsAtExit(_player.Bounds) && _keysCollected < KEYS_NEEDED)
            {
                var warningRect = new Rectangle(screenWidth / 2 - 150, screenWidth / 2 + 100, 300, 50);
                SpriteBatch.Draw(_pixelTex, warningRect, Color.Red * 0.7f);
            }
        }

        private void DrawStartCountdown(int sw, int sh)
        {
            if (_gameState != MazeGameState.Starting) return;

            float remaining = INITIAL_DELAY - _startDelayTimer;
            if (remaining < 0) remaining = 0;

            int barWidth = (int)(100 * (remaining / INITIAL_DELAY));
            if (barWidth < 1) barWidth = 1;

            var bgRect = new Rectangle(sw / 2 - 50, sh / 2 - 20, 100, 40);
            SpriteBatch.Draw(_pixelTex, bgRect, Color.Gray * 0.5f);
            SpriteBatch.Draw(_pixelTex, new Rectangle(bgRect.X, bgRect.Y, barWidth, bgRect.Height), Color.Yellow);
        }

        private void DrawMenuOverlay(int sw, int sh)
        {
            Color bgColor = Color.Transparent;

            switch (_gameState)
            {
                case MazeGameState.Menu:
                    bgColor = Color.Black;
                    break;
                case MazeGameState.Victory:
                    bgColor = Color.DarkGreen * 0.8f;
                    break;
                case MazeGameState.Defeat:
                    bgColor = Color.DarkRed * 0.8f;
                    break;
                default:
                    return;
            }

            SpriteBatch.Draw(_pixelTex, Game.Window.ClientBounds, bgColor);

            // Simulación de texto con rectángulos
            int centerX = sw / 2;
            int centerY = sh / 2;

            var titleRect = new Rectangle(centerX - 250, centerY - 100, 500, 50);
            SpriteBatch.Draw(_pixelTex, titleRect, Color.White * 0.9f);

            var msgRect = new Rectangle(centerX - 250, centerY + 20, 500, 50);
            SpriteBatch.Draw(_pixelTex, msgRect, Color.Gray * 0.7f);
        }
    }

    // ================= ENTITY CLASSES =================

    public abstract class MazeCharacter
    {
        public Texture2D Texture { get; protected set; }
        public Vector2 Position { get; set; }
        public float Speed { get; protected set; }

        protected const float SCALE = 0.7f;

        public Rectangle Bounds
        {
            get
            {
                int size = (int)(40 * SCALE);
                int offset = (40 - size) / 2;
                return new Rectangle((int)Position.X + offset, (int)Position.Y + offset, size, size);
            }
        }

        public MazeCharacter(Texture2D texture, Vector2 position, float speed)
        {
            Texture = texture;
            Position = position;
            Speed = speed;
        }

        public virtual void Draw(SpriteBatch sb, int tileSize)
        {
            int size = (int)(tileSize * SCALE);
            int offset = (tileSize - size) / 2;
            var destRect = new Rectangle((int)Position.X + offset, (int)Position.Y + offset, size, size);
            sb.Draw(Texture, destRect, Color.White);
        }
    }

    public class MazePlayer : MazeCharacter
    {
        public enum Direction { Front, Back, Right, Left }
        public Direction CurrentDirection { get; set; } = Direction.Front;

        public MazePlayer(Texture2D tex, Vector2 pos, float speed) : base(tex, pos, speed) { }

        public void UpdateDirection(Vector2 movement)
        {
            if (movement.Y < 0) CurrentDirection = Direction.Back;
            else if (movement.Y > 0) CurrentDirection = Direction.Front;
            else if (movement.X < 0) CurrentDirection = Direction.Left;
            else if (movement.X > 0) CurrentDirection = Direction.Right;
        }
    }

    public class MazeEnemy : MazeCharacter
    {
        private MazeLayout _maze;
        private List<Vector2> _currentRoute;
        public Vector2 LastMovement { get; private set; }
        private float _recalcTime;
        private const float RECALC_INTERVAL = 0.35f;

        public MazeEnemy(Texture2D tex, Vector2 pos, float speed) : base(tex, pos, speed) { }

        public void SetMaze(MazeLayout maze) => _maze = maze;

        public void ResetRoute() => _currentRoute = null;

        public void Chase(Vector2 target, GameTime gameTime)
        {
            if (_maze == null)
            {
                LastMovement = Vector2.Zero;
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _recalcTime += dt;

            int startX = (int)(Position.X / 40);
            int startY = (int)(Position.Y / 40);
            int targetX = (int)(target.X / 40);
            int targetY = (int)(target.Y / 40);

            if (_currentRoute == null || _currentRoute.Count == 0 || _recalcTime >= RECALC_INTERVAL)
            {
                _recalcTime = 0f;
                if (startX != targetX || startY != targetY)
                    _currentRoute = _maze.FindPathAStar(startX, startY, targetX, targetY);
                else
                    _currentRoute = null;
            }

            if (_currentRoute != null && _currentRoute.Count > 0)
            {
                Vector2 nextStep = _currentRoute[0];
                Vector2 direction = nextStep - Position;
                float distSq = direction.LengthSquared();
                float moveDist = Speed * dt;

                if (distSq < moveDist * moveDist)
                {
                    Position = nextStep;
                    _currentRoute.RemoveAt(0);

                    if (_currentRoute.Count == 0)
                    {
                        LastMovement = Vector2.Zero;
                        return;
                    }

                    nextStep = _currentRoute[0];
                    direction = nextStep - Position;
                }

                if (direction != Vector2.Zero)
                    direction.Normalize();

                LastMovement = direction * Speed * dt;
            }
            else
            {
                LastMovement = Vector2.Zero;
            }
        }
    }

    public class MazeKey
    {
        private Texture2D _texture;
        public Vector2 Position { get; private set; }
        public bool IsActive { get; private set; } = true;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 40, 40);

        public MazeKey(Texture2D texture, Vector2 position)
        {
            _texture = texture;
            Position = position;
        }

        public void Collect() => IsActive = false;
        public void Activate() => IsActive = true;

        public void Draw(SpriteBatch sb, int tileSize)
        {
            if (!IsActive) return;

            int size = (int)(tileSize * 0.5f);
            int offset = (tileSize - size) / 2;
            var destRect = new Rectangle((int)Position.X + offset, (int)Position.Y + offset, size, size);
            sb.Draw(_texture, destRect, Color.Gold);
        }
    }

    public class MazeLayout
    {
        public int[,] grid = new int[,]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,1,0,0,0,1,0,0,1,0,1,0,0,0,1,0,1,0,0,0,1,0,0,0,1,0,0,1},
            {1,0,1,1,0,1,0,1,0,1,1,0,1,1,1,0,1,0,1,1,1,0,1,0,1,0,1,0,1,1},
            {1,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,1},
            {1,1,0,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,1},
            {1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,0,1,0,1,1,1,1,1,1},
            {1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1,0,1,0,1,0,0,0,0,1},
            {1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,0,1,1,1,0,1,0,1,1,0,1},
            {1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,1,0,0,1},
            {1,1,1,0,1,0,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,0,1,0,1,1},
            {1,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,1,1,1,1,0,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1},
            {1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,2,1}
        };

        public bool IsWall(int x, int y)
        {
            if (x < 0 || y < 0 || y >= grid.GetLength(0) || x >= grid.GetLength(1))
                return true;
            return grid[y, x] == 1;
        }

        public List<Vector2> FindPathAStar(int startX, int startY, int goalX, int goalY)
        {
            var nodes = new MazeNode[grid.GetLength(0), grid.GetLength(1)];
            for (int y = 0; y < grid.GetLength(0); y++)
                for (int x = 0; x < grid.GetLength(1); x++)
                    nodes[y, x] = new MazeNode(x, y, grid[y, x] == 1);

            var start = nodes[startY, startX];
            var goal = nodes[goalY, goalX];

            var open = new List<MazeNode> { start };
            var closed = new HashSet<MazeNode>();

            start.GCost = 0;
            start.HCost = Heuristic(start, goal);

            while (open.Count > 0)
            {
                var current = open.OrderBy(n => n.FCost).First();
                open.Remove(current);
                closed.Add(current);

                if (current.Equals(goal))
                    return ReconstructPath(start, goal);

                foreach (var neighbor in GetNeighbors(nodes, current))
                {
                    if (neighbor.IsWall || closed.Contains(neighbor))
                        continue;

                    int newG = current.GCost + Heuristic(current, neighbor);

                    if (newG < neighbor.GCost || !open.Contains(neighbor))
                    {
                        neighbor.GCost = newG;
                        neighbor.HCost = Heuristic(neighbor, goal);
                        neighbor.Parent = current;

                        if (!open.Contains(neighbor))
                            open.Add(neighbor);
                    }
                }
            }
            return new List<Vector2>();
        }

        private List<MazeNode> GetNeighbors(MazeNode[,] nodes, MazeNode node)
        {
            var neighbors = new List<MazeNode>();
            int maxRows = grid.GetLength(0);
            int maxCols = grid.GetLength(1);

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = node.X + dx[i];
                int ny = node.Y + dy[i];

                if (nx >= 0 && nx < maxCols && ny >= 0 && ny < maxRows)
                    neighbors.Add(nodes[ny, nx]);
            }
            return neighbors;
        }

        private int Heuristic(MazeNode a, MazeNode b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Vector2> ReconstructPath(MazeNode start, MazeNode end)
        {
            var path = new List<Vector2>();
            var current = end;

            while (current != null && !current.Equals(start))
            {
                path.Add(new Vector2(current.X * 40, current.Y * 40));
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }

    public class MazeNode
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsWall { get; set; }
        public MazeNode Parent { get; set; }
        public int GCost { get; set; } = int.MaxValue;
        public int HCost { get; set; }
        public int FCost => GCost + HCost;

        public MazeNode(int x, int y, bool isWall)
        {
            X = x;
            Y = y;
            IsWall = isWall;
        }

        public override int GetHashCode() => X * 10000 + Y;

        public override bool Equals(object obj) =>
            obj is MazeNode other && X == other.X && Y == other.Y;
    }
}  