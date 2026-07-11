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
        public static readonly int DefaultTextWidth = 80;

        public static readonly string HelpText = $"""
                                                    usage: {Prog} [flag] [option <arg>] ... (CIPHER|TEXT)
                                                    
                                                    encrypt and decrypt your memo using the polybius cipher
                                                    
                                                    examples:
                                                      {Prog} -k $KEY $TEXT
                                                      {Prog} -k $KEYFILE.txt -d $TEXT
                                                      {Prog} -k $KEY -f $FILE -e 
                                                      {Prog} -k $KEY -o $OUTPUTFILE $TEXT
                                                    
                                                    positional arguments:
                                                      (CIPHER|TEXT)       the input to encrypt or decrypt.
                                                                          in strict mode, input to be encrypted containing any 
                                                                          invalid character will raise and error whilein normal mode
                                                                          invalid characters will be ignored, possibly malforming 
                                                                          the original text. 
                                                                          valid characters are: all upper-/lowercase ascii alphabet 
                                                                          characters and whitespace. regex pattern: [A-Za-z\s]
                                                    
                                                    options:
                                                      -k,--key (KEY|FILE) the keyword to use for encryption or decryption.
                                                                          either a string of ascii alphabetical characters
                                                                          a <textfile>.txt containg that string.
                                                                          can be ommited, making the encrytion algorithm
                                                                          resort to the default alphabet, thus making it
                                                                          highly insecure. 
                                                                          using a key is generally highly advised.
                                                      -f,--file <FILE>    read the input TEXT from a file <textfile>.txt
                                                                          instead from arguments. input from file takes 
                                                                          precedence over a TEXT provided as positional 
                                                                          argument, meaning the latter is redundant and 
                                                                          will be ignored.
                                                      
                                                    flags:
                                                      -e,--encrypt        encrypt the input (default).
                                                      -d,--decrypt        decrypt the input.
                                                      -s,--strict         run in strict mode. see CIPHER|TEXT for details.
                                                      -i,--interactive    run in interactive mode.
                                                      -t,--test           run tests.
                                                      -h,--help           show this help message and exit.
                                                      -V,--version        show version info and exit.
                                                    
                                                    please note, that this software is barely a working prototype. it is the result
                                                    of a short termed practice assignment finishing a csharp introduction course
                                                    and has only been checked for the most obvious bugs and not tested thoroughly.
                                                    """;
        public static readonly string LicenseText = """
                                                    MIT License

                                                    Copyright (c) 2026 Kai Elsässer

                                                    Permission is hereby granted, free of charge, to any person obtaining a copy
                                                    of this software and associated documentation files (the "Software"), to deal
                                                    in the Software without restriction, including without limitation the rights
                                                    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                                                    copies of the Software, and to permit persons to whom the Software is
                                                    furnished to do so, subject to the following conditions:

                                                    The above copyright notice and this permission notice shall be included in all
                                                    copies or substantial portions of the Software.

                                                    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                                                    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                                                    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                                                    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                                                    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                                                    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                                                    SOFTWARE.
                                                    """;
    }
    
    public static void ShowVersion()
    {
        Console.Write($"{Constants.Prog} {Constants.Version}");
    }

    public static void ShowHelp(bool license = false)
    {
        Console.WriteLine(Constants.HelpText);
        if (license)
        {
            Console.WriteLine(
                $"\n{new string('-', Constants.DefaultTextWidth)}\n\n{Constants.LicenseText}"
                );
        }
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

    /// <summary>
    /// Validate the given filepath.
    /// </summary>
    /// <param name="path">The filepath to validate.</param>
    /// <param name="isDir">Wheather the path point to a directory.</param>
    /// <exception cref="FileNotFoundException">If file does not exist.</exception>
    public static void ValidateFilePath(string? path, bool isDir = false)
    {
        if (isDir)
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

