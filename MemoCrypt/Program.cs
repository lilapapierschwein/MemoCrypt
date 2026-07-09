namespace MemoCrypt
{
    abstract class Program
    {
        private static PolybiusCipher _cipher = new PolybiusCipher();

        static void Main()
        {
            // RunTest();
            RunInteractive();
        }

        static void RunInteractive()
        {
            var consoleTitle = "MemoCrypt Interactive";
            var option = 0;
            Console.Title = consoleTitle;

            // set mode
            Console.Write("Run the app in strict mode? [y/N]: ");
            var strictModeSetting = Console.ReadLine() ?? string.Empty;
            var strictMode = strictModeSetting.Trim().ToLower() == "y";
            Console.Clear();

            if (strictMode)
            {
                consoleTitle += " (Strict Mode)";
                Console.Title = consoleTitle;
            }

            Console.Write("Insert the encryption key: ");
            _cipher.SetKeyWord((Console.ReadLine() ?? string.Empty).Trim());

            while (true)
            {
                Console.Clear();
                Console.Write(
                    "What do you want to do?\n" +
                    " [1] Encrypt a memo\n" +
                    " [2] Decrypt a ciphertext\n" +
                    "Choose [1/2]: "
                );

                var userInput = Console.ReadLine() ?? "0";
                if (!int.TryParse(userInput, out var userOption))
                {
                    Console.WriteLine(
                        userInput == "" ? $"Error: Empty input!" : $"Error '{userInput}' is not a number!");
                    Console.Write("Press any key to continue...");
                    Console.ReadLine();
                    continue;
                }

                if (userOption == 1 || userOption == 2)
                {
                    option = userOption;
                    break;
                }

                Console.WriteLine($"Error: Invalid option ({option})");
                Console.Write("Press any key to continue...");
                Console.ReadLine();
            }
            
            Console.Clear();
            
            switch (option)
            {
                case 1:
                    consoleTitle += " | Encrypt a memo";
                    Console.Title = consoleTitle;

                    Console.WriteLine("Insert memo text:");
                    var memoText = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Clear();

                    Console.WriteLine($"Encrypted text:\n\n{_cipher.Encrypt(memoText)}");
                    break;
                case 2:
                    consoleTitle += " | Decrypt a ciphertext";
                    Console.Title = consoleTitle;

                    Console.WriteLine("Insert ciphertext:");
                    var cipherText = (Console.ReadLine() ?? string.Empty).Trim();
                    Console.Clear();

                    Console.WriteLine($"Decrypted memo:\n\n{_cipher.Decrypt(cipherText)}");
                    break;
            }
        }

        private static void RunTest(string keyword = "PROGRAMMIEREN", string memoText = "HELLO WORLD",
            bool strictMode = false)
        {
            _cipher.SetKeyWord(keyword);
            _cipher.SetStrict(strictMode);

            Console.Title = "Polybius Cipher Test Suite";

            var consoleColor = Console.ForegroundColor;

            var testResultColor = ConsoleColor.Red;
            var errorCode = 0;

            Console.WriteLine($"--- ORIGINAL MEMO TEXT ---\n\n{memoText}\n");

            try
            {
                string encryptedText = _cipher.Encrypt(memoText);
                string decryptedText = _cipher.Decrypt(encryptedText);

                var testOk = (decryptedText == memoText);
                testResultColor = testOk ? ConsoleColor.Green : ConsoleColor.Red;

                Console.WriteLine($"--- ENCRYPTED ---\n\n{encryptedText}\n");
                Console.WriteLine($"--- DECRYPTED ---\n\n{decryptedText}\n");

                Console.WriteLine("--- RESULT ---\n");
                Console.ForegroundColor = testResultColor;
                Console.WriteLine(testOk ? "OK" : "FAIL");
                Console.ForegroundColor = consoleColor;
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
                if (errorCode != 0)
                {
                    Console.ForegroundColor = testResultColor;
                    Console.WriteLine("Test failed.");
                    Console.ForegroundColor = consoleColor;
                }
            }
        }
    }
}