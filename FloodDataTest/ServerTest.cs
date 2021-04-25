using System.Net;
using System.Threading.Tasks;
using Landmark.FloodData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace FloodDataTest
{
    public class ServerTest
    {
        [Test]
        public async Task GetFlood_WithFromActualServerWithNoRegion_ReturnsUnfilteredData()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();

            var testServer = new TestServer(builder);
            var client = testServer.CreateClient();

            var response = await client.GetAsync("/Flood");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            StringAssert.Contains("<ArrayOfFlood", content);
        }
    }
}

  
