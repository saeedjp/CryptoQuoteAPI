namespace CryptoQuoteAPI.Models;

public record CryptoQuoteResponse(
    decimal Usd,
    decimal Eur,
    decimal Brl,
    decimal Gbp,
    decimal Aud
);