using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Shell;

namespace Terminal
{
    class Program
    {
        private static List<Command> Commands { get; } = new List<Command>()
        {
            Command.Create("Hello", Args => {
                Console.WriteLine("Hello, World!\n");
            }),
            Command.Create("Test", Args => {
                foreach (var arg in Args) Console.WriteLine($"{arg}");
            }),
            Command.Create("Help", Args => {
                Console.WriteLine("This is a new help box!\n");
            })
        };
        static void Main(string[] args)
        {
            var shell = new Shell(Commands);

            shell.Bind("Test", Args => { Console.WriteLine("Nico\n"); });
            shell.Run();
        }
    }
}
