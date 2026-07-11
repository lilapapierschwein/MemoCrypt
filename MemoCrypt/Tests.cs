using System.Text;

namespace MemoCrypt;

public class Tests
{
    private static readonly PolybiusCipher Cipher = new PolybiusCipher();
    private static readonly Dictionary<string, Func<string, string, bool, (List<bool>, List<TestError>, string)>>
        TestFunctions =
            new Dictionary<string, Func<string, string, bool, (List<bool>, List<TestError>, string)>>
            {
                ["Encrypt/Decrypt"] = TestEncryptDecrypt,
            };
    private static readonly List<TestReport> TestReports = [];
    
    public void RunTests()
    {
        Utils.SetConsoleTitle("| Test Suite",extend:true);
        Console.WriteLine(Utils.CenterText(" Running tests...", fillchar: '=', useTermWidth: true));

        int testRunCounter = 1;
        foreach (var testFunction in TestFunctions)
        {
            Console.WriteLine($"Running Test {testRunCounter}/{TestFunctions.Count} ({testFunction.Key})\n");
            var testReport = new TestReport(testFunction.Key, testFunction.Value("strongkeyword", "lorem ipsim dolor sit amet", false));
            Console.WriteLine(testReport.ToString());
            TestReports.Add(testReport);
        }
        
        var totalTestsCount = TestReports.Count;
        var successfulTestCount = totalTestsCount;
        var totalErrorsCount = TestReports.Sum(report => report.ErrorCountTotal);
        
        string errorsText = (totalErrorsCount == 0)
            ? "Finished without errors"
            : $"Finished with {totalErrorsCount} errors";
        
        Utils.WriteColoredLine(Utils.CenterText($" All tests passed ({successfulTestCount}/{totalTestsCount}). {errorsText}. ", fillchar: '=', useTermWidth: true), ConsoleColor.Green);
    }
    
    static (List<bool> testsPassed, List<TestError> errors, string resultsText) TestEncryptDecrypt(string keyword = "strongkeyword", string memoText = "lorem ipsim dolor sit amet", bool strictMode = false)
    {
        List<bool> testsPassedFailed = [];
        List<TestError> errors = [];
        var resultsText = new StringBuilder();
        Cipher.UpdateKey(keyword);
        Cipher.SetStrict(strictMode);

        try
        {
            var normalizedMemoText = Validators.NormalizeString(memoText);
            var encryptedText = Cipher.Encrypt(memoText);
            (string keyword, string message)[] keywordChanges = [
                ("anotherkeyword","changed key"), 
                ("mal:f0rm*d k3y","malformed key"), 
                ("","empty key")
            ];
            
            resultsText.AppendLine($"ORIGINAL TEXT:   {memoText}\n\n[encrypting with original key]");
            for (int i = 0; i < keywordChanges.Length + 1; i++)
            {
                var decryptedText = Cipher.Decrypt(encryptedText);
                var passed = (i > 0) ? (decryptedText != normalizedMemoText) : (decryptedText ==  normalizedMemoText);
                testsPassedFailed.Add(passed);
                
                resultsText.AppendLine($"KEY:             {keyword}");
                resultsText.AppendLine($"ENCRYPTED TEXT:  {encryptedText}");
                resultsText.AppendLine($"DECRYPTED TEXT:  {decryptedText}");
                resultsText.AppendLine($"RESULT:          {(passed ? "PASSED ✅" : "FAILED ❌")}");

                if (i >= keywordChanges.Length) continue;
                keyword = keywordChanges[i].keyword;
                Cipher.UpdateKey(keyword);
                resultsText.AppendLine($"\n[encrypting with {keywordChanges[i].message}]");
            }
        }
        catch (FormatException exc)
        {
            errors.Add(new TestError(exc, exc.Message));
        }
        catch (ArgumentException exc)
        {
            errors.Add(new TestError(exc, exc.Message));
        }
        return (testsPassedFailed, errors, resultsText.ToString());
    }

    private class TestReport(string testName, (List<bool> testsPassedFailed, List<TestError> errors, string resultsText) results)
    {
        public string TestName = testName;
        private readonly List<bool> _testsPassedFailed = results.testsPassedFailed;
        private readonly string _resultsText = results.resultsText;
        // private List<TestError> _errors = results.errors;
        private readonly int _testCountTotal = results.testsPassedFailed.Count;
        public readonly int ErrorCountTotal = results.errors.Count;

        public override string ToString()
        {
            var text = new StringBuilder(_resultsText);
            text.AppendLine($"\n{GetEndResult()}");
            return text.ToString();
        }
        
        private string GetEndResult()
        {
            var sb = new StringBuilder();
            string passesFailsResult;

            if (_testCountTotal == 0 || _testsPassedFailed.All(_ => true))
            {
                passesFailsResult = (_testCountTotal == 0) ? $"All checks passed ✅" : $"All checks passed ✅ ({_testCountTotal}/{_testCountTotal})";
            }
            else
            {
                var failsCount = _testsPassedFailed.Count(_ => false);
                passesFailsResult = failsCount == _testCountTotal
                    ? $"All checks failed ❌ ({failsCount}/{_testCountTotal})"
                    : $"{_testCountTotal} checks run. Failed: {failsCount} ❌ Passed: {_testCountTotal -  failsCount} ✅";
            }
            sb.AppendLine(passesFailsResult);
            
            var errorsResult = (ErrorCountTotal == 0) 
                ? "Tests run without errors ✅" 
                : $"{ErrorCountTotal} errors ocurred ❗️";
            sb.AppendLine(errorsResult);
            return sb.ToString();
        }

        public bool IsSuccess()
        {
            return (_testCountTotal == 0 || _testsPassedFailed.All(_ => true));
        }

        public bool IsFailed(bool fullFailure)
        {
            if (_testCountTotal == 0) return false;
            
            var totalFailures = _testsPassedFailed.Count(_ => false);
            return (fullFailure)
                ? (totalFailures == _testCountTotal)
                : (totalFailures > 0 && totalFailures < _testCountTotal);
        }
    }

    private class TestError(Exception exc, string message)
    {
        public string Message = message;
        public Exception Exc = exc;
    }
}