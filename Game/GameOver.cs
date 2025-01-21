using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Serilog;

namespace Snake
{
    internal class GameOver
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(GameOver));
        private Canvas _canvas;
        #endregion

        public GameOver(Canvas canvas)
        {
            _canvas = canvas;
        }
        public void GameOverScreen()
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
            Canvas.SetLeft(text, (_canvas.Width / 2.0) - 200);
            Canvas.SetTop(text, (_canvas.Height / 2.0) - 25);
            s_log.Information("Game Over...");
            _canvas.Children.Add(text);
        }
    }
}
