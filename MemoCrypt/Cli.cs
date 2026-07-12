namespace MemoCrypt;

public class Cli
{
    private static OutputStream Output { get; set; } = OutputStream.Stdout;

    public static (TargetAction, PolybiusCipher) RunCli(string[] args, PolybiusCipher cipher)
    {
        var parser = new Parser(args);
        var parsedArgs = parser.ParsedArgs;
        
        if (parsedArgs.ShowHelpText)
        {
            var action = (parsedArgs.ShowHelpTextFull) ? TargetAction.ShowFullHelp : TargetAction.ShowHelp;
            return (action, cipher);
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

        if (!string.IsNullOrEmpty(parsedArgs.Key) && Validators.KeyIsFile(parsedArgs.Key))
        {
            try
            {
                var keyFromFile = Utils.ReadFileContent(parsedArgs.Key);
                cipher.UpdateKey(keyFromFile);
            }
            catch (FileNotFoundException exc)
            {
                Console.Write($"Error: {exc.Message} ({exc.FileName}). Key could not be found.");
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
                Console.Write(result.Trim());
                break;
            case OutputStream.FileStream:
                Utils.WriteToFile(parsedArgs.OutFilePath, result, validate:false);
                Console.WriteLine($"Saved to file: '{Path.GetFullPath(parsedArgs.OutFilePath)}'");
                break;
        }
        return (TargetAction.None, cipher);
    }

    public class Parser
    {
        private string[] _originalArgs;
        public Arguments ParsedArgs { get; } = new Arguments();

        public Parser(string[] args)
        {
            _originalArgs = args;
            Parse();
        }

        private void Parse(string[]? args = null) 
        {
            if (args != null)
            {
                _originalArgs = args;
            }
            
            if (Flags.Markers.HelpFlags.Any(item => _originalArgs.Contains(item)))
            {
                ParsedArgs.ShowHelpText = true;
                foreach (var originalArg in _originalArgs)
                {
                    if (Flags.Markers.HelpFlags.Contains(originalArg))
                    {
                        ParsedArgs.ShowHelpTextFull = (originalArg == "--help" || originalArg == "\\help");
                    }
                }
                return;
            }

            if (Flags.Markers.VersionFlags.Any(item => _originalArgs.Contains(item)))
            {
                ParsedArgs.ShowVersionText = true;
                return;
            }

            if (Flags.Markers.InteractiveFlags.Any(item => _originalArgs.Contains(item)))
            {
                ParsedArgs.RunInteractive = true;
                return;
            }

            if (Flags.Markers.TestRunFlags.Any(item => _originalArgs.Contains(item)))
            {
                ParsedArgs.RunTest = true;
                return;
            }

            if (Flags.Markers.OperationFlags.Any(item => _originalArgs.Contains(item)))
            {
                foreach (string originalArg in _originalArgs)
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
            
            ParsedArgs.Strict = Flags.Markers.StrictModeFlags.Any(item => _originalArgs.Contains(item));
            
            bool textFromFile = false;
            try
            {
                for (int i = 0; i < _originalArgs.Length; i++)
                {
                    if (Flags.Markers.KeyFlags.Contains(_originalArgs[i]) || _originalArgs[i].EndsWith(Utils.Constants.KeyFiletype))
                    {
                        if (_originalArgs[i].EndsWith(Utils.Constants.KeyFiletype))
                        {
                            ParsedArgs.Key = Utils.ReadFileContent(_originalArgs[i]);
                            continue;
                        }
                        
                        ParsedArgs.Key = (Validators.KeyIsFile(_originalArgs[i + 1])) 
                            ? Utils.ReadFileContent(_originalArgs[i + 1]).Trim() 
                            : _originalArgs[i + 1];
                        i += 1;
                        continue;
                    }
                    if (Flags.Markers.FileFlags.Contains(_originalArgs[i]))
                    {
                        Utils.ValidateFilePath(_originalArgs[i + 1]);
                        ParsedArgs.FilePath = _originalArgs[i + 1];
                        ParsedArgs.Text = Utils.ReadFileContent(ParsedArgs.FilePath).Trim();
                        textFromFile = true;
                        i += 1;
                        continue;
                    }

                    if (Flags.Markers.OutputFlags.Contains(_originalArgs[i]))
                    {
                        try
                        {
                            var outPath = _originalArgs[i + 1];
                            var outputDir = Path.GetDirectoryName(Path.GetFullPath(outPath));
                            Utils.ValidateFilePath(outputDir, true);
                            ParsedArgs.OutFilePath = _originalArgs[i + 1];
                            Output = OutputStream.FileStream;
                        }
                        catch (DirectoryNotFoundException exc)
                        {
                            Console.WriteLine($"Error: {exc.Message}. Outputfile cannot be created.");
                            Environment.Exit(1);
                            i += 1;
                        }
                    }
                }

                if (!textFromFile)
                {
                    // let the user specify a file implicitly
                    var inputText = _originalArgs[^1];
                    ParsedArgs.Text = (Validators.TextIsFile(inputText))
                        ? Utils.ReadFileContent(inputText).Trim()
                        : inputText.Trim();
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
            public bool ShowHelpTextFull { get; set; }
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
        None,
        ShowHelp,
        ShowFullHelp,
        ShowVersion,
        RunInteractive,
        RunTest,
    }
}