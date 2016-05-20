using Microsoft.CodeAnalysis.CSharp.Scripting;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class FormatService
    {
        public string Format(SlackWorkItemModel model, string formatString)
        {
            return CSharpScript.EvaluateAsync<string>("$@\"" + formatString + "\"", globals: model).Result;
        }
    }
}