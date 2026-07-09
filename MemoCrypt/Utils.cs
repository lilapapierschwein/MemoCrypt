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

        public static readonly string HelpText = $"{Prog} [HELP TEXT HERE]";
        
        public struct CliFlags
        {
            public struct Flags
            {
                public static readonly string[] HelpFlags = ["-h", "--help", "/h", "/help", "/?"];
                public static readonly string[] VersionFlags = ["-V", "--version", "/V", "/version"];
                public static readonly string[] InteractiveFlags = ["-i", "--interactive", "/i", "/interactive"];
                public static readonly string[] TestRunFlags = ["-t", "--test", "/t", "/test"];
                public static readonly string[] StrictModeFlags = ["-s", "--strict", "/s", "/strict"];
                public static readonly string[] KeyFlags = ["-k", "--key", "/k", "/key"];
            }

            public struct Options
            {
                public static readonly string[] ActionOptions = ["-k", "--key", "/k", "/key"];
            }
        }
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

    public enum CliAction
    {
        Encrypt = 0,
        Decrypt = 1,
    }

    public class CliArguments
    {
        public bool ShowHelpText { get; set; }
        public bool ShowVersionText { get; set; }
        public bool RunInteractive { get; set; }
        public bool RunTest { get; set; }
        public bool Strict { get; set; }
        public string Key { get; set; } = "";
        public CliAction Action { get; set; } = CliAction.Encrypt;
        public string Text { get; set; } = string.Empty;
    }
    
    public static CliArguments ParseCliOptions(string[] args)
    {
        var parsedArgs = new CliArguments();
        
        if (Constants.CliFlags.Flags.HelpFlags.Any(item => args.Contains(item)))
        {
            parsedArgs.ShowHelpText = true;
            return parsedArgs;
        }
        if (Constants.CliFlags.Flags.VersionFlags.Any(item => args.Contains(item)))
        {
            parsedArgs.ShowVersionText = true;
            return parsedArgs;
        }
        if (Constants.CliFlags.Flags.InteractiveFlags.Any(item => args.Contains(item)))
        {
            parsedArgs.RunInteractive = true;
            return parsedArgs;
        }
        if (Constants.CliFlags.Flags.TestRunFlags.Any(item => args.Contains(item)))
        {
            parsedArgs.RunTest = true;
            return parsedArgs;
        }
        
        parsedArgs.Strict = Constants.CliFlags.Flags.StrictModeFlags.Any(item => args.Contains(item));
        for (int i=0; i<args.Length; i++)
        {
            if (Constants.CliFlags.Flags.KeyFlags.Contains(args[i]))
            {
                parsedArgs.Key = args[i+1];
                i += 1;
                continue;
            }
            if (args[i] == "encrypt" || args[i] == "decrypt") {
                parsedArgs.Action = (args[i] == "encrypt") ? CliAction.Encrypt : CliAction.Decrypt;
                continue;
            }

            if (i != args.Length - 1) continue;
            parsedArgs.Text = args[i];
            return  parsedArgs;
        }
        return parsedArgs;
    }
}
