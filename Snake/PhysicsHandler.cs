using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Snake
{
    public class PhysicsHandler
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        private Window window;
        private Canvas canvas;
        private Directions? direction;
        private static int Score = 0;


        public PhysicsHandler(Window window, Canvas canvas, Directions? direction) 
        {
            this.window = window;
            this.canvas = canvas;
            this.direction = direction;
        }

        public bool Update(Canvas canvas, Directions? dir, List<Particle> Tail)
        {
            Particle particle = Tail[0];
            var flag = false;
            if (!CheckBounds(canvas, particle))
            {
                switch (dir)
                {
                    case Directions.UP:
                        particle.PositionY = particle.PositionY - 1;
                        break;
                    case Directions.DOWN:
                        particle.PositionY = particle.PositionY + 1;
                        break;
                    case Directions.LEFT:
                        particle.PositionX = particle.PositionX - 1;
                        break;
                    case Directions.RIGHT:
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

        public bool CheckBounds(Canvas canvas, Particle particle)
        {
            var flag = false;
            if (particle.PositionY > canvas.Height - particle.shape.Width)
            {
                direction = null;
                particle.PositionY = canvas.Height + 1;
                Canvas.SetTop(particle.shape, particle.PositionY);
                flag = true;
            }
            if (particle.PositionY - particle.shape.Width < 0)
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

        public bool CheckCollision(TextBox scoreBox, List<Particle> Particles, List<Particle> Tail)
        {
            double distance;
            var flag = false;
            for (int i = 0; i < Particles.Count; i++)
            {
                distance = Math.Sqrt(Math.Pow(Tail[0].PositionX - Particles[i].PositionX, 2) + Math.Pow(Tail[0].PositionY - Particles[i].PositionY, 2));
                if (Math.Floor(distance) < 15.0)
                {
                    scoreBox.Text = $"Score = {++Score}";
                    canvas.Children.Remove(Particles[i].shape);
                    Particles[i].Dispose();
                    s_log.Information($"Food particle at X : {Particles[i].PositionX}, Y : {Particles[i].PositionX} disposed succesfully...");
                    Particles.RemoveAt(i);
                    Particle tail = new Particle(window.Width, window.Height, Brushes.White);
                    Tail.Add(tail);
                    canvas.Children.Add(tail.shape);
                    flag = true;
                }
            }
            return flag;
        }
    }
}
