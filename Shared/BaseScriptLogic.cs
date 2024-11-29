using RestSharp;

namespace Shared
{
    public partial class BaseScriptLogic
    {
        public static RestClient SetupRest(string url, string key)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(url);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(key);

            var restClient = new RestClient(url);
            restClient.AddDefaultHeader(Consts.ApiHeaders.ApiKey, key);

            return restClient;
        }
    }
}
