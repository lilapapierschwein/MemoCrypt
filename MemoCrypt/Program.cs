namespace MemoCrypt
{
    internal abstract class Program
    {
        private static readonly PolybiusCipher Cipher = new PolybiusCipher();

        public static void Main(string[] args)
        {
                SetConsoleTitle(Utils.Constants.Prog);
                if (args.Length > 0)
                {
                    try
                    {
                        RunCli(args);
                    }
                    catch (FormatException exc)
                    {
                        Console.WriteLine($"Error: {exc.Message}");
                    }
                    catch (ArgumentException exc)
                    {
                        Console.WriteLine($"Error: {exc.Message}");
                    }
                    return;
                }
                Utils.ShowVersion();
                Console.WriteLine("\n\nTo run the app interactive mode, run with '-i'.");
            }

            static void RunInteractive()
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
                    Cipher.SetStrict(true);
                    Utils.SetConsoleTitle("(Strict Mode)", true);
                }

                Console.Write("Insert the encryption key: ");
                Cipher.SetKeyWord((Console.ReadLine() ?? string.Empty).Trim());
                Cipher.UpdateKey(Cipher.GetKeyword());

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

                        Console.WriteLine($"Encrypted text:\n\n{Cipher.Encrypt(memoText)}");
                        break;
                    case 2:
                        Utils.SetConsoleTitle("| Decrypt a ciphertext", true);

                        Console.WriteLine("Insert ciphertext:");
                        var cipherText = (Console.ReadLine() ?? string.Empty).Trim();
                        Console.Clear();

                        Console.WriteLine($"Decrypted memo:\n\n{Cipher.Decrypt(cipherText)}");
                        break;
                }
            }

            static void RunCli(string[] args)
            {
                var parser = new Utils.CliParser(args);
                var parsedArgs = parser.ParsedArgs;
                
                if (!string.IsNullOrEmpty(parsedArgs.FilePath))
                {
                    var filePath = Path.GetFullPath(parsedArgs.FilePath);
                    try
                    {
                        parsedArgs.Text = Utils.ReadFileContent(filePath).Trim();
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e);
                    }
                }
                // TODO: implement output to file
                // if (!string.IsNullOrEmpty(parsedArgs.OutFilePath))
                // {
                // }
                
                if (parsedArgs.ShowHelpText)
                {
                    Utils.ShowHelp();
                    return;
                }

                if (parsedArgs.ShowVersionText)
                {
                    Utils.ShowVersion();
                    return;
                }

                if (parsedArgs.RunInteractive)
                {
                    RunInteractive();
                    return;
                }

                if (parsedArgs.RunTest)
                {
                    RunTest();
                    return;
                }
                
                Cipher.UpdateKey(parsedArgs.Key);
                Cipher.SetStrict(parsedArgs.Strict);

                string result = (parsedArgs.Action == Utils.CliParser.Action.Encrypt)
                    ? Cipher.Encrypt(parsedArgs.Text)
                    : Cipher.Decrypt(parsedArgs.Text);
                Console.WriteLine(result);
            }

            static void RunTest(string keyword = "PROGRAMMIEREN", string memoText = "HELLO WORLD",
                bool strictMode = false)
            {
                Console.Title = $"{Utils.Constants.Prog} | Test Suite";
                Console.WriteLine(Utils.CenterText(" Running Test... ", fillchar: '=', useTermWidth: true) + "\n");

                bool testOk = false;

                Cipher.SetKeyWord(keyword);
                Cipher.SetStrict(strictMode);

                Console.Title = "Polybius Cipher Test Suite";

                var consoleColor = Console.ForegroundColor;

                var errorCode = 0;

                try
                {
                    string encryptedText = Cipher.Encrypt(memoText);
                    string decryptedText = Cipher.Decrypt(encryptedText);

                    testOk = (decryptedText == memoText);
                    string resultText = (testOk ? "PASSED ✅" : "FAILED ❌");

                    Console.WriteLine($"ORIGINAL TEXT:   {memoText}");
                    Console.WriteLine($"ENCRYPTED TEXT:  {encryptedText}");
                    Console.WriteLine($"DECRYPTED TEXT:  {decryptedText}");
                    Console.WriteLine($"RESULT:          {resultText}");
                }
                catch (FormatException exc)
                {
                    Console.WriteLine($"Error: {exc.Message}");
                    errorCode = 1;
                }
                catch (ArgumentException exc)
                {
                    Console.WriteLine($"Error: {exc.Message}");
                    errorCode = 1;
                }
                finally
                {
                    string finalMsg = (errorCode != 0) ? "Testrun finished with errors." : "Testrun finished.";
                    Console.ForegroundColor = (errorCode != 0) ? ConsoleColor.Red :
                        testOk ? ConsoleColor.Green : ConsoleColor.Yellow;
                    Console.WriteLine("\n" + Utils.CenterText($" {finalMsg} ", fillchar: '=', useTermWidth: true));
                    Console.ForegroundColor = consoleColor;
                }
            }

            static void SetConsoleTitle(string title)
            {
                Console.Title = title;
            }
        }
    }
