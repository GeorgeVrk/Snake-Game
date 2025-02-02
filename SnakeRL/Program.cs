using Serilog;
using Snake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeRL
{
    internal class Program
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            ParseOptions(args);
        }

        public static void ParseOptions(string[] args)
        {
            var o = new Options();
            try
            {
                o.Parse(args);
                if (o.auto && o.manual)
                {
                    throw new ArgumentException("Invalid options: Choose either -a (auto) or -m (manual), not both.");
                }
                if (o.manual)
                {
                    GameHandler.StartGame();
                }
                else if (o.auto)
                {
                    State.Train();

                }
                else
                {
                    s_log.Error("No arguments passed to the debugger.");
                    Console.ReadKey();
                    throw new ArgumentException("No valid option selected. Use -a for auto or -m for manual.");
                }
            }
            catch (Exception ex)
            {
                s_log.Error($"Error parsing options: {ex.Message}");
            }
        }
    }
}
