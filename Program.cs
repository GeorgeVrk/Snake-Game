using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Serilog;

namespace Snake
{
    internal class Program
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        #region Properties
        private static double Width = 1300;
        private static double Height = 740;
        private static Directions? direction = null;
        private static List<Particle> Particles = new List<Particle>();
        private static List<Particle> Tail = new List<Particle>();
        private static CompositionTarget handler;
        #endregion
        

        [STAThread]
        static void Main(string[] args)
        {
            var program = new Program();
            var app = new Application();
            var window = program.CreateWindow();
            var canvas = program.CreateCanvas();
            var scoreBox = program.CreateScoreBox(canvas);
            var foodHandler = new FoodHandler(window, canvas, Particles);
            var gameHandler = new GameHandler(window, canvas, direction);
            var gameOver = new GameOver(canvas);

            var compHandler = new ComponentHandler(window);
            compHandler.OnDirectionChanged += HandleMovementChange;
            compHandler.MonitorMovement();

            SnakeObj snake = new SnakeObj();
            snake.CreateSnakeHead(canvas, window, Tail);



            CompositionTarget.Rendering += (s, d) =>
            {
                if (direction != null)
                {
                    if (gameHandler.Update(canvas,direction,Tail))
                    {
                        gameOver.GameOverScreen();
                    }
                    else
                    {
                        int n = 1;
                        if (foodHandler.GetFoodList().Count < n)
                        {
                            foodHandler.SpawnRandomParticles(n);
                        }
                        gameHandler.CheckCollision(scoreBox,Particles,Tail);
                        snake.AddTail(Tail);
                    }
                }
            };

            window.Content = canvas;
            app.Run(window);
        }


        private static void HandleMovementChange(Directions? newDirections)
        {
            direction = newDirections;
        }

        public Window CreateWindow()
        {
            s_log.Verbose("Creating window...");

            Window window = new Window
            {
                Width = Width,
                Height = Height,
                Title = "Snake Game"
            };

            window.Focus();

            s_log.Verbose("Window Created...");

            return window;
        }

        public Canvas CreateCanvas()
        {
            s_log.Verbose("Creating canvas...");

            Canvas canvas = new Canvas
            {
                Width = 1280,
                Height = 720,
                Background = Brushes.Black
            };
            s_log.Verbose("Window canvas...");

            return canvas;
        }

        public TextBox CreateScoreBox(Canvas canvas)
        {
            s_log.Verbose("Creating score box...");

            TextBox textBox = new TextBox
            {
                Text = $"Score = {0}"
            };
            s_log.Verbose("Score box created...");
            canvas.Children.Add (textBox);
            return textBox;
        }
    }
}
