# Answers to Technical Questions

  

1. **How long did you spend on the coding assignment? What would you add to your solution if you had more time?**

  

   I spent approximately 2 hours on the coding assignment. If I had more time, I would add the following:

   - Implement caching to reduce API calls and improve performance

   - Add more comprehensive error handling and logging

   - Implement rate limiting to comply with API usage restrictions

  

2. **What was the most useful feature that was added to the latest version of your language of choice? Please include a snippet of code that shows how you've used it.**

  

   One of the most useful features added in C# 12 is primary constructors for classes. Here's an example of how I've used it:

  

   ```csharp

//primary constructors
public class CryptoQuoteService (IHttpClientFactory clientFactory,IConfiguration configuration): ICryptoQuoteService  
{  
  
  
  
    public async Task<CryptoQuoteResponse> GetQuoteAsync(string cryptoCode)  
    {      
    
	  var exchangeRatesApiKey = configuration["ExchangeRatesApiKey"];  
        var coinMarketCapApiKey = configuration["CoinMarketCapApiKey"];  
        if (exchangeRatesApiKey is null && coinMarketCapApiKey is null)  
        {            throw new ArgumentNullException($"Failed to retrieve config .");  
        }  
        var usdPrice = await GetUsdPriceAsync(cryptoCode, coinMarketCapApiKey!);  
        var exchangeRates = await GetExchangeRatesAsync(exchangeRatesApiKey!);  
  
        return new CryptoQuoteResponse(  
            Usd: usdPrice,  
            Eur: usdPrice * exchangeRates["EUR"],  
            Brl: usdPrice * exchangeRates["BRL"],  
            Gbp: usdPrice * exchangeRates["GBP"],  
            Aud: usdPrice * exchangeRates["AUD"]  
        );   
	}
}

   ```

  

3. **How would you track down a performance issue in production? Have you ever had to do this?**

  

   To track down a performance issue in production, I would:

   1. Use application performance monitoring.

   2. Analyze logs for any errors or unusual patterns and use tools like sentry and etc .

   3. Monitor database query performance

   4. Check server resources (CPU, memory, disk I/O)

   5. Implement distributed tracing if dealing with microservices use jaeger

  

   Yes, I have had to do this in previous projects. In one case, we discovered that a poorly optimized database query was causing slow response times .

  

4. **What was the latest technical book you have read or tech conference you have been to? What did you learn?**

  

  The last thing I learned focused on design patterns, specifically the Visitor pattern. This pattern allows you to separate an algorithm from the object structure it operates on, making it easier to add new operations without modifying the existing code. Understanding this pattern has enhanced my appreciation for the flexibility it provides in maintaining and extending software systems.

  

5. **What do you think about this technical assessment?**

  

   I think this technical assessment is well-rounded and relevant to real-world scenarios. It tests not only coding skills but also the ability to integrate multiple APIs and handle data transformation. The addition of technical questions allows candidates to showcase their broader understanding of software development practices.

  

6. **Please, describe yourself using JSON.**

  

   ```json

   {

     "name": "saeed jafarpanah",

     "profession": "Software Developer",

     "skills": ["C#", ".NET", "Web Development", "no-sql db", "rdbms"],

     "interests": ["Clean Code", "Software Architecture", "Continuous Learning"],

     "experience": +3,

     "education": {

       "degree": "Bachelor",

       "major": "Electrical engineering",

       "university": "tafresh"

     },

     "languages": ["English", "persian"],

     "hobbies": ["Billiards", "Mafia game", "Playing Guitar"]

   }

   ```