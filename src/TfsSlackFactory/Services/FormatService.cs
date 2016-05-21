using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TfsSlackFactory.Services
{
    public class FormatService
    {
        public async Task<string> Format<T>(T model, string formatString)
        {
            //handle quotes in string
            formatString = formatString.Replace("\"", "\"\"").Replace("'", "\"\"");

            return await CSharpScript.EvaluateAsync<string>("$@\"" + formatString + "\"", globals: model);
        }
    }
}