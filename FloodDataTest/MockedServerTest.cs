using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Landmark.FloodData;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
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
                builder.ConfigureServices(services => { services.AddSingleton(_mockMessageHandler.Object); });
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
            StringAssert.Contains("<ArrayOfFlood", content);
            StringAssert.Contains("<EaAreaName>Cornwall</EaAreaName>", content);
            StringAssert.Contains("<EaAreaName>North</EaAreaName>", content);
            StringAssert.Contains("<EaAreaName>West</EaAreaName>", content);
            StringAssert.Contains("<EaAreaName>Kent and South London</EaAreaName>", content);
            StringAssert.Contains("<EaAreaName>Eastern</EaAreaName>", content);
        }


        [Test]
        public async Task GetFlood_WithRegion_ReturnsOnlyRegionSpecified()
        {
            SetupResponse(HttpStatusCode.OK, LoadSample());

            var response = await _client.GetAsync("/Flood/Cornwall");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            StringAssert.Contains("<EaAreaName>Cornwall</EaAreaName>", content);
            StringAssert.DoesNotContain("<EaAreaName>North</EaAreaName>", content);
            StringAssert.DoesNotContain("<EaAreaName>West</EaAreaName>", content);
            StringAssert.DoesNotContain("<EaAreaName>Kent and South London</EaAreaName>", content);
            StringAssert.DoesNotContain("<EaAreaName>Eastern</EaAreaName>", content);
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