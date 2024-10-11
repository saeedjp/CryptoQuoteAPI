using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CryptoQuoteAPI.Services;

namespace CryptoQuoteAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CryptoQuoteController(ICryptoQuoteService cryptoQuoteService) : ControllerBase
{
    /// <summary>
    /// Retrieves the current cryptocurrency quote for a given code.
    /// </summary>
    /// <param name="cryptoCode">The code of the cryptocurrency (e.g., BTC, ETH, LTC).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>An IActionResult containing the cryptocurrency quote.</returns>
    /// <response code="200">Returns the cryptocurrency quote.</response>
    /// <response code="404">If the cryptocurrency code is not found.</response>
    /// <response code="503">If there is an error communicating with the external API.</response>
    /// <response code="500">If there is a problem processing the request.</response>
    [HttpGet("{cryptoCode}")]
    public async Task<IActionResult> GetQuote(string cryptoCode, CancellationToken cancellationToken)
    {
        try
        {
            var symbol = cryptoCode.Trim().ToUpper();

            var result = await cryptoQuoteService.GetQuoteAsync(symbol, cancellationToken);
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