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
    public class SnakeObj
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(SnakeObj));
        #endregion

        public void CreateSnakeHead(Canvas canvas, Window window, List<Particle> Tail)
        {
            s_log.Verbose("Creating snake head...");
            Particle snakeHead = new Particle(window.Width, window.Height, Brushes.Green);
            Tail.Add(snakeHead);
            Canvas.SetLeft(snakeHead.shape, window.Width / 2.0);
            Canvas.SetTop(snakeHead.shape, window.Height / 2.0);
            canvas.Children.Add(snakeHead.shape);
            s_log.Verbose("Snake head created succesfully...");
        }

        public void AddTail(List<Particle> tail)
        {
            var prevX = tail[0].PositionX;
            var prevY = tail[0].PositionY;
            var distance = 8;

            if (tail.Count > 1)
            {
                for (int i = 1; i < tail.Count; i++)
                {
                    var tempX = tail[i].PositionX;
                    var tempY = tail[i].PositionY;

                    var deltaX = tail[i].PositionX - prevX;
                    var deltaY = tail[i].PositionY - prevY;

                    var magnitude = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    var unitX = (magnitude > 0) ? deltaX / magnitude : 0;
                    var unitY = (magnitude > 0) ? deltaY / magnitude : 0;

                    tail[i].PositionX = prevX + unitX * distance;
                    tail[i].PositionY = prevY + unitY * distance;

                    prevX = tempX;
                    prevY = tempY;


                    Canvas.SetLeft(tail[i].shape, tail[i].PositionX);
                    Canvas.SetTop(tail[i].shape, tail[i].PositionY);
                }
            }
        }
    }
}
