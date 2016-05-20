using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TfsSlackFactory.Services
{
    public class FormatService
    {
        public string Format<T>(T model, string formatString)
        {
            //handle quotes in string
            formatString = formatString.Replace("\"", "\"\"").Replace("'", "\"\"");

            return CSharpScript.EvaluateAsync<string>("$@\"" + formatString + "\"", globals: model).Result;
        }
    }
}