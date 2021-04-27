using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Landmark.FloodData;
using Landmark.FloodData.Processor.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FloodDataTest
{
    public class ServerTest
    {
        [Test]
        public async Task GetFlood_WithFromActualServerWithNoRegion_ReturnsUnfilteredData()
        {
            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.development.json");
                })
                .UseStartup<Startup>();

            var testServer = new TestServer(builder);
            var client = testServer.CreateClient();

            var response = await client.GetAsync("/Flood");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<List<Flood>>(content);
            Assert.IsInstanceOf<List<Flood>>(json);
        }
    }
}

  
