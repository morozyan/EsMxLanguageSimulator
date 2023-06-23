using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EsMxSimulator.Core.Services;
using EsMxSimulator.Core.Models;
using System.Web.Http;
using Microsoft.Net.Http.Headers;

namespace EsMxSimulator.NumberApp;

public class Generator
{
    private const string GuessedNumberHeaderName = "guessed_number";

    private readonly INumberSimulator _numberSimulator;

    public Generator(INumberSimulator numberSimulator)
    {
        _numberSimulator = numberSimulator;
    }

    [FunctionName("Generator")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        var endNumberValue = req.Query["maxValue"];

        if (!int.TryParse(endNumberValue, out var endNumber) || 0 >= endNumber || endNumber > 10000)
        {
            log.LogWarning("Incorrect value: {EndNumber}", endNumber);
            return new BadRequestObjectResult(endNumberValue);
        }

        Turn result;
        try
        {
            result = await _numberSimulator.GuessNumber(0, endNumber);

            req.HttpContext.Response.Headers.Add(HeaderNames.AccessControlExposeHeaders, GuessedNumberHeaderName);
            req.HttpContext.Response.Headers.Add(GuessedNumberHeaderName, result.Number.ToString());
        }
        catch (Exception e)
        {
            log.LogError(e, $"Error");
            return new InternalServerErrorResult();
        }

        return new FileContentResult(result.Voice, "audio/wav");
    }
}
