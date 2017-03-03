using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace CVaS.Shared.Services.Broker
{
    public class BrokerStatus
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;

        public BrokerStatus(IOptions<BrokerOptions> brokerOptions)
        {
            _brokerOptions = brokerOptions;
        }


        public async Task<int> GetConnectedClients()
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(_brokerOptions.Value.Username, _brokerOptions.Value.Password)
            };

            using (var client = new HttpClient(handler))
            {
                var uri = new Uri("https://" + _brokerOptions.Value.Hostname + "/api/queues/" + _brokerOptions.Value.Vhost);
                var result = await client.GetAsync(uri);

                if (result.IsSuccessStatusCode)
                {
                    var jObject = JArray.Parse(await result.Content.ReadAsStringAsync());

                    return jObject[0]["consumers"].ToObject<int>();
                }

                throw new HttpRequestException();
            }
        }
    }
}