using Mono.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Snake
{
    internal class Options
    {
        #region Logger
        private static Serilog.ILogger s_log = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger().ForContext(typeof(Program));
        #endregion

        public bool manual = false;
        public bool auto = false;

        public void Parse(string[] args) 
        {
            var o = new OptionSet
            {
                {"m|manual", "Play manually", m =>  manual = true},
                {"a|auto", "AI play", a => auto = true},
            };

            try
            {
                o.Parse(args);
            }
            catch (Exception e)
            {
                s_log.Error($"Error parsing data. ExMessage : {e.Message}");
            }
        }

    }
}
