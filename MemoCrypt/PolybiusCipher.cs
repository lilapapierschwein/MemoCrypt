using System.Text;
using System.Text.RegularExpressions;

namespace MemoCrypt;

public class PolybiusCipher
{
    private static readonly (int Rows, int Colums) GridSize = (6, 5);
    private const string BaseCharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
    private readonly char?[,] _encryptionMatrix;
    private readonly Dictionary<char, (int Row, int Col)> _coordinateOf;
    private string _keyword;
    
    private string _keyedAlphabet;
    private string GetKeyedAlphabet()
    {
        return _keyedAlphabet;
    }
    private void SetKeyedAlphabet(string keyword)
    {
        _keyedAlphabet = CompileKeyedAlphabet(NormalizeKey(keyword));
        if (string.IsNullOrEmpty(_keyedAlphabet))
        {
            _keyedAlphabet = BaseCharSet;
        }
    }

    /// <summary>
    /// Initialize a new instance of the PolybiusCipher class to encrypt and decrypt memos.
    ///
    /// Input validation will be based on the strictness setting.
    /// If `strict` is set to `true`, only memos consisting of characters a-z (case-insensitive) and whitespaces are allowed.
    /// Otherwise, all invalid characters will be removed from the input.
    /// 
    /// </summary>
    /// <param name="keyword">The keyword</param>
    /// <param name="strict">Strictness (default: false).</param>
    public PolybiusCipher(string keyword = "",  bool strict = false)
    {
        _keyword = keyword;
        SetStrict(strict);
        
        _keyedAlphabet = CompileKeyedAlphabet(NormalizeKey(keyword));
        _encryptionMatrix = new char?[GridSize.Rows, GridSize.Colums];
        _coordinateOf = new Dictionary<char, (int Row, int Col)>();
        
        BuildEncryptionMatrix(GetKeyedAlphabet());
    }
    
    /// <summary>
    /// Encrypt a memo.
    /// </summary>
    /// <param name="plaintext">The memo to encrypt. Restrictions apply based on the strictness setting.</param>
    /// <returns>The original memo text.</returns>
    /// <exception cref="FormatException">On empty input or if the lenght of the ciphertext is uneven.</exception>
    public string Encrypt(string plaintext)
    {
        if (_strict && !Validators.IsValidText(plaintext))
        {
            throw new FormatException($"Plaintext '{plaintext}' is not a valid string.");
        }
        string normalizedText = Validators.NormalizeString(plaintext);
        var sb = new StringBuilder(normalizedText.Length * 2);

        foreach (char c in normalizedText)
        {
            var (row, col) = GetCoordinateOf(c);
            sb.Append(row+1);
            sb.Append(col+1);
        }
        var result = sb.ToString();
        return sb.ToString();
    }
    
    /// <summary>
    /// Decrypt a ciphertext.
    /// </summary>
    /// <param name="ciphertext">Text produced by running `Encrypt` on a memo.</param>
    /// <returns>The original memo text.</returns>
    /// <exception cref="FormatException">On empty input or if the lenght of the ciphertext is uneven.</exception>
    public string Decrypt(string ciphertext)
    {
        if (ciphertext.Length == 0)
        {
            throw new FormatException("Ciphertext is empty.");
        } else if (ciphertext.Length % 2 != 0)
        {
            throw new FormatException($"Ciphertext '{ciphertext}' must have an even number of digits.");
        }
        
        var sb = new StringBuilder(ciphertext.Length / 2);

        for (int i = 0; i < ciphertext.Length; i += 2)
        {
            int row = (ciphertext[i] - '0') - 1;
            int col = (ciphertext[i + 1] - '0') - 1;
            
            if (row < 0 || row >= GridSize.Rows || col < 0 || col >= GridSize.Colums)
                throw new FormatException($"Coordinate out of range: {ciphertext[i]}{ciphertext[i + 1]}");
            
            sb.Append(GetCharacter(row, col));
        }
        
        return sb.ToString();
    }

    public string GetKeyword()
    {
        return _keyword;
    }

    public void SetKeyWord(string keyword)
    {
        _keyword = NormalizeKey(keyword);
    }

    public void UpdateKey(string keyword = "", bool reload = false)
    {
        if (reload)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                SetKeyWord(keyword);
            }
            SetKeyedAlphabet(_keyword);
            BuildEncryptionMatrix(GetKeyedAlphabet());
            return;
        }
        SetKeyWord(keyword);
        SetKeyedAlphabet(_keyword);
        BuildEncryptionMatrix(GetKeyedAlphabet());
    }

    private bool _strict;
    public bool GetStrict()
    {
        return _strict;
    }

    public void SetStrict(bool strict)
    {
        _strict = strict;
    }
    
    /// <summary>
    /// Lookup method for the coordinate of a char in the enctryption matrix.
    /// </summary>
    /// <param name="c">The character to look up.</param>
    /// <returns>
    /// ValueTuple(int Row, int Col) 
    /// </returns>
    /// <exception cref="ArgumentException">When the character is not present.</exception>
    private (int Row, int Col) GetCoordinateOf(char c)
    {
        if (!_coordinateOf.TryGetValue(c, out var coordinate))
        {
            throw new ArgumentException($"Character '{c}' is not in the matrix");
        }

        return coordinate;
    }
    
    /// <summary>
    /// Populates the encryption matrix from the alphabet with the key incorporated.
    /// </summary>
    /// <param name="keyedAlphabet">The alphabet with the key incorporated.</param>
    private void BuildEncryptionMatrix(string keyedAlphabet)
    {
        for (int i = 0; i < keyedAlphabet.Length; i++)
        {
            int row = i / GridSize.Colums;
            int col = i % GridSize.Colums;
            
            char c = keyedAlphabet[i];
            _encryptionMatrix[row, col] = c;
            _coordinateOf[c] = (row, col);
        }
    }

    /// <summary>
    /// Get the character based on the matrix coordinates.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <param name="col">The column.</param>
    /// <returns>char: The character matching the coordinates.</returns>
    /// <exception cref="FormatException">When the coordinates map to an unused (overflow) grid cell.</exception>
    private char GetCharacter(int row, int col)
    {
        char? c = _encryptionMatrix[row, col];

        if (c is null)
        {
            throw new FormatException($"Coordinate ({row+1},{col+1}) is an unused grid cell.");
        }
        
        return c.Value;
    }
    
    /// <summary>
    /// Compiles the alphabet sequence to be placed in the encryption matrix.
    /// </summary>
    /// <param name="normalizedKey">The normalized and deduplicated keyword.</param>
    /// <returns>
    /// string: The alphabet sequence to place into the grid.
    /// </returns>
    private static string CompileKeyedAlphabet(string normalizedKey)
    {
        string compliledAlphabet = normalizedKey;
        foreach (var c in BaseCharSet)
        {
            if (!compliledAlphabet.Contains(c))
            {
                compliledAlphabet += c;
            }
        }
        return compliledAlphabet;
    }

    
    /// <summary>
    /// Normalizes and deduplicates the input.
    /// </summary>
    /// <param name="keyword">The plain keyword.</param>
    /// <returns>
    /// string: The normalized and deduplicated keyword. (Can also be empty).
    /// </returns>
    private static string NormalizeKey(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return string.Empty;
        }

        var normalizedKeyword = Validators.NormalizeString(keyword);
        if (string.IsNullOrEmpty(normalizedKeyword))
        {
            return string.Empty;
        } else if (normalizedKeyword.Length == 1)
        {
            return normalizedKeyword;
        }
        return new string(normalizedKeyword.Distinct().ToArray());
    }
}

/// <summary>
/// Convenience class providing methods for text- and input validation.
/// </summary>
public abstract partial class Validators
{
    [GeneratedRegex(@"[^A-Za-z\s]")]
    private static partial Regex InvalidChars();
    /// <summary>
    /// Check if the string consists of valid characters only.
    /// </summary>
    /// <param name="text">Text to validate.</param>
    /// <returns>`true` if all characters are valid, otherwise `false`</returns>
    public static bool IsValidText (string text)
    {
        return !InvalidChars().IsMatch(text);
    }
        
    [GeneratedRegex(@"[^A-Za-z\s]+")]
    private static partial Regex InValidCharsPattern();
    /// <summary>
    /// Converts the input to uppercase, removes all unallowed characters defined in the pattern `ValidCharsRegex`
    /// and returns the result with whitespaces truncated.
    /// 
    /// For C# Regex see quickref:
    /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference
    /// </summary>
    /// <param name="plaintext">The plain input.</param>
    /// <returns>
    /// string: The normalized string for further processing. 
    /// </returns>
    public static string NormalizeString(string plaintext)
    {
        // we use `string.ToUpperCaseInvariant` over `string.ToUpperCase` to avoid unexpected (local) character mappings
        var normalizedText = InValidCharsPattern().Replace(plaintext.ToUpperInvariant(), "");
        return Regex.Replace(normalizedText, @"\s{2,}", " ");
    }

}
