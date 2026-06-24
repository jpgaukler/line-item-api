using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LineItem.Api.Clients;

public class PdfGeneratorClient
{
    private readonly HttpClient _httpClient;

    public PdfGeneratorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        // Convert raw HTML string into a stream content block
        var htmlBytes = Encoding.UTF8.GetBytes(html);
        using var htmlStream = new MemoryStream(htmlBytes);
        using var streamContent = new StreamContent(htmlStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");

        // Gotenberg looks specifically for a form field named "files" and an individual filename called "index.html"
        using var formContent = new MultipartFormDataContent();
        formContent.Add(streamContent, "files", "index.html");

        var response = await _httpClient.PostAsync("http://localhost:3000/forms/chromium/convert/html", formContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            throw new Exception(
                $"Gotenberg PDF generation failed. Status: {response.StatusCode}. Details: {errorDetails}");
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();

        return bytes;
    }
}