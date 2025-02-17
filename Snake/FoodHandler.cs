using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Serilog;

namespace Snake
{
    public class FoodHandler
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        private Window window;
        private Canvas canvas;
        public List<Particle> Food;
        private Random random = new Random();

        public FoodHandler(Window window, Canvas canvas, List<Particle> Food) 
        {
            this.window = window;
            this.canvas = canvas;
            this.Food = Food;
        }
        public void SpawnRandomParticles(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Particle particle = new Particle(window.Width, window.Height, Brushes.Yellow);
                particle.PositionX += random.Next(-300, 301);
                particle.PositionY += random.Next(-300, 301);
                Food.Add(particle);
                Canvas.SetLeft(particle.shape, particle.PositionX);
                Canvas.SetTop(particle.shape, particle.PositionY);
                canvas.Children.Add(particle.shape);
                s_log.Information($"Spawning food at X : {particle.PositionX}, Y : {particle.PositionY}");
            }
        }

        public List<Particle> GetFoodList()
        {
            return Food;
        }
    }
}
