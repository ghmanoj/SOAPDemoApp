using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;


namespace SOAPDemo.Controllers;

public class WsResponse
{
    public bool Success { get; set; } = false;
    public string Data { get; set; } = "No Match";
}

[ApiController]
[Route("/")]
public class HomeController : Controller
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string API = "https://www.dataaccess.com/webservicesserver/TextCasing.wso";
    private static readonly Regex INVERT_RESULT_PATTERN = new Regex("<m:InvertStringCaseResult>(.*?)</m:InvertStringCaseResult>");

    [HttpGet]
    public async Task<ActionResult> Index([FromQuery(Name="input")] string input)
    {
        var xmlData = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                        <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                        <soap:Body>
                            <InvertStringCase xmlns = ""http://www.dataaccess.com/webservicesserver/"">
                                <sAString>{input}</sAString>
                            </InvertStringCase>
                        </soap:Body>
                        </soap:Envelope>
                       ";

        var soapResponse = await PostSOAPRequestAsync(API, xmlData);
        var regexMatch = INVERT_RESULT_PATTERN.Match(soapResponse);

        WsResponse response = new WsResponse();

        if (regexMatch.Success)
        {
            var match = regexMatch.Groups[1].ToString().Trim();
            response.Data = match;
            response.Success = true;
        }

        return Ok(response);
    }

    private static async Task<string> PostSOAPRequestAsync(string url, string text)
    {
        using HttpContent content = new StringContent(text, Encoding.UTF8, "text/xml");
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("SOAPAction", "InvertStringCase");
        request.Content = content;

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        return await response.Content.ReadAsStringAsync();
    }
}

