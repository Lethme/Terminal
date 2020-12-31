using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.Shell
{
    public class Shell
    {
        private String _command { get; set; }
        private String _base { get; set; }
        private List<String> _args { get; } = new List<string>();
        private Dictionary<String, Action<List<String>>> _commandList { get; } = new Dictionary<String, Action<List<String>>>();
        private Dictionary<String, Action<List<String>>> _defaultCommandList { get; } = new Dictionary<string, Action<List<string>>>();
        public IEnumerable<String> DefaultCommands => _defaultCommandList.Select(pair => pair.Key);
        public Action Reference { get; set; } = () =>
        {
            Console.WriteLine("Use 'help' to see all the available commands in current shell!");
            Console.WriteLine("Use 'help <utility>' to see an additional information!\n");
        };
        public Shell()
        {
            this.Initialize();
        }
        public Shell(params Command[] commands)
        {
            this.Initialize();
            this.Initialize(commands);
        }
        public Shell(IEnumerable<Command> commands)
        {
            this.Initialize();
            this.Initialize(commands);
        }
        private void Initialize()
        {
            this.Initialize(
                Command.Create("help", Args => {
                    Console.WriteLine("You can override 'help' command using method Bind\n");
                }),
                Command.Create("cls", Args => {
                    this.Clear();
                }),
                Command.Create("exit", Args => {
                    if (Shell.Confirmation("You really want to exit?"))
                    {
                        Environment.Exit(0);
                    }
                }),
                Command.Create("commands", Args => {
                    if (Args.Count == 0)
                    {
                        foreach (var command in _commandList)
                        {
                            Console.WriteLine(command.Key);
                        }
                        Console.WriteLine();
                        return;
                    }
                    
                    switch (Args[0])
                    {
                        case "-a":
                            {
                                foreach (var command in _commandList)
                                {
                                    Console.WriteLine(command.Key);
                                }
                                Console.WriteLine();

                                break;
                            }
                        case "-d":
                            {
                                foreach (var command in DefaultCommands)
                                {
                                    Console.WriteLine(command);
                                }
                                Console.WriteLine();

                                break;
                            }
                    }
                })
            );
        }
        private void Initialize(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
            {
                if (!(_defaultCommandList.ContainsKey(command.command.ToLower())))
                {
                    _defaultCommandList.Add(command.command.ToLower(), command.action);
                }
                else
                {
                    _defaultCommandList[command.command.ToLower()] = command.action;
                }
            }
            
            this.Bind(_defaultCommandList);
        }
        private void Initialize(params Command[] commands)
        {
            foreach (var command in commands)
            {
                if (!(_defaultCommandList.ContainsKey(command.command.ToLower())))
                {
                    _defaultCommandList.Add(command.command.ToLower(), command.action);
                }
                else
                {
                    _defaultCommandList[command.command.ToLower()] = command.action;
                }
            }

            this.Bind(_defaultCommandList);
        }
        public void Bind(String command, Action<List<String>> action)
        {
            if (!(_commandList.ContainsKey(command.ToLower())))
            {
                _commandList.Add(command.ToLower(), action);
            }
            else
            {
                if (!DefaultCommands.Contains(command.ToLower()) || command.ToLower() == "help")
                {
                    _commandList[command.ToLower()] = action;
                }
            }
        }
        public void Bind(params Command[] commands)
        {
            foreach (var item in commands)
            {
                this.Bind(item.command, item.action);
            }
        }
        public void Bind(IEnumerable<Command> commands)
        {
            foreach (var item in commands)
            {
                this.Bind(item.command, item.action);
            }
        }
        private void Bind(Dictionary<String, Action<List<String>>> commands)
        {
            foreach (var command in commands)
            {
                this.Bind(command.Key, command.Value);
            }
        }
        private String[] ArgsCollection(string ParamStr, string Pattern = "([\"].+?[\"]|[^ ]+)+")
        {
            var regex = new Regex(Pattern, RegexOptions.Singleline);
            return regex.Matches(ParamStr).Cast<Match>().Select(match => match.ToString()).ToArray();
        }
        private string GetArg(string ParamStr, int ParamNumber, string Pattern = "([\"].+?[\"]|[^ ]+)+")
        {
            var Params = ArgsCollection(ParamStr, Pattern);
            if (Params.Length - 1 < ParamNumber) return string.Empty;
            else return Params[ParamNumber].ToString();
        }
        private string Enter(string Marker = ">")
        {
            string tempCommand;
            do
            {
                Console.Write($"{Marker} "); tempCommand = Console.ReadLine();
            } while (tempCommand == String.Empty);
            return tempCommand;
        }
        private void GetArgs()
        {
            var args = ArgsCollection(_command);
            _base = args[0].ToString();
            _args.Clear();
            _args.AddRange(args.Where((arg, index) => index > 0).Select(arg => arg.Trim('"')));
        }
        public void Clear()
        {
            Console.Clear();
            Reference.Invoke();
        }
        private void Execute()
        {
            if (_commandList.ContainsKey(_base))
            {
                Action<List<String>> action;
                _commandList.TryGetValue(_base.ToLower(), out action);
                if (action != null) action.Invoke(_args);
            }
        }
        public void Execute(String command)
        {
            _command = command;
            GetArgs();
            Execute();
        }
        public void Execute(params String[] commands)
        {
            foreach (var command in commands) this.Execute(command);
        }
        public void Execute(IEnumerable<String> commands)
        {
            foreach (var command in commands) this.Execute(command);
        }
        public static bool Confirmation(string Line = "")
        {
            char symb;
            Console.CursorVisible = false;
            do
            {
                Console.CursorLeft = 0;
                Console.Write($"{Line} y/n: ");
                symb = Console.ReadKey().KeyChar;
            } while (symb != 'y' && symb != 'n');
            Console.WriteLine("\n");

            Console.CursorVisible = true;

            switch (symb)
            {
                case 'y': return true;
                case 'n': return false;
            }

            return false;
        }
        public void Run(IEnumerable<string> Args = null, string ConsoleTitle = "Terminal")
        {
            Console.Title = ConsoleTitle;

            this.Clear();
            if (Args != null) this.Execute(Args);

            while (true)
            {
                _command = this.Enter(">");
                this.GetArgs();
                this.Execute();
            }
        }
    }
    public struct Command
    {
        public string command;
        public Action<List<string>> action;
        public Command(string command, Action<List<string>> action)
        {
            this.command = command;
            this.action = action;
        }
        public override bool Equals(object obj)
        {
            return obj is Command other &&
                   command == other.command &&
                   EqualityComparer<Action<List<string>>>.Default.Equals(action, other.action);
        }
        public override int GetHashCode()
        {
            int hashCode = -1969733759;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(command);
            hashCode = hashCode * -1521134295 + EqualityComparer<Action<List<string>>>.Default.GetHashCode(action);
            return hashCode;
        }
        public void Deconstruct(out string command, out Action<List<string>> action)
        {
            command = this.command;
            action = this.action;
        }
        public static implicit operator (string command, Action<List<string>> action)(Command value)
        {
            return (value.command, value.action);
        }
        public static implicit operator Command((string command, Action<List<string>> action) value)
        {
            return new Command(value.command, value.action);
        }
        public static Command Create(string command, Action<List<string>> action) => new Command(command, action);

        public static bool operator ==(Command left, Command right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Command left, Command right)
        {
            return !(left == right);
        }
    }
}
