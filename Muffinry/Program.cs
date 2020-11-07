using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;

namespace Muffinry
{
    class Program
    {

        const string version = "Alpha 0.3";
        public static BigInteger muffins = 250/*+1000000*/;

        public static sbyte gameMode = 0;
        public static bool exited = false;
        public static byte fps = 15;
        public static int selected = 0;
        public static int prevselected = 0;
        private static readonly Thread keyThread = new Thread(Keys);

        internal static List<FactoryType> Factories { get; private set; } = new List<FactoryType> {
            new FactoryType(1, 1)};
        internal static List<FactoryType> UpgradeableFactories { get; private set; } = new List<FactoryType> { };

        public static string[] menuOptions = new string[] {
            " - Buy a Muffin Factory (Cost: "+Factories[0].Cost.ToString()+" muffins)",
            " - Upgrade a Muffin Factory"
        };

        public static ConsoleColor[] Theme = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Cyan };

        public static string Titletext { get; private set; } = @"
 __       __  __    __  ________  ________  ______  __    __  _______  __      __ 
│  \     /  \│  \  │  \│        \│        \│      \│  \  │  \│       \|  \    /  \                      
│ ██\   /  ██│ ██  │ ██│ ████████│ ████████ \██████│ ██\ │ ██│ ███████\\██\  /  ██
│ ███\ /  ███│ ██  │ ██│ ██__    │ ██__      │ ██  │ ███\│ ██│ ██__│ ██ \██\/  ██ 
│ ████\  ████│ ██  │ ██│ ██  \   │ ██  \     │ ██  │ ████\ ██│ ██    ██  \██  ██  
│ ██\██ ██ ██│ ██  │ ██│ █████   │ █████     │ ██  │ ██\██ ██│ ███████\   \████   
│ ██ \███│ ██│ ██__/ ██│ ██      │ ██       _│ ██_ │ ██ \████│ ██  | ██   │ ██    
│ ██  \█ │ ██ \██    ██│ ██      │ ██      │   ██ \│ ██  \███│ ██  | ██   │ ██    
 \██      \██  \██████  \██       \██       \██████ \██   \██ \██   \██    \██
";

        static void Main(string[] args)
        {
            InitConsole();
            Game();
        }

        static void Game()
        {

            keyThread.Start();
            var timer = new Timer((e) => { GetMuffins(); }, null, 0, 5000);
            try
            {
                while (!exited)
                {
                    Console.CursorVisible = false;
                    ClearConsole();
                    switch (gameMode)
                    {
                        default:
                            Title();
                            break;
                        case 1:
                            Menu();
                            break;
                        case 2:
                            Upgrade();
                            break;
                        case -1:
                            InvalidAction();
                            break;
                    }
                    Thread.Sleep(1000 / fps);
                }
            } catch (OutOfMemoryException) {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(">:)\nYou ran out of memory, which means you're screwed!\nLife sucks, doesn't it?\n");
            } catch (Exception e) {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(":\\\n something bad happened and here's what and why: {0}\nOh well have fun staring at this message",e.Message);
            }
        }

        private static void Keys()
        {
            while (!exited)
            {
                switch (gameMode)
                {
                    case 0:
                        ConsoleKey key = Console.ReadKey().Key;
                        switch (key)
                        {
                            case ConsoleKey.Enter:
                                gameMode = 1;
                                break;
                        }
                        break;
                    case 1:
                        key = Console.ReadKey().Key;
                        switch (key)
                        {
                            case ConsoleKey.UpArrow:
                                if (selected > 0)
                                {
                                    selected--;
                                }
                                else
                                {
                                    selected = (byte)(menuOptions.Length - 1);
                                }
                                break;
                            case ConsoleKey.DownArrow:
                                if (selected < menuOptions.Length - 1)
                                {
                                    selected++;
                                }
                                else
                                {
                                    selected = 0;
                                }
                                break;
                            case ConsoleKey.Enter:
                                switch (selected)
                                {
                                    case 0:
                                        if (!Factories[0].Buy()) {
                                            gameMode = -1;
                                        }
                                        break;
                                    case 1:
                                        prevselected = selected;
                                        selected = 0;
                                        UpgradeableFactories.Clear();
                                        if (Factories[^1].Amount > 0)
                                        {
                                            Factories.Add(new FactoryType(Factories[^1].Level + 1, 0));
                                        }
                                        for (int i = 0; i < Factories.Count; i++)
                                        {
                                            if (Factories[i].Amount > 0)
                                            {
                                                UpgradeableFactories.Add(Factories[i]);
                                            }
                                        }
                                        gameMode = 2;
                                        break;
                                }
                                break;
                            case ConsoleKey.Escape:
                                gameMode = 0;
                                break;
                        }
                        break;
                    case 2:
                        key = Console.ReadKey().Key;
                        switch (key)
                        {
                            case ConsoleKey.UpArrow:
                                if (selected > 0)
                                {
                                    selected--;
                                }
                                else
                                {
                                    selected = (byte)(UpgradeableFactories.Count - 1);
                                }
                                break;
                            case ConsoleKey.DownArrow:
                                if (selected < UpgradeableFactories.Count - 1)
                                {
                                    selected++;
                                }
                                else
                                {
                                    selected = 0;
                                }
                                break;
                            case ConsoleKey.Enter:
                                if (UpgradeableFactories[selected].Upgrade())
                                {
                                    gameMode = 1;
                                    selected = prevselected;
                                }
                                else
                                {
                                    gameMode = -1;
                                }
                                break;
                            case ConsoleKey.Escape:
                                gameMode = 1;
                                selected = prevselected;
                                break;
                        }
                        break;
                }
            }
        }

        static void GetMuffins()
        {
            for (byte i = 0; i < Factories.Count; i++)
            {
                if (Factories[i].Amount > 0)
                {
                    new Thread(Factories[i].Muffins).Start();
                }
            }
        }

        static void InvalidAction() {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The action could not be performed. Try again later?\n\n");
            Console.ForegroundColor = Theme[1];
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            gameMode = 1;
            selected = prevselected;
        }

        static void ClearConsole()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder empty = new StringBuilder();
            empty.Append(' ', Console.BufferWidth);
            for (int i = 0; i < Console.BufferHeight; i++)
            {
                Console.WriteLine(empty);
            }
            Console.SetCursorPosition(0, 0);
        }
        static void InitConsole()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF32;
            }
            catch (System.IO.IOException)
            {
                try
                {
                    Console.OutputEncoding = Encoding.Unicode;
                }
                catch (System.IO.IOException)
                {
                    try
                    {
                        Console.OutputEncoding = Encoding.UTF8;
                    }
                    catch (System.IO.IOException)
                    {
                        Console.OutputEncoding = Encoding.ASCII;
                        Titletext = @"
 __       __             ______    ______   __                                         
|  \     /  \           /      \  /      \ |  \                                        
| $$\   /  $$ __    __ |  $$$$$$\|  $$$$$$\ \$$ _______    ______    ______   __    __ 
| $$$\ /  $$$|  \  |  \| $$_  \$$| $$_  \$$|  \|       \  /      \  /      \ |  \  |  \
| $$$$\  $$$$| $$  | $$| $$ \    | $$ \    | $$| $$$$$$$\|  $$$$$$\|  $$$$$$\| $$  | $$
| $$\$$ $$ $$| $$  | $$| $$$$    | $$$$    | $$| $$  | $$| $$    $$| $$   \$$| $$  | $$
| $$ \$$$| $$| $$__/ $$| $$      | $$      | $$| $$  | $$| $$$$$$$$| $$      | $$__/ $$
| $$  \$ | $$ \$$    $$| $$      | $$      | $$| $$  | $$ \$$     \| $$       \$$    $$
 \$$      \$$  \$$$$$$  \$$       \$$       \$$ \$$   \$$  \$$$$$$$ \$$       _\$$$$$$$
                                                                             |  \__| $$
                                                                              \$$    $$
                                                                               \$$$$$$ ";
                    }
                }
            }
            Console.Title = "   Muffinry - " + version;
            Console.SetWindowSize(120, 35);
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;
            Console.CursorVisible = false;
            Console.BackgroundColor = Theme[0];
            Console.ForegroundColor = Theme[1];
        }


        private static void Title()
        {
            Console.ForegroundColor = (Math.Floor((decimal)(DateTime.Now - Process.GetCurrentProcess().StartTime).Milliseconds / 250) % 4) switch
            {
                0 => ConsoleColor.Cyan,
                1 => ConsoleColor.Green,
                2 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red,
            };
            Console.WriteLine(Titletext);

            Console.ForegroundColor = Theme[1];
            Console.SetCursorPosition(85,2);
            Console.Write("(c) TM");
            Console.SetCursorPosition(0,12);
            Console.WriteLine(@"                                ~A(n arguably) decent game~ 
                         kudos to Matthew Pacitto for inspiration
                                 - Press Enter to Load -

               P.S. please use a monospace font or ur mean and a bad person");
        }

        private static void Menu()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Playing for: {0} minutes", Math.Floor((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMinutes));
            Console.WriteLine("\n\nMUFFINS: {0}\n", muffins.ToString());
            Console.WriteLine("You Can:");
            for (byte i = 0; i < menuOptions.Length; i++)
            {
                if (i == selected) { Console.ForegroundColor = Theme[2]; } else { Console.ForegroundColor = Theme[1]; }
                Console.WriteLine(menuOptions[i]);
            }
            Console.ForegroundColor = Theme[1];
            Console.WriteLine("\n\n");

            for (byte i = 0; i < Factories.Count; i++)
            {
                switch ((int)Factories[i].Amount)
                {
                    case 0:
                        break;
                    case 1:
                        Console.Write("You have ");
                        Console.ForegroundColor = Theme[2];
                        Console.Write("1 ");
                        Console.ForegroundColor = Theme[1];
                        Console.Write("Level ");
                        Console.ForegroundColor = Theme[2];
                        Console.Write("{0} ", Factories[i].Level);
                        Console.ForegroundColor = Theme[1];
                        Console.WriteLine("factory.");
                        break;
                    default:
                        Console.Write("You have ");
                        Console.ForegroundColor = Theme[2];
                        Console.Write("{0} ", Factories[i].Amount);
                        Console.ForegroundColor = Theme[1];
                        Console.Write("Level ");
                        Console.ForegroundColor = Theme[2];
                        Console.Write("{0} ", Factories[i].Level);
                        Console.ForegroundColor = Theme[1];
                        Console.WriteLine("factories.");
                        break;
                }
            }

            Console.WriteLine("\nPress ESC to return to the title screen");
        }
        private static void Upgrade()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - Playing for: {0} minutes", Math.Floor((DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMinutes));
            Console.WriteLine("\n\nMUFFINS: {0}\n", muffins.ToString());
            for (byte i = 0; i < UpgradeableFactories.Count; i++)
            {
                if (i == selected) { Console.ForegroundColor = Theme[2]; } else { Console.ForegroundColor = Theme[1]; }
                Console.WriteLine(" - Upgrade one Level {0} factory to a Level {1} (Cost: {2})", UpgradeableFactories[i].Level, UpgradeableFactories[i].Level + 1, Program.Factories[Program.Factories.IndexOf(UpgradeableFactories[i]) + 1].Cost);
            }
            Console.ForegroundColor = Theme[1];
            Console.WriteLine("Press ESC to leave this submenu.\n");
        }
    }

    class FactoryType
    {
        public BigInteger Level { get; }
        public BigInteger Amount { get; private set; }
        public BigInteger Cost { get; private set; }
        public FactoryType(BigInteger level, ulong amount)
        {
            this.Level = level;
            this.Amount = amount;
            this.Cost = (BigInteger)Math.Pow((double)(Level + 1), 4) * 3 + 200;
        }

        public bool Upgrade()
        {
            /*if (Program.Factories.IndexOf(this) == Program.Factories.Count - 1) {
                Program.Factories.Add(new FactoryType(Level+1,0));
            }*/
            FactoryType nextFactory = Program.Factories[Program.Factories.IndexOf(this) + 1];
            if (Program.muffins < (ulong)nextFactory.Cost)
            {
                return false;
            }
            Program.muffins -= (ulong)nextFactory.Cost;

            Amount--;
            nextFactory.Add();
            return true;
        }

        public void Add() => Amount++;
        public bool Buy()
        {
            if (Program.muffins < (ulong)Cost)
            {
                return false;
            }
            this.Add();
            Program.muffins -= (ulong)Cost;
            return true;
        }

        public void Muffins()
        {
            ulong inc = (ulong)Math.Floor(Math.Pow(5, (double)Level) * 10 / 3);
            for (ulong i = 0; i < inc * Amount; i++)
            {
                Program.muffins++;
                Thread.Sleep(2);
            }
        }
    }
}
