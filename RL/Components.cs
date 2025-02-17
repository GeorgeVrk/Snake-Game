using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Serilog;



namespace RL
{
    public static class Components
    {
        #region Logger
        private static readonly Serilog.ILogger s_log = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger()
            .ForContext(typeof(Components));
        #endregion

        #region Properties
        private static int Width = 1340;
        private static int Height = 740;
        private static int Rows = 20;
        private static int Cols = 20;
        private static int CellWidth = Width / Cols;
        private static int CellHeight = Height / Rows;
        private static List<Rectangle> snakeBody = new List<Rectangle>();
        private static Action? snakeDirection = null;
        private static Random random = new Random();
        private static Rectangle food;
        private static Canvas canvas;
        private static State state = new State();
        private static DispatcherTimer gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        private static int currentStep = 0;
        private static bool flag = true;
        public enum Action { UP, DOWN, LEFT, RIGHT }
        #endregion

        public static Window InitializeComponents()
        {
            canvas = CreateCanvas();
            CreateGrid(canvas);
            AddSnake(canvas);
            SpawnFood(canvas);
            var window = CreateWindow(canvas);
            gameTimer.Tick += async (s, e) =>
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    int snakeX = (int)Canvas.GetLeft(snakeBody[0]);
                    int sx = snakeX / 67;
                    int snakeY = (int)Canvas.GetTop(snakeBody[0]);
                    int sy = snakeY / 37;
                    int snakeState = (sy * Rows + sx);

                    int goalX = (int)Canvas.GetLeft(food);
                    int gx = goalX / 67;
                    int goalY = (int)Canvas.GetTop(food);
                    int gy = goalY / 37;
                    int foodState = (gy * Rows + gx);

                    var actions = state.Train(snakeState, foodState);
                    Console.WriteLine($"Snake at : ( {sx} , {sy} ), Goal at : ( {gx} , {gy} )");

                    await StartMovementAsync(actions);
                });
            };
            gameTimer.Start();

            return window;
        }

        private static Window CreateWindow(Canvas canvas)
        {
            s_log.Verbose("Creating window...");

            Window window = new Window
            {
                Width = Width + 100,
                Height = Height + 100,
                Title = "Snake Game",
                Content = canvas
            };

            window.KeyDown += Window_KeyDown;

            return window;
        }

        private static Canvas CreateCanvas()
        {
            return new Canvas
            {
                Width = Width,
                Height = Height,
                Background = Brushes.DarkGray
            };
        }

        private static void CreateGrid(Canvas canvas)
        {
            for (int i = 0; i <= Height; i += CellHeight)
            {
                Line horLine = new Line { Stroke = Brushes.LightGray, X1 = 0, Y1 = i, X2 = Width, Y2 = i };
                canvas.Children.Add(horLine);
            }

            for (int i = 0; i <= Width; i += CellWidth)
            {
                Line verLine = new Line { Stroke = Brushes.LightGray, X1 = i, Y1 = 0, X2 = i, Y2 = Height };
                canvas.Children.Add(verLine);
            }
        }

        private static void MoveSnake(Action? snakeDirection = null)
        {
            if (snakeDirection == null) return;

            var head = snakeBody[0];
            double headX = Canvas.GetLeft(head);
            double headY = Canvas.GetTop(head);

            switch (snakeDirection)
            {
                case Action.RIGHT: headX += CellWidth; break;
                case Action.DOWN: headY += CellHeight; break;
                case Action.LEFT: headX -= CellWidth; break;
                case Action.UP: headY -= CellHeight; break;
            }

            Rectangle newHead = new Rectangle { Width = CellWidth, Height = CellHeight, Fill = Brushes.Black };
            Canvas.SetLeft(newHead, headX);
            Canvas.SetTop(newHead, headY);
            snakeBody.Insert(0, newHead);
            canvas.Children.Add(newHead);

            if (snakeBody.Count > 1)
            {
                var tail = snakeBody[snakeBody.Count - 1];
                canvas.Children.Remove(tail);
                snakeBody.RemoveAt(snakeBody.Count - 1);
            }

            CheckSelfCollision();
            CheckFoodCollision();
            CheckBounds();
        }

        private static async Task StartMovementAsync(List<State.Action> actions)
        {
            if (actions == null || actions.Count == 0) return;
            gameTimer.Stop();

            foreach (var action in actions)
            {
                MoveSnakeSingleStep(action);
                await Task.Delay(100);
            }

            gameTimer.Start();
        }

        private static void MoveSnakeSingleStep(State.Action action)
        {
            var head = snakeBody[0];
            double headX = Canvas.GetLeft(head);
            double headY = Canvas.GetTop(head);

            switch (action)
            {
                case State.Action.RIGHT: headX += CellWidth; break;
                case State.Action.DOWN: headY += CellHeight; break;
                case State.Action.LEFT: headX -= CellWidth; break;
                case State.Action.UP: headY -= CellHeight; break;
            }

            Rectangle newHead = new Rectangle { Width = CellWidth, Height = CellHeight, Fill = Brushes.Black };
            Canvas.SetLeft(newHead, headX);
            Canvas.SetTop(newHead, headY);
            snakeBody.Insert(0, newHead);
            canvas.Children.Add(newHead);

            if (snakeBody.Count > 1)
            {
                var tail = snakeBody[snakeBody.Count - 1];
                canvas.Children.Remove(tail);
                snakeBody.RemoveAt(snakeBody.Count - 1);
            }

            //CheckSelfCollision();
            CheckFoodCollision();
            CheckBounds();
        }

        private static void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D: if (snakeDirection != Action.LEFT) snakeDirection = Action.RIGHT; break;
                case Key.S: if (snakeDirection != Action.UP) snakeDirection = Action.DOWN; break;
                case Key.A: if (snakeDirection != Action.RIGHT) snakeDirection = Action.LEFT; break;
                case Key.W: if (snakeDirection != Action.DOWN) snakeDirection = Action.UP; break;
            }

            MoveSnake(snakeDirection);
        }
        private static void AddSnake(Canvas canvas)
        {
            Rectangle snakeHead = new Rectangle { Width = CellWidth, Height = CellHeight, Fill = Brushes.Black };
            Canvas.SetLeft(snakeHead, 10 * CellWidth);
            Canvas.SetTop(snakeHead, 10 * CellHeight);
            canvas.Children.Add(snakeHead);
            snakeBody.Add(snakeHead);
        }

        private static void CheckBounds()
        {
            var head = snakeBody[0];
            double headX = Canvas.GetLeft(head);
            double headY = Canvas.GetTop(head);

            if (headX < 0 || headX >= Width || headY < 0 || headY >= Height)
            {
                EndGame();
            }
        }

        private static void EndGame()
        {
            s_log.Information("Game Over!");
            MessageBox.Show("Game Over!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            flag = false;
        }
        private static (int x, int y) GetRandomFoodPosition()
        {
            int randomCol = random.Next(0, Cols);
            int randomRow = random.Next(0, Rows);
            return (randomCol * CellWidth, randomRow * CellHeight);
        }

        private static void SpawnFood(Canvas canvas)
        {
            (int foodX, int foodY) = GetRandomFoodPosition();
            food = new Rectangle { Width = CellWidth, Height = CellHeight, Fill = Brushes.Yellow };
            Canvas.SetLeft(food, foodX);
            Canvas.SetTop(food, foodY);
            canvas.Children.Add(food);
        }
        private static void CheckFoodCollision()
        {
            var head = snakeBody[0];
            if (Canvas.GetLeft(head) == Canvas.GetLeft(food) && Canvas.GetTop(head) == Canvas.GetTop(food))
            {
                GrowSnake();
                canvas.Children.Remove(food);
                SpawnFood(canvas);
            }
        }

        private static void GrowSnake()
        {
            var tail = snakeBody[snakeBody.Count - 1];
            Rectangle newSegment = new Rectangle { Width = CellWidth, Height = CellHeight, Fill = Brushes.Black };
            Canvas.SetLeft(newSegment, Canvas.GetLeft(tail));
            Canvas.SetTop(newSegment, Canvas.GetTop(tail));
            snakeBody.Add(newSegment);
            canvas.Children.Add(newSegment);
        }

        private static void CheckSelfCollision()
        {
            var head = snakeBody[0];
            for (int i = 1; i < snakeBody.Count; i++)
            {
                if (Canvas.GetLeft(head) == Canvas.GetLeft(snakeBody[i]) &&
                    Canvas.GetTop(head) == Canvas.GetTop(snakeBody[i]))
                {
                    EndGame();
                }
            }
        }
    }
}
