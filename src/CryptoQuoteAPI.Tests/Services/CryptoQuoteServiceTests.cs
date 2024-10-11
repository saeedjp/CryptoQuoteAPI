using System.Net;
using System.Text.Json;
using CryptoQuoteAPI.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace CryptoQuoteAPI.Tests.Services;

public class CryptoQuoteServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory;
    private readonly Mock<IConfiguration> _configuration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public CryptoQuoteServiceTests()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>();
        _configuration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Create HttpClient using the mocked handler
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
    }

    [Fact]
    public async Task GetQuoteAsync_ReturnsCorrectQuote()
    {
        // Arrange
        _configuration.Setup(c => c["ExchangeRatesApiKey"]).Returns("test_exchange_key");
        _configuration.Setup(c => c["CoinMarketCapApiKey"]).Returns("test_coinmarketcap_key");

        // Mock response for CoinMarketCap API
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        BTC = new
                        {
                            quote = new
                            {
                                USD = new
                                {
                                    price = 50000m
                                }
                            }
                        }
                    }
                }))
            });

        // Mock response for Exchange Rates API
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("api.exchangeratesapi.io")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    rates = new
                    {
                        EUR = 0.85m,
                        BRL = 5.25m,
                        GBP = 0.75m,
                        AUD = 1.35m
                    }
                }))
            });

        var service = new CryptoQuoteService(_httpClientFactory.Object, _configuration.Object);

        // Create a CancellationTokenSource
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            // Act
            var result = await service.GetQuoteAsync("BTC", cancellationTokenSource.Token);

            // Assert
            Assert.Equal(50000m, result.Usd);
            Assert.Equal(42500m, result.Eur);
            Assert.Equal(262500m, result.Brl);
            Assert.Equal(37500m, result.Gbp);
            Assert.Equal(67500m, result.Aud);
        }
    }


    [Fact]
    public async Task GetUsdPriceAsync_ThrowsHttpRequestException_WhenApiFails()
    {
        // Arrange
        _configuration.Setup(c => c["CoinMarketCapApiKey"]).Returns("test_coinmarketcap_key");
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var service = new CryptoQuoteService(_httpClientFactory.Object, _configuration.Object);

        // Act & Assert
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            var exception =
                await Assert.ThrowsAsync<HttpRequestException>(() =>
                    service.GetQuoteAsync("BTC", cancellationTokenSource.Token));
            Assert.NotNull(exception);
        }
    }

    [Fact]
    public async Task GetUsdPriceAsync_ThrowsKeyNotFoundException_WhenPriceNotFound()
    {
        // Arrange
        _configuration.Setup(c => c["CoinMarketCapApiKey"]).Returns("test_coinmarketcap_key");

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    data = new
                    {
                        BTC = new { } // Missing 'quote' property
                    }
                }))
            });

        var service = new CryptoQuoteService(_httpClientFactory.Object, _configuration.Object);

        // Act & Assert
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            var exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    service.GetQuoteAsync("BTC", cancellationTokenSource.Token));
            Assert.Equal("Price data for BTC not found in the API response.", exception.Message);
        }
    }
}