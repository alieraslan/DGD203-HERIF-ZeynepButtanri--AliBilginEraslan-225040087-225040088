using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Herif
{
    // Public : Terminalde Gözüken     Private : Terminalde Gözükmeyen
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _pixel;
        private SpriteFont _font;

        // Ekran Büyüklüğünü ayarlamak için ve Sabit olduğunu belirtmek için
        private const int ScreenW = 1920;
        private const int ScreenH = 1080;

        // Zemin Büyüklüğünü ayarlamak için ve sabit olduğunu belirtmek için
        private const int GroundHeight = 120;
        private float _groundScrollX;
        private const float GroundSpeed = 420f;

        // Oyun objeleri
        private Bird _bird;
        private readonly List<PipePair> _pipes = new();
        private readonly Random _rng = new();

        // Boruların Konumunu ayarlamak için 
        private float _pipeSpawnTimer;
        private const float PipeSpawnInterval = 1.75f; 
        private const float PipeSpeed = 420f;

        // Skor ve high score değerleri
        private int _score;
        private int _highScore;

        // Oyunun durumunu belirtir. Sabit liste
        private enum GameState { Menu, Playing, GameOver }
        private GameState _state;

        // Fare ve Klavye Durumları
        private KeyboardState _prevKb;
        private MouseState _prevMouse;

        // Butonlar
        private Rectangle _btnStart, _btnExit;
        private Rectangle _btnRetry, _btnMenu, _btnExit2;

         // Çözünürlük Ektan Boyutu Optimizasyonu
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = ScreenW;
            _graphics.PreferredBackBufferHeight = ScreenH;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.Title = "HERIF";
        }

        protected override void Initialize()
        {
            base.Initialize();
            BuildUI();
            ResetToMenu(); // ✅ Menüye dön (metod aşağıda var)
        }

        protected override void LoadContent()
        {
         _spriteBatch = new SpriteBatch(GraphicsDevice);

         _pixel = new Texture2D(GraphicsDevice, 1, 1);
         _pixel.SetData(new[] { Color.White });

         _font = Content.Load<SpriteFont>("DefaultFont");
        }

        private void BuildUI()
        {
            // Menü butonları
            _btnStart = CenterRect(360, 90, y: 520);
            _btnExit = CenterRect(360, 90, y: 640);

            // GameOver butonları
            _btnRetry = CenterRect(360, 90, y: 620);
            _btnMenu = CenterRect(360, 90, y: 740);
            _btnExit2 = CenterRect(360, 90, y: 860);
        }

        private Rectangle CenterRect(int w, int h, int y)
        {
            int x = (ScreenW - w) / 2;
            return new Rectangle(x, y, w, h);
        }

        // ✅ Eksik olduğu için hata aldığın metod: burada, tam hali
        private void ResetToMenu()
        {
            _state = GameState.Menu;

            _score = 0;
            _pipes.Clear();
            _pipeSpawnTimer = 0f;
            _groundScrollX = 0f;

            _bird = new Bird(
                startPos: new Vector2(ScreenW * 0.30f, ScreenH * 0.45f),
                size: new Point(64, 48)
            );
        }

        private void StartGame()
        {
            _state = GameState.Playing;

            _score = 0;
            _pipes.Clear();
            _pipeSpawnTimer = 0f;
            _groundScrollX = 0f;

            _bird = new Bird(
                startPos: new Vector2(ScreenW * 0.30f, ScreenH * 0.45f),
                size: new Point(64, 48)
            );
        }

        private void SetGameOver()
        {
            _state = GameState.GameOver;
            if (_score > _highScore) _highScore = _score;
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            var mouse = Mouse.GetState();
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kb.IsKeyDown(Keys.Escape))
                Exit();

            bool click = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            Point mpos = mouse.Position;

            switch (_state)
            {
                case GameState.Menu:
                    UpdateMenu(kb, click, mpos);
                    break;

                case GameState.Playing:
                    UpdatePlaying(kb, mouse, dt);
                    break;

                case GameState.GameOver:
                    UpdateGameOver(kb, click, mpos);
                    break;
            }

            _prevKb = kb;
            _prevMouse = mouse;

            base.Update(gameTime);
        }

        private void UpdateMenu(KeyboardState kb, bool click, Point mpos)
        {
            // Klavye: Enter başla, X çıkış
            if (JustPressed(kb, Keys.Enter)) StartGame();
            if (JustPressed(kb, Keys.X)) Exit();

            // Mouse
            if (click && _btnStart.Contains(mpos)) StartGame();
            if (click && _btnExit.Contains(mpos)) Exit();
        }

        private void UpdateGameOver(KeyboardState kb, bool click, Point mpos)
        {
            // Klavye: R yeniden dene, M menü, X çıkış
            if (JustPressed(kb, Keys.R)) StartGame();
            if (JustPressed(kb, Keys.M)) ResetToMenu();
            if (JustPressed(kb, Keys.X)) Exit();

            // Mouse
            if (click && _btnRetry.Contains(mpos)) StartGame();
            if (click && _btnMenu.Contains(mpos)) ResetToMenu();
            if (click && _btnExit2.Contains(mpos)) Exit();
        }

        private void UpdatePlaying(KeyboardState kb, MouseState mouse, float dt)
        {
            // Flap
            if (IsFlapPressed(kb, mouse))
                _bird.Flap();

            _bird.Update(dt);

            // Zemin kaydır
            _groundScrollX -= GroundSpeed * dt;
            if (_groundScrollX <= -ScreenW) _groundScrollX += ScreenW;

            // Boru üret (seyrek)
            _pipeSpawnTimer += dt;
            if (_pipeSpawnTimer >= PipeSpawnInterval)
            {
                _pipeSpawnTimer = 0f;
                SpawnPipePair();
            }

            // Borular + skor
            for (int i = _pipes.Count - 1; i >= 0; i--)
            {
                _pipes[i].Update(dt, PipeSpeed);

                if (!_pipes[i].Scored && _pipes[i].X + _pipes[i].Width < _bird.Position.X)
                {
                    _pipes[i].Scored = true;
                    _score++;
                }

                if (_pipes[i].X + _pipes[i].Width < -200)
                    _pipes.RemoveAt(i);
            }

            // Çarpışma
            if (CheckCollisions())
            {
                SetGameOver();
                return;
            }

            // Üst sınır
            if (_bird.Bounds.Top < 0)
                _bird.ClampTop();

            // Zemine değdi mi
            if (_bird.Bounds.Bottom >= ScreenH - GroundHeight)
            {
                SetGameOver();
                return;
            }
        }

        private void SpawnPipePair()
        {
            int gapSize = 260;   // 1080p için
            int pipeWidth = 120;

            int minY = 200;
            int maxY = ScreenH - GroundHeight - 200;
            int gapCenterY = _rng.Next(minY, maxY);

            int topPipeHeight = gapCenterY - gapSize / 2;
            int bottomPipeY = gapCenterY + gapSize / 2;
            int bottomPipeHeight = (ScreenH - GroundHeight) - bottomPipeY;

            var pair = new PipePair(
                startX: ScreenW + 120,
                width: pipeWidth,
                topHeight: topPipeHeight,
                bottomY: bottomPipeY,
                bottomHeight: bottomPipeHeight
            );

            _pipes.Add(pair);
        }

        private bool CheckCollisions()
        {
            Rectangle birdRect = _bird.Bounds;

            for (int i = 0; i < _pipes.Count; i++)
            {
                if (_pipes[i].TopRect.Intersects(birdRect)) return true;
                if (_pipes[i].BottomRect.Intersects(birdRect)) return true;
            }

            return false;
        }

        private bool IsFlapPressed(KeyboardState kb, MouseState mouse)
        {
            bool key =
                JustPressed(kb, Keys.Space) ||
                JustPressed(kb, Keys.W) ||
                JustPressed(kb, Keys.Up);

            bool click = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

            return key || click;
        }

        private bool JustPressed(KeyboardState kb, Keys k)
            => kb.IsKeyDown(k) && !_prevKb.IsKeyDown(k);

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_font == null ? Color.Magenta : new Color(120, 200, 255));

            _spriteBatch.Begin();

            DrawBackground();

            // Borular (Playing / GameOver)
            if (_state == GameState.Playing || _state == GameState.GameOver)
            {
                foreach (var p in _pipes)
                {
                    DrawRect(p.TopRect, new Color(40, 200, 80));
                    DrawRect(p.BottomRect, new Color(40, 200, 80));

                    var capTop = new Rectangle(p.TopRect.X - 10, p.TopRect.Bottom - 28, p.TopRect.Width + 20, 28);
                    var capBottom = new Rectangle(p.BottomRect.X - 10, p.BottomRect.Y, p.BottomRect.Width + 20, 28);
                    DrawRect(capTop, new Color(30, 170, 70));
                    DrawRect(capBottom, new Color(30, 170, 70));
                }
            }

            // Kuş (her durumda çiz)
            DrawRect(_bird.Bounds, new Color(255, 220, 50));
            var eye = new Rectangle(_bird.Bounds.X + _bird.Bounds.Width - 16, _bird.Bounds.Y + 10, 10, 10);
            DrawRect(eye, Color.Black);

            // Zemin
            DrawRect(new Rectangle(0, ScreenH - GroundHeight, ScreenW, GroundHeight), new Color(230, 200, 120));
            DrawRect(new Rectangle(0, ScreenH - GroundHeight, ScreenW, 18), new Color(90, 200, 90));

            // UI (Font varsa)
            if (_font != null)
            {
                if (_state == GameState.Menu)
                {
                    DrawCenteredText("HERIF", y: 280, scale: 2.2f, Color.Black);
                    DrawCenteredText("JUMP with SPACE / ^ / MOUSE", y: 370, scale: 1.0f, Color.Black);
                    DrawCenteredText("ENTER: START | X: EXIT", y: 410, scale: 0.9f, Color.Black);

                    DrawButton(_btnStart, "START");
                    DrawButton(_btnExit, "EXIT");

                    DrawCenteredText($"HIGH SCORE: {_highScore}", y: 430, scale: 1.0f, Color.Black);
                }
                else if (_state == GameState.Playing)
                {
                    _spriteBatch.DrawString(_font, $"SCORE: {_score}", new Vector2(24, 24), Color.Black);
                    _spriteBatch.DrawString(_font, $"HIGH SCORE: {_highScore}", new Vector2(24, 64), Color.Black);
                }
                else if (_state == GameState.GameOver)
                {
                    DrawCenteredText("YOU DEAD", y: 280, scale: 2.0f, Color.DarkRed);
                    DrawCenteredText($"SCORE: {_score}", y: 360, scale: 1.3f, Color.Black);
                    DrawCenteredText($"HIGH SCORE: {_highScore}", y: 410, scale: 1.1f, Color.Black);

                    DrawButton(_btnRetry, "TRY AGAIN ( R )");
                    DrawButton(_btnMenu, "MENU ( M )");
                    DrawButton(_btnExit2, "EXIT ( X )");
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            // basit bulutlar
            DrawRect(new Rectangle(180, 120, 320, 50), new Color(240, 250, 255));
            DrawRect(new Rectangle(820, 180, 420, 60), new Color(240, 250, 255));
            DrawRect(new Rectangle(520, 280, 520, 55), new Color(240, 250, 255));
        }

        private void DrawCenteredText(string text, int y, float scale, Color color)
        {
            if (_font == null) return;

            Vector2 size = _font.MeasureString(text) * scale;
            Vector2 pos = new Vector2((ScreenW - size.X) / 2f, y);
            _spriteBatch.DrawString(_font, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        private void DrawButton(Rectangle r, string text)
        {
            var mouse = Mouse.GetState();
            bool hover = r.Contains(mouse.Position);

            DrawRect(r, hover ? new Color(30, 30, 30) : new Color(10, 10, 10));
            DrawRect(new Rectangle(r.X + 4, r.Y + 4, r.Width - 8, r.Height - 8),
                hover ? new Color(230, 230, 230) : new Color(210, 210, 210));

            // Buton metnini ortala
            DrawCenteredText(text, r.Center.Y - 16, 1.0f, Color.Black);
        }

        private void DrawRect(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_pixel, rect, color);
        }
    }
}
