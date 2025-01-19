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
    public static class SnakeObj
    {
        public static void CreateSnakeHead(Canvas canvas, Window window, List<Particle> Particles, TextBox score)
        {
            Particle particle = new Particle(window.Width, window.Height, Brushes.Green);
            Particles.Add(particle);
            Canvas.SetLeft(particle.shape, window.Width / 2.0);
            Canvas.SetTop(particle.shape, window.Height / 2.0);
            canvas.Children.Add(particle.shape);
            canvas.Children.Add(score);
        }

        public static void AddTail(List<Particle> Particles, List<Particle> tail)
        {
            var prevX = Particles[0].PositionX;
            var prevY = Particles[0].PositionY;
            var distance = 8;

            foreach (Particle p in tail)
            {
                var tempX = p.PositionX;
                var tempY = p.PositionY;

                var deltaX = p.PositionX - prevX;
                var deltaY = p.PositionY - prevY;

                var magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                var unitX = (magnitude > 0) ? deltaX / magnitude : 0;
                var unitY = (magnitude > 0) ? deltaY / magnitude : 0;

                p.PositionX = prevX + unitX * distance;
                p.PositionY = prevY + unitY * distance;

                prevX = tempX;
                prevY = tempY;


                Canvas.SetLeft(p.shape, p.PositionX);
                Canvas.SetTop(p.shape, p.PositionY);
            }
        }
    }
}
