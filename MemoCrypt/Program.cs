namespace MemoCrypt
{
    abstract class Program
    {
        static void Main(string[] args)
        {
            RunTest();
        }

        private static void RunTest()
        {
            var consoleColor = Console.ForegroundColor;

            const string outputHeadline = "Polybius Cipher Test Suite";
            const string keyword = "PROGRAMMIEREN";
            const string memoText = "HELLO WORLD";

            var cipher = new PolybiusCipher(keyword);

            string encryptedText = cipher.Encrypt(memoText);
            string decryptedText = cipher.Decrypt(encryptedText);
            bool testOk = (decryptedText == memoText);
            var testResultColor = testOk ? ConsoleColor.Green : ConsoleColor.Red;

            Console.WriteLine($"{outputHeadline}\n{new string('=', outputHeadline.Length)}\n");
            Console.WriteLine($"--- ORIGINAL MEMO TEXT ---\n\n{memoText}\n");
            Console.WriteLine($"--- ENCRYPTED ---\n\n{encryptedText}\n");
            Console.WriteLine($"--- DECRYPTED ---\n\n{decryptedText}\n");

            Console.WriteLine("--- RESULT ---\n");
            Console.ForegroundColor = testResultColor;
            Console.WriteLine(testOk ? "OK" : "FAIL");
            Console.ForegroundColor = consoleColor;
        }
    }
}