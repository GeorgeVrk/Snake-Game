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
        [STAThread]
        static void Main(string[] args)
        {
            State.Train(10000, 0.5, 0.9, 0.1);
        }
    }
}
