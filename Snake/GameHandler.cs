using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace Snake
{
    public static class GameHandler
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        #region Properties
        public static double Width = 1340;
        public static double Height = 740;
        private static Directions? direction = null;
        private static List<Particle> Particles = new List<Particle>();
        public static List<Particle> Tail = new List<Particle>();
        public static FoodHandler foodHandler;
        public static PhysicsHandler gameHandler;
        private static GameOver gameOver;
        public static ComponentHandler compHandler;
        private static Application app;
        private static Canvas canvas;
        private static TextBox scoreBox;
        private static Window window;
        public static SnakeObj snake;
        #endregion

        public static void StartGame()
        {
            s_log.Information($"Initializing Components...");
            InitialiazeComponents();
            s_log.Information($"Starting game...");
            BeginRender();
        }

        public static void InitialiazeComponents()
        {
            window = CreateWindow();
            canvas = CreateCanvas();
            scoreBox = CreateScoreBox(canvas);
            app = new Application();
            foodHandler = new FoodHandler(window, canvas, Particles);
            gameHandler = new PhysicsHandler(window, canvas, direction);
            gameOver = new GameOver(canvas);
            compHandler = new ComponentHandler(window);
            compHandler.OnDirectionChanged += HandleMovementChange;
            compHandler.MonitorMovement();
            snake = new SnakeObj();
            snake.CreateSnakeHead(canvas, window, Tail);
        }

        public static void BeginRender(Directions direction)
        {
            CompositionTarget.Rendering += (sender, e) => RenderFrame(sender, e, direction);
            s_log.Information("Game started succesfully...");
            window.Content = canvas;
            app.Run(window);
        }

        private static void BeginRender()
        {
            CompositionTarget.Rendering += RenderFrame;
            s_log.Information("Game started succesfully...");
            window.Content = canvas;
            app.Run(window);
        }

        private static void StopRender(Directions? d = null)
        {
            CompositionTarget.Rendering -= (sender, e) => RenderFrame(sender, e, null);
        }

        private static void StopRender()
        {
            CompositionTarget.Rendering -= RenderFrame;
        }

        private static void RenderFrame(object sender, EventArgs e, Directions? direction)
        {
            if (direction != null)
            {
                if (gameHandler.Update(canvas, direction, Tail))
                {
                    //gameOver.GameOverScreen();
                    //StopRender();
                }
                else
                {
                    int n = 1;
                    if (foodHandler.GetFoodList().Count < n)
                    {
                        foodHandler.SpawnRandomParticles(n);
                    }
                    gameHandler.CheckCollision(scoreBox, Particles, Tail);
                    snake.AddTail(Tail);
                }
            }
        }

        private static void RenderFrame(object sender, EventArgs e)
        {
            if (direction != null)
            {
                if (gameHandler.Update(canvas, direction, Tail))
                {
                    gameOver.GameOverScreen();
                    StopRender();
                }
                else
                {
                    int n = 1;
                    if (foodHandler.GetFoodList().Count < n)
                    {
                        foodHandler.SpawnRandomParticles(n);
                    }
                    gameHandler.CheckCollision(scoreBox, Particles, Tail);
                    snake.AddTail(Tail);
                }
            }
        }

        private static void HandleMovementChange(Directions? newDirections)
        {
            direction = newDirections;
        }

        private static Window CreateWindow()
        {
            s_log.Verbose("Creating window...");

            Window window = new Window
            {
                Width = Width,
                Height = Height,
                Title = "Snake Game"
            };

            window.Focus();
            s_log.Verbose("Window Created succesfully...");

            return window;
        }

        private static Canvas CreateCanvas()
        {
            s_log.Verbose("Creating canvas...");

            Canvas canvas = new Canvas
            {
                Width = 1280,
                Height = 720,
                Background = Brushes.Black
            };
            s_log.Verbose("Canvas created succesfully...");

            return canvas;
        }

        private static TextBox CreateScoreBox(Canvas canvas)
        {
            s_log.Verbose("Creating score box...");

            TextBox textBox = new TextBox
            {
                Text = $"Score = {0}"
            };
            s_log.Verbose("Score box created succesfully...");
            canvas.Children.Add(textBox);
            return textBox;
        }
    }
}
