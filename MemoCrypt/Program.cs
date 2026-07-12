namespace MemoCrypt
{
    internal abstract class Program
    {
        private static PolybiusCipher _cipher = new PolybiusCipher();
        
        public static void Main(string[] args)
        {
            Utils.SetConsoleTitle(Utils.Constants.Prog);
            
            if (args.Length == 0)
            {
                Utils.ShowHelp();
                return;
            }
            
            try
            {
                (var action, _cipher) = Cli.RunCli(args, _cipher);
                
                switch (action)
                {
                    case Cli.TargetAction.ShowHelp:
                        Utils.ShowHelp();
                        break;
                    case Cli.TargetAction.ShowFullHelp:
                        Utils.ShowHelp(license:true);
                        break;
                    case Cli.TargetAction.ShowVersion:
                        Utils.ShowVersion();
                        break;
                    case Cli.TargetAction.RunInteractive:
                        RunInteractive();
                        break;
                    case Cli.TargetAction.RunTest:
                        var tests = new Tests();
                        tests.RunTests();
                        break;
                    case Cli.TargetAction.None:
                        break;
                    default:
                        Utils.ShowHelp();
                        break;
                }
            }
            catch (FormatException exc)
            {
                Console.Write($"Error: {exc.Message}");
            }
            catch (ArgumentException exc)
            {
                Console.Write($"Error: {exc.Message}");
            }
            catch (IndexOutOfRangeException exc)
            {
                Console.Write($"Error: {exc.Message}");
            }
        }

        private static void RunInteractive()
            {
                Utils.SetConsoleTitle("| Interactive", true);
                var option = 0;

                // set mode
                Console.Write("Run the app in strict mode? [y/N]: ");
                var strictModeSetting = Console.ReadLine() ?? string.Empty;
                var strictMode = strictModeSetting.Trim().ToLower() == "y";
                Console.Clear();

                if (strictMode)
                {
                    _cipher.SetStrict(true);
                    Utils.SetConsoleTitle("(Strict Mode)", true);
                }

                Console.Write("Insert the encryption key: ");
                _cipher.SetKeyWord((Console.ReadLine() ?? string.Empty).Trim());
                _cipher.UpdateKey(_cipher.GetKeyword());

                while (true)
                {
                    Console.Clear();
                    Console.Write(
                        "What do you want to do?\n" +
                        " [1] Encrypt a memo\n" +
                        " [2] Decrypt a ciphertext\n" +
                        "Choose [1/2]: "
                    );

                    var userInput = (Console.ReadLine() ?? string.Empty).Trim();
                    if (!int.TryParse(userInput, out var userOption))
                    {
                        Console.WriteLine(
                            "\n" + (userInput == ""
                                ? $"Error: Empty input!"
                                : $"Error '{userInput}' is not a number!"));
                        Console.Write("Press any key to continue or 'Q' to quit...");
                        ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            Environment.Exit(0);
                        }

                        continue;
                    }

                    if (!(userOption == 1 || userOption == 2))
                    {
                        Console.WriteLine($"Error: Invalid option ({option})");
                        Console.Write("Press any key to continue...");
                        Console.ReadKey(intercept: true);
                        continue;
                    }

                    option = userOption;
                    break;
                }

                Console.Clear();

                switch (option)
                {
                    case 1:
                        Utils.SetConsoleTitle("| Encrypt a memo", true);

                        Console.WriteLine("Insert memo text:");
                        var memoText = (Console.ReadLine() ?? string.Empty).Trim();
                        Console.Clear();

                        Console.WriteLine($"Encrypted text:\n\n{_cipher.Encrypt(memoText)}");
                        break;
                    case 2:
                        Utils.SetConsoleTitle("| Decrypt a ciphertext", true);

                        Console.WriteLine("Insert ciphertext:");
                        var cipherText = (Console.ReadLine() ?? string.Empty).Trim();
                        Console.Clear();

                        Console.WriteLine($"Decrypted memo:\n\n{_cipher.Decrypt(cipherText)}");
                        break;
                }
            }
        }
    }
