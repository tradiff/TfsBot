using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TfsBot.Services
{
    public class FormatService
    {
        private static readonly Dictionary<(string, Type), Script<string>> Formatters = new Dictionary<(string, Type), Script<string>>();

        public async Task<string> Format<T>(T model, string formatString)
        {
            //handle quotes in string
            formatString = formatString.Replace("\"", "\"\"").Replace("'", "\"\"");
            var hash = Sha1Hash(formatString);
            var key = (hash, model.GetType());

            if (Formatters.ContainsKey(key))
            {
                return await RunScript(Formatters[key], model);
            }

            lock (Formatters)
            {
                if (Formatters.ContainsKey(key))
                {
                    return RunScript(Formatters[key], model).Result;
                }

                var result = CSharpScript.Create<string>("$@\"" + formatString + "\"", globalsType: model.GetType());
                Formatters.Add(key, result);
                return RunScript(result, model).Result;
            }
        }

        private static async Task<string> RunScript<T>(Script<string> script, T model)
        {
            Serilog.Log.Information($"{GC.GetTotalMemory(false) / 1024 / 1024}");
            var result = await script.RunAsync(model);
            Serilog.Log.Information($"{GC.GetTotalMemory(false) / 1024 / 1024}");
            return result.ReturnValue;
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