using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.UI
{
    // Allows WPF application to run with a console window (for displaying computer moves - hard to see)
    // Requires setting Properties -> Startup Object as Chessington.UI.Program rather than Chessington.UI.App (2 entries)
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
