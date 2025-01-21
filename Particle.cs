using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake
{
    public class Particle : IDisposable
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        public Shape shape { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }

        private bool disposed = false;

        public Particle(double Width, double Height, Brush Color)
        {
            shape = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Color
            };

            PositionX = Width / 2.0;
            PositionY = Height / 2.0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (shape != null)
                    {
                        shape = null;
                    }
                }
                disposed = true;
            }
        }

        ~Particle()
        {
            Dispose(false);
        }
    }
}
