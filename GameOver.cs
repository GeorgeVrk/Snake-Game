using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Snake
{
    internal class GameOver
    {
        public GameOver(Canvas canvas)
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
