using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.WebHost
            .UseKestrel(o =>
            {
                o.AllowSynchronousIO = true;
            });
        
        builder.Services.AddControllers();
        
        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}

[Route("/")]
public class WeatherForecastController : ControllerBase
{
    [HttpPost("{**path}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> Upload()
    {
        var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(Request.ContentType).Boundary).Value;
        var reader = new MultipartReader(boundary!, Request.Body);
        MultipartSection section = await reader.ReadNextSectionAsync();

        var buffer = new byte[1_000_000];
        
        // Copy section to buffer
        var read = fillBuffer(section!.Body, buffer, 1_000_000);
        
        if (Encoding.ASCII.GetString(buffer.Take(4).ToArray()) != "PK\u0003\u0004") // Check if it's a zip file: https://en.wikipedia.org/wiki/ZIP_(file_format)#File_headers
            return BadRequest("Not a zip file, but should be!");
        
        return Ok();
    }
    
    private int fillBuffer(Stream stream, byte[] buffer, int count)
    {
        var read = 0;
        while (read < count)
        {
            var readNow = stream.Read(buffer, read, count - read);
            if (readNow == 0)
                break;
            read += readNow;
        }

        return read;
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}