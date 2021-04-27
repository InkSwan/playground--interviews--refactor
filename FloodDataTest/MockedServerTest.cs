using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Landmark.FloodData;
using Landmark.FloodData.Processor.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FloodDataTest
{
    public class MockedServerTest
    {
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private HttpClient _client;
        private WebApplicationFactory<Startup> _factory;

        [SetUp]
        public void SetUp()
        {
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            _factory = new WebApplicationFactory<Startup>();
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddHttpClient("EnvironmentAgency")
                        .ConfigurePrimaryHttpMessageHandler(() => _mockMessageHandler.Object);
                });
            }).CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task GetFlood_WithNoRegion_ReturnsUnfilteredData()
        {
            SetupResponse(HttpStatusCode.OK, LoadSample());

            var response = await _client.GetAsync("/Flood");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var floods = JsonConvert.DeserializeObject<List<Flood>>(content);
            Assert.IsInstanceOf<List<Flood>>(floods);
            CollectionAssert.IsSupersetOf(
                floods.Select(flood => flood.EaAreaName),
                new[]{ "Cornwall", "North", "West", "Kent and South London", "Eastern"});
        }


        [Test]
        public async Task GetFlood_WithRegion_ReturnsOnlyRegionSpecified()
        {
            SetupResponse(HttpStatusCode.OK, LoadSample());
            
            var response = await _client.GetAsync("/Flood/Cornwall");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var floods = JsonConvert.DeserializeObject<List<Flood>>(content);
            Assert.IsInstanceOf<List<Flood>>(floods);
            CollectionAssert.AreEqual(new[] { "Cornwall" },
                floods.Select(flood => flood.EaAreaName).Distinct());
        }

        [Test]
        public async Task GetFlood_WithNotFoundFromEa_ReturnsNotFound()
        {
            SetupResponse(HttpStatusCode.NotFound, "");

            var response = await _client.GetAsync("/Flood");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task GetFlood_WithExceptionThrown_ReturnsInternalServerError()
        {
            SetupResponseThatThrows();

            var response = await _client.GetAsync("/Flood");

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Test]
        public async Task GetFlood_ReturnJsonWithFriendlyEnums()
        {
            SetupResponse(HttpStatusCode.OK, LoadSample());

            var response = await _client.GetAsync("/Flood");

            var content = await response.Content.ReadAsStringAsync();
            StringAssert.Contains("\"severity\":\"Amber\"", content);
            StringAssert.Contains("\"action\":\"Ignore\"", content);
        }

        private void SetupResponse(HttpStatusCode statusCode, string content)
        {
            _mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(content)
                });
        }

        private void SetupResponseThatThrows()
        {
            _mockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException());
        }

        private string LoadSample()
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"{GetType().Namespace}.SampleResponse.eaSampleData.json");

            Assume.That(stream, Is.Not.Null, "Sample not found");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}