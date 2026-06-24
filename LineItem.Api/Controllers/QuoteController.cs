using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using LineItem.Api.Clients;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/quotes")]
public class QuoteController : ControllerBase
{
    // private readonly IQuoteService _quoteService;
    //
    // public QuoteController()
    // {
    //     _quoteService = quoteService;
    // }

    [HttpPost]
    public async Task<IActionResult> GeneratePdfAsync(CancellationToken cancellationToken)
    {
        // Step 1: Compile your Handlebars template
        var templateSource = "<h1>Line Item Quote</h1><p>Customer: {{CustomerName}}</p>";
        var template = Handlebars.Compile(templateSource);

        // Step 2: Hydrate template with your JSON/Data model data
        var data = new { CustomerName = "Test Customer" };
        var renderedHtml = template(data);

        // Step 3: Instantiate HTTP client and call Gotenberg
        using var httpClient = new HttpClient();
        var generator = new PdfGeneratorClient(httpClient);

        try
        {
            var pdfBytes = await generator.ConvertHtmlToPdfAsync(renderedHtml);

            await System.IO.File.WriteAllBytesAsync("output_report.pdf", pdfBytes, cancellationToken);
            Debug.WriteLine("PDF generated successfully via Gotenberg!");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }

        return Ok();
    }

    // [HttpPost]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> CreateAsync([FromBody] QuoteModel quote, CancellationToken cancellationToken)
    // {
    //     try
    //     {
    //         var createdQuote = await _quoteService.CreateAsync(quote, cancellationToken);
    //         return CreatedAtAction(
    //             nameof(GetByIdAsync),
    //             new { version = "1", id = createdQuote.Id },
    //             createdQuote
    //         );
    //     }
    //     catch (BadRequestException ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
    //
    // [HttpGet("{id:long}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    // {
    //     var result = await _quoteService.RetrieveByIdAsync(id, cancellationToken);
    //
    //     return result is not null
    //         ? Ok(result)
    //         : NotFound($"Quote with Id = {id} not found!");
    // }
    //
    // [HttpPut("{id:long}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // public async Task<IActionResult> UpdateAsync(
    //     [FromRoute] long id,
    //     [FromBody] QuoteModel quote,
    //     CancellationToken cancellationToken
    // )
    // {
    //     try
    //     {
    //         var updatedQuote = await _quoteService.UpdateAsync(id, quote, cancellationToken);
    //
    //         return updatedQuote is not null
    //             ? Ok(updatedQuote)
    //             : NotFound($"Quote with Id = {id} not found!");
    //     }
    //     catch (BadRequestException ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
    //
    // [HttpDelete("{id}")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    // {
    //     await _quoteService.DeleteAsync(id, cancellationToken);
    //     return NoContent();
    // }
}