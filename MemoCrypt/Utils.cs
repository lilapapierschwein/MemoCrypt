using System.Diagnostics.CodeAnalysis;

namespace MemoCrypt;

public abstract class Utils
{
    /// <summary>
    /// A collection of contant or 'magic' values.
    /// </summary>
    public readonly struct Constants
    {
        public static readonly string Prog = "MemoCrypt";
        public static readonly string Version = "0.1.0";

        public static readonly string HelpText = $"{Prog} [HELPTEXT MISSING (TODO)]";
    }
    public static void ShowVersion()
    {
        Console.Write($"{Constants.Prog} {Constants.Version}");
    }

    public static void ShowHelp()
    {
        Console.Write(Constants.HelpText);
    }

    /// <summary>
    /// Center the given text. If the lenght exceeds the target width, the original string is returned,
    /// </summary>
    /// <param name="text">Text to center</param>
    /// <param name="width">Target width (default: 80)</param>
    /// <param name="fillchar">Character to fill space (default: ' ')</param>
    /// <param name="useTermWidth">Determine target width by terminal colums count.</param>
    /// <returns>The proccessed string.</returns>
    public static string CenterText(string text, int width = 80, char fillchar = ' ', bool useTermWidth = false)
    {
        int windowWidth = Console.WindowWidth;
        // set the target width according to parameters
        // in case `useTermWidth` is false but `width` is greater than the window width, resort to window width
        int targetWidth = useTermWidth ? windowWidth : (width > windowWidth) ? Console.WindowWidth : width;
        
        int textLength = text.Length;
        if (textLength > targetWidth)
        {
            return text;
        }
        
        bool isAsymetrical = ((targetWidth - textLength) % 2) != 0;
        int leftFillerWidth = (targetWidth - textLength) / 2;
        int rightFillerWidth = isAsymetrical ? leftFillerWidth -1 : leftFillerWidth;

        string rightFiller = new string(fillchar,rightFillerWidth);
        string leftFiller = new string(fillchar,leftFillerWidth);

        return $"{leftFiller}{text}{rightFiller}";
    }

    /// <summary>
    /// Set or extend the terminal title, if supported.
    /// </summary>
    /// <param name="title">Text to insert / append.</param>
    /// <param name="extend">If true, the text appended to orginal title, if any. (default: false).</param>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
    public static bool SetConsoleTitle(string title, bool extend = false)
    {
        try
        {
            title = extend ? $"{Console.Title}\u0020{title}" : title;
            Console.Title = title;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
        return true;
    }

    public class CliParser
    {
        private string[] OriginalArgs { get; set; }
        public Arguments ParsedArgs { get; set; } = new Arguments();

        public CliParser(string[] args)
        {
            OriginalArgs = args;
            Parse();
        }

        private void Parse()
        {
            if (Flags.Markers.HelpFlags.Any(item => OriginalArgs.Contains(item)))
            {
                ParsedArgs.ShowHelpText = true;
                return;
            }
            if (Flags.Markers.VersionFlags.Any(item => OriginalArgs.Contains(item)))
            {
                ParsedArgs.ShowVersionText = true;
                return;
            }

            if (Flags.Markers.InteractiveFlags.Any(item => OriginalArgs.Contains(item)))
            {
                ParsedArgs.RunInteractive = true;
                return;
            }
            if (Flags.Markers.TestRunFlags.Any(item => OriginalArgs.Contains(item)))
            {
                ParsedArgs.RunTest = true;
                return;
            }

            ParsedArgs.Strict = Flags.Markers.StrictModeFlags.Any(item => OriginalArgs.Contains(item));
            try
            {
                for (int i = 0; i < OriginalArgs.Length; i++)
                {
                    if (Flags.Markers.KeyFlags.Contains(OriginalArgs[i]))
                    {
                        ParsedArgs.Key = OriginalArgs[i + 1];
                        i += 1;
                        continue;
                    }

                    if (Flags.Markers.FileFlags.Contains(OriginalArgs[i]))
                    {
                        ParsedArgs.FilePath = OriginalArgs[i + 1];
                        i += 1;
                        continue;
                    }

                    if (Flags.Markers.OutputFlags.Contains(OriginalArgs[i]))
                    {
                        ParsedArgs.OutFilePath = OriginalArgs[i + 1];
                        i += 1;
                        continue;
                    }

                    if (OriginalArgs[i] == "encrypt" || OriginalArgs[i] == "decrypt")
                    {
                        ParsedArgs.Action = (OriginalArgs[i] == "encrypt") ? Action.Encrypt : Action.Decrypt;
                        continue;
                    }

                    if (i != OriginalArgs.Length - 1) continue;
                    ParsedArgs.Text = OriginalArgs[i];
                }
            }
            catch (IndexOutOfRangeException) {
                throw new IndexOutOfRangeException("Unable to parse malformed command line arguments.");
            }
        }

        public class Arguments
        {
            public bool ShowHelpText { get; set; }
            public bool ShowVersionText { get; set; }
            public bool RunInteractive { get; set; }
            public bool RunTest { get; set; }
            public bool Strict { get; set; }
            public string FilePath { get; set; } = "";
            // TODO
            public string OutFilePath { get; set; } = "";
            public string Key { get; set; } = "";
            public Action Action { get; set; } = Action.Encrypt;
            public string Text { get; set; } = string.Empty;
        }
        
        public enum Action
        {
            Encrypt,
            Decrypt
        }
        
        private struct Flags
        {
            public struct Markers
            {
                public static readonly string[] HelpFlags = ["-h", "--help", "/h", "/help", "/?"];
                public static readonly string[] VersionFlags = ["-V", "--version", "/V", "/version"];
                public static readonly string[] InteractiveFlags = ["-i", "--interactive", "/i", "/interactive"];
                public static readonly string[] TestRunFlags = ["-t", "--test", "/t", "/test"];
                public static readonly string[] StrictModeFlags = ["-s", "--strict", "/s", "/strict"];
                public static readonly string[] KeyFlags = ["-k", "--key", "/k", "/key"];
                public static readonly string[] FileFlags =  ["-f", "--file", "/f", "/file"];
                public static readonly string[] OutputFlags =  ["-o", "--file", "/o", "/output"];
            }
        }
    }

    /// <summary>
    /// Validate the given filepath.
    /// </summary>
    /// <param name="path">The filepath to validate.</param>
    /// <exception cref="FileNotFoundException">If file does not exist.</exception>
    public static void ValidateFilePath(string? path, bool isDirectory = false)
    {
        if (isDirectory)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist.");
            }
            return;
        }
        
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File file not found.", path);
        }
    }

    public static string ReadFileContent(string path)
    {
        ValidateFilePath(path);
        return File.ReadAllText(path);
    }
    
    public static void WriteToFile(string path, string content, bool append = false, bool validate = true)
    {
        if (validate)
        {
            ValidateFilePath(path);
        }

        if (append)
        {
            File.AppendAllText(path, content);
            return;
        }
        File.WriteAllText(path, content);
    }
}

