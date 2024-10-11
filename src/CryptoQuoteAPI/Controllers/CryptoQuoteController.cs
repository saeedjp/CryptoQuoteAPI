using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CryptoQuoteAPI.Services;

namespace CryptoQuoteAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CryptoQuoteController : ControllerBase
{
    private readonly ICryptoQuoteService _cryptoQuoteService;

    public CryptoQuoteController(ICryptoQuoteService cryptoQuoteService)
    {
        _cryptoQuoteService = cryptoQuoteService;
    }

    [HttpGet("{cryptoCode}")]
    public async Task<IActionResult> GetQuote(string cryptoCode, CancellationToken cancellationToken)
    {
        try
        {
            var symbol = cryptoCode.Trim().ToUpper();

            var result = await _cryptoQuoteService.GetQuoteAsync(symbol, cancellationToken);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                $"Error communicating with external API: {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            return StatusCode(StatusCodes.Status404NotFound, $"{ex.Message}");
        }
        catch (JsonException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"There is a problem, please try again later");
        }
        catch (ArgumentNullException ex)
        {
            return StatusCode(StatusCodes.Status404NotFound, $"{ex.Message}");
        }
    }

}