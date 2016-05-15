using System.Net;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json.Linq;
using TfsSlackFactory.Models;

namespace TfsSlackFactory.Services
{
    public class TfsService
    {
        private readonly NetworkCredential _networkCredential;
        private readonly string _baseAddress;

        public TfsService(IOptions<TfsSettings> tfsSettings)
        {
            _networkCredential = new NetworkCredential(tfsSettings.Value.Username, tfsSettings.Value.Password);
            _baseAddress = tfsSettings.Value.Server;
        }


        public void GetWorkItem(int workItemId)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = _networkCredential;
                var response = client.DownloadString($"{_baseAddress}_apis/wit/workItems/{workItemId}?$expand=relations&api-version=1.0");
                dynamic responseObject = JObject.Parse(response);
            }
        }
    }
}