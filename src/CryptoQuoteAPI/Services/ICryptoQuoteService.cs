using CryptoQuoteAPI.Models;

namespace CryptoQuoteAPI.Services;

public interface ICryptoQuoteService
{
    Task<CryptoQuoteResponse> GetQuoteAsync(string cryptoCode);
}