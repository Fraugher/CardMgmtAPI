//using System.Net;
//using Microsoft.AspNetCore.Mvc.Testing;

//namespace CardManagementAPI.Test
//{
//    public class CardTests
//    {
//        [Fact]
//        public async Task GET_retrieves_weather_forecast()
//        {
//            await using var application = new WebApplicationFactory<CardManagementAPI.Startup>();
//            using var client = application.CreateClient();

//            var response = await client.GetAsync("/weatherforecast");
//            response.StatusCode.Should().Be(HttpStatusCode.OK);
//        }
//    }
//}
