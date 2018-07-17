
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace TfsBot.Services
{
    public class EvalService
    {
        private static readonly Dictionary<string, bool> Results = new Dictionary<string, bool>();

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
            var hash = Sha1Hash(formatString);

            if (Results.ContainsKey(hash))
            {
                return Results[hash];
            }

            lock (Results)
            {
                if (Results.ContainsKey(hash))
                {
                    return Results[hash];
                }

                var result = CSharpScript.EvaluateAsync<bool>(formatString).Result;
                Results.Add(hash, result);
                return result;
            }
        }

        private static string Sha1Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            SHA1CryptoServiceProvider sha1Provider = new SHA1CryptoServiceProvider();
            byte[] bytes = sha1Provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }

            return hash.ToString().ToUpper();
        }
    }
}