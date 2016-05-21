
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TfsSlackFactory.Services
{
    public class EvalService
    {
        private readonly FormatService _formatService;
        public EvalService(FormatService formatService)
        {
            _formatService = formatService;
        }

        public async Task<bool> Eval<T>(T model, string formatString)
        {
            formatString = await _formatService.Format(model, formatString);

            //sanitize bool for eval
            formatString = formatString.Replace("True", "true").Replace("False", "false");

            return await CSharpScript.EvaluateAsync<bool>(formatString);
        }
    }
}