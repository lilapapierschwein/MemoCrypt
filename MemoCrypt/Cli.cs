namespace MemoCrypt;

public class Cli
{
    private static OutputStream Output { get; set; } = OutputStream.Stdout;

    public (TargetAction, PolybiusCipher) RunCli(string[] args, PolybiusCipher cipher)
    {
        var parser = new Parser(args);
        var parsedArgs = parser.ParsedArgs;
        
        if (parsedArgs.ShowHelpText)
        {
            return (TargetAction.ShowHelp, cipher);
        }

        if (parsedArgs.ShowVersionText)
        {
            return (TargetAction.ShowVersion, cipher);
        }

        if (parsedArgs.RunInteractive)
        {
            return (TargetAction.RunInteractive, cipher);
        }

        if (parsedArgs.RunTest)
        {
            return (TargetAction.RunTest, cipher);
        }

        if (!string.IsNullOrEmpty(parsedArgs.Key) && parsedArgs.Key.EndsWith(".txt"))
        {
            try
            {
                var keyFromFile = Utils.ReadFileContent(parsedArgs.Key).Trim();
                cipher.UpdateKey(keyFromFile);
            }
            catch (FileNotFoundException exc)
            {
                Console.WriteLine($"Error: {exc.Message} ({exc.FileName}). Key could not be found.");
                Environment.Exit(1);
            }
        }
        else
        {
            cipher.UpdateKey(parsedArgs.Key);
        }

        cipher.SetStrict(parsedArgs.Strict);
        string result = (parsedArgs.Action == Parser.Action.Encrypt)
            ? cipher.Encrypt(parsedArgs.Text)
            : cipher.Decrypt(parsedArgs.Text);
        
        switch (Output)
        {
            case OutputStream.Stdout or OutputStream.Stderr:
                Console.WriteLine($"\r{result.Trim()}");
                break;
            case OutputStream.FileStream:
                Utils.WriteToFile(parsedArgs.OutFilePath, result, validate:false);
                Console.WriteLine($"\rSaved to file: '{Path.GetFullPath(parsedArgs.OutFilePath)}'");
                break;
        }
        return (TargetAction.None, cipher);
    }

    public class Parser
    {
        public string[] OriginalArgs;
        public Arguments ParsedArgs { get; set; } = new Arguments();

        public Parser(string[] args)
        {
            OriginalArgs = args;
            Parse();
        }

        private void Parse(string[]? args = null) 
        {
            if (args != null)
            {
                OriginalArgs = args;
            }
            
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

            if (Flags.Markers.OperationFlags.Any(item => OriginalArgs.Contains(item)))
            {
                foreach (string originalArg in OriginalArgs)
                {
                    if (Flags.Markers.OperationFlags.Contains(originalArg))
                    {
                        ParsedArgs.Action = (originalArg == "-d" || originalArg.EndsWith("decrypt"))
                            ? Action.Decrypt
                            : Action.Encrypt;
                        break;
                    }
                }
            }
            
            ParsedArgs.Strict = Flags.Markers.StrictModeFlags.Any(item => OriginalArgs.Contains(item));

            bool textFromFile = false;
            try
            {
                for (int i = 0; i < OriginalArgs.Length; i++)
                {
                    if (Flags.Markers.KeyFlags.Contains(OriginalArgs[i]))
                    {
                        ParsedArgs.Key = (OriginalArgs[i + 1].EndsWith(".txt")) 
                            ? Utils.ReadFileContent(OriginalArgs[i + 1]).Trim() 
                            : OriginalArgs[i + 1];
                        i += 1;
                        continue;
                    }

                    if (Flags.Markers.FileFlags.Contains(OriginalArgs[i]))
                    {
                        Utils.ValidateFilePath(OriginalArgs[i + 1]);
                        ParsedArgs.FilePath = OriginalArgs[i + 1];
                        ParsedArgs.Text = Utils.ReadFileContent(ParsedArgs.FilePath).Trim();
                        textFromFile = true;
                        i += 1;
                        continue;
                    }

                    if (Flags.Markers.OutputFlags.Contains(OriginalArgs[i]))
                    {
                        try
                        {
                            var outPath = OriginalArgs[i + 1];
                            var outputDir = Path.GetDirectoryName(Path.GetFullPath(outPath));
                            Utils.ValidateFilePath(outputDir, true);
                            ParsedArgs.OutFilePath = OriginalArgs[i + 1];
                            Output = OutputStream.FileStream;
                        }
                        catch (DirectoryNotFoundException exc)
                        {
                            Console.WriteLine($"Error: {exc.Message}. Outputfile cannot be created.");
                            Environment.Exit(1);
                            i += 1;
                            continue;
                        }
                    }
                }

                if (!textFromFile)
                {
                    ParsedArgs.Text = OriginalArgs[^1];
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Unable to parse malformed command line arguments.");
            }
            catch (FileNotFoundException exc)
            {
                throw new FileNotFoundException(exc.Message, exc);
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
                public static readonly string[] FileFlags = ["-f", "--file", "/f", "/file"];
                public static readonly string[] OutputFlags = ["-o", "--output", "/o", "/output"];

                public static readonly string[] OperationFlags =
                    ["-d", "--decrypt", "/d", "/decrypt", "-e", "--encrypt", "/e", "/encrypt"];
            }
        }
    }

    private enum OutputStream
    {
        Stdout,
        Stderr,
        FileStream
    }

    public enum TargetAction
    {
        None = 0,
        ShowHelp,
        ShowVersion,
        RunInteractive,
        RunTest,
    }
}