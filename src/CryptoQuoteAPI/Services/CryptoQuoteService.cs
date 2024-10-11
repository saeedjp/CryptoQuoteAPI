using System.Text.Json;
using CryptoQuoteAPI.Models;

namespace CryptoQuoteAPI.Services;

public class CryptoQuoteService(IHttpClientFactory clientFactory, IConfiguration configuration) : ICryptoQuoteService
{
    public async Task<CryptoQuoteResponse> GetQuoteAsync(string cryptoCode, CancellationToken cancellationToken)
    {
        var exchangeRatesApiKey = configuration["ExchangeRatesApiKey"];
        var coinMarketCapApiKey = configuration["CoinMarketCapApiKey"];
        if (exchangeRatesApiKey is null && coinMarketCapApiKey is null)
        {
            throw new ArgumentNullException($"Failed to retrieve config .");
        }

        var usdPrice = await GetUsdPriceAsync(cryptoCode, coinMarketCapApiKey!, cancellationToken);
        var exchangeRates = await GetExchangeRatesAsync(exchangeRatesApiKey!, cancellationToken);

        return new CryptoQuoteResponse(
            Usd: usdPrice,
            Eur: usdPrice * exchangeRates["EUR"],
            Brl: usdPrice * exchangeRates["BRL"],
            Gbp: usdPrice * exchangeRates["GBP"],
            Aud: usdPrice * exchangeRates["AUD"]
        );
    }

    private async Task<decimal> GetUsdPriceAsync(string cryptoCode, string apiKey, CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
        var response =
            await client.GetAsync(
                $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={cryptoCode.ToUpper()}",
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Log the failure
            Console.WriteLine($"Failed to fetch data for {cryptoCode}. Status code: {response.StatusCode}");
            throw new HttpRequestException();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;
            if (root.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty(cryptoCode, out var cryptoElement) &&
                cryptoElement.TryGetProperty("quote", out var quoteElement) &&
                quoteElement.TryGetProperty("USD", out var usdElement) &&
                usdElement.TryGetProperty("price", out var priceElement))
            {
                decimal price = priceElement.GetDecimal();
                return price;
            }

            // Log and throw exception if the price data is not found
            string errorMessage = $"Price data for {cryptoCode} not found in the API response.";
            Console.WriteLine(errorMessage);
            throw new KeyNotFoundException(errorMessage);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
            Console.WriteLine($"Raw response: {content}");
            throw;
        }
    }


    private async Task<Dictionary<string, decimal>> GetExchangeRatesAsync(string apiKey,
        CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient();
        var response =
            await client.GetAsync(
                $"https://api.exchangeratesapi.io/v1/latest?access_key={apiKey}&symbols=EUR,BRL,GBP,AUD",
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new KeyNotFoundException($"Failed to fetch exchange rates. Status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("rates", out var ratesElement))
            {
                var exchangeRates = new Dictionary<string, decimal>();

                foreach (var currency in new[] { "EUR", "BRL", "GBP", "AUD" })
                {
                    if (ratesElement.TryGetProperty(currency, out var rateElement))
                    {
                        exchangeRates[currency] = rateElement.GetDecimal();
                    }
                    else
                    {
                        // Log if a specific currency rate is not found
                        Console.WriteLine($"Exchange rate for {currency} not found in response.");
                        throw new KeyNotFoundException($"Exchange rate for {currency} not found.");
                    }
                }

                return exchangeRates;
            }

            throw new KeyNotFoundException($"Exchange rate not found.");
        }
        catch (JsonException ex)
        {
            // Catch deserialization issues
            Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
            Console.WriteLine($"Raw response: {content}");
            throw;
        }
    }
}