using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace CardManagementAPI.Test
{
    public class CardLiveTest : IDisposable
    {
        private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:7133") };
  
        public void Dispose()
        {
            _httpClient.DeleteAsync("/state").GetAwaiter().GetResult();
        }
    }
}
