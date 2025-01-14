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

namespace Snake
{
    internal class Program
    {
        private static double Width = 1300;
        private static double Height = 740;
        private static Window window;
        private static string direction = null;
        private static List<Particle> Particles = new List<Particle>();
        private static List<Particle> Tail = new List<Particle>();
        private static CompositionTarget handler;
        private static int Score = 0;
        private static TextBox score = new TextBox
        {
            Text = $"Score = {Score}",
        };

        [STAThread]
        static void Main(string[] args)
        {
            Program program = new Program();
            Application app = new Application();

            window = new Window
            {
                Width = Width,
                Height = Height,
                Title = "Snake"
            };
            window.Focus();

            Canvas canvas = new Canvas
            {
                Width = 1280,
                Height = 720,
                Background = Brushes.Black
            };

            CreateSnakeHead(canvas,window);
            Thread.Sleep(500);

            window.KeyDown += Movement;

            CompositionTarget.Rendering += (s, d) =>
            {
                if (direction != null)
                {
                    if (Update(canvas, direction))
                    {
                        GameOver(canvas);
                        
                    }
                    else
                    {
                        int n = 1;
                        if (Particles.Count - 1 < n)
                        {
                            SpawnRandomParticles(canvas, n);
                        }
                        CheckCollision(canvas);
                        AddTail(Tail);
                        
                    }
                }
            };

            window.Content = canvas;
            app.Run(window);
        }

        public static void CreateSnakeHead(Canvas canvas, Window window)
        {
            Particle particle = new Particle(window.Width,window.Height, Brushes.Green);
            Particles.Add(particle);
            Canvas.SetLeft(particle.shape,window.Width / 2.0);
            Canvas.SetTop(particle.shape, window.Height / 2.0);
            canvas.Children.Add(particle.shape);
            canvas.Children.Add(score);
        }

        public static bool Update(Canvas canvas, string dir)
        {
            Particle particle = Particles[0];
            var flag = false;
            if (!CheckBounds(canvas, particle))
            {
                switch (dir)
                {
                    case "Up":
                        particle.PositionY = particle.PositionY - 1;
                        break;
                    case "Down":
                        particle.PositionY = particle.PositionY + 1;
                        break;
                    case "Left":
                        particle.PositionX = particle.PositionX - 1;
                        break;
                    case "Right":
                        particle.PositionX = particle.PositionX + 1;
                        break;
                };
            }
            else
            {
                direction = null;
                flag = true;
            }

            Canvas.SetLeft(particle.shape, particle.PositionX);
            Canvas.SetTop(particle.shape, particle.PositionY);
            return flag;
        }

        public static bool CheckBounds(Canvas canvas, Particle particle)
        {
            var flag = false;
            if (particle.PositionY > canvas.Height - particle.shape.Width)
            {
                direction = null;
                particle.PositionY = canvas.Height + 1;
                Canvas.SetTop(particle.shape, particle.PositionY);
                flag = true;
            }
            if (particle.PositionY - particle.shape.Width <  0)
            {
                direction = null;
                particle.PositionY = -1;
                Canvas.SetTop(particle.shape, particle.PositionY);
                flag = true;
            }
            if (particle.PositionX > canvas.Width)
            {
                direction = null;
                particle.PositionX = canvas.Width + 1;
                Canvas.SetLeft(particle.shape, particle.PositionX);
                flag = true;
            }
            if (particle.PositionX - particle.shape.Width < 0)
            {
                direction = null;
                particle.PositionX = -1;
                Canvas.SetLeft(particle.shape, particle.PositionX);
                flag = true;
            }
            return flag;
        }

        public static void Movement(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    direction = "Up";
                    break;
                case Key.S:
                    direction = "Down";
                    break;
                case Key.D:
                    direction = "Right";
                    break;
                case Key.A:
                    direction = "Left";
                    break;
                default:
                    direction = null;
                    break;
            }
        }

        public static void SpawnRandomParticles(Canvas canvas, int n)
        {
            Random random = new Random();
            for (int i = 0; i < n; i++)
            {
                Particle particle = new Particle(window.Width, window.Height, Brushes.Yellow);
                particle.PositionX += random.Next(-300, 301);
                particle.PositionY += random.Next(-300, 301);
                Particles.Add(particle);
                Canvas.SetLeft(particle.shape, particle.PositionX);
                Canvas.SetTop(particle.shape, particle.PositionY);
                canvas.Children.Add(particle.shape);
            }
        }

        public static bool CheckCollision(Canvas canvas)
        {
            double distance;
            var flag = false;
            for (int i = 1; i < Particles.Count; i++)
            {
                distance = Math.Sqrt(Math.Pow(Particles[0].PositionX - Particles[i].PositionX, 2) + Math.Pow(Particles[0].PositionY - Particles[i].PositionY, 2));
                if (Math.Floor(distance) < 15.0)
                {
                    score.Text = $"Score = {++Score}";
                    canvas.Children.Remove(Particles[i].shape);
                    Particles[i].Dispose();
                    Particles.RemoveAt(i);
                    Particle tail = new Particle(window.Width, window.Height, Brushes.White);
                    if (direction == "Left")
                    {
                        tail.PositionX = Particles[0].PositionX + 16;
                        tail.PositionY = Particles[0].PositionY;
                        flag = true;
                    }
                    if (direction == "Right")
                    {
                        tail.PositionX = Particles[0].PositionX - 16;
                        tail.PositionY = Particles[0].PositionY;
                        flag = true;
                    }
                    if (direction == "Up")
                    {
                        tail.PositionX = Particles[0].PositionX;
                        tail.PositionY = Particles[0].PositionY + 16;
                        flag = true;
                    }
                    if (direction == "Down")
                    {
                        tail.PositionX = Particles[0].PositionX;
                        tail.PositionY = Particles[0].PositionY - 16;
                        flag = true;
                    }
                    Canvas.SetTop(tail.shape,tail.PositionY);
                    Canvas.SetLeft(tail.shape,tail.PositionX);
                    canvas.Children.Add(tail.shape);
                    Tail.Add(tail);
                }
            }
            return flag;
        }

        public static void AddTail(List<Particle> tail)
        {
            if (tail.Count != 0)
            {
                CheckDirection(tail[0], Particles[0]);
                if (tail.Count >= 2) {
                    for (int i = 1; i < tail.Count; i++)
                    {
                        CheckDirection(tail[i], tail[i-1]);
                    }
                }
            }
        }

        public static void CheckDirection(Particle par1, Particle par2)
        {
            if (direction == "Left")
            {
                par1.PositionX = par2.PositionX + 10;
                par1.PositionY = par2.PositionY;
            }
            if (direction == "Right")
            {
                par1.PositionX = par2.PositionX - 10;
                par1.PositionY = par2.PositionY;
            }
            if (direction == "Up")
            {
                par1.PositionX = par2.PositionX;
                par1.PositionY = par2.PositionY + 10;
            }
            if (direction == "Down")
            {
                par1.PositionX = par2.PositionX;
                par1.PositionY = par2.PositionY - 10;
            }
            Canvas.SetLeft(par1.shape, par1.PositionX);
            Canvas.SetTop(par1.shape, par1.PositionY);
        }


        public static void GameOver(Canvas canvas)
        {
            TextBlock text = new TextBlock
            {
                Text = "GAME OVER",
                Background = Brushes.Black,
                FontSize = 32,
                Width = 400,
                Height = 50,
                Foreground = Brushes.Green,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(text, (canvas.Width / 2.0) - 200);
            Canvas.SetTop(text, (canvas.Height / 2.0) - 25);
            canvas.Children.Add(text);
        }
    }
}
