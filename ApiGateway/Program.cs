using Serilog;
using System.Diagnostics;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/ApiGateway-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
            PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                            RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey:
                                    httpContext.Connection.RemoteIpAddress.ToString() ?? "unknown",

                                factory: partitionKey =>
                                     new FixedWindowRateLimiterOptions
                                     {
                                         AutoReplenishment = true,
                                         PermitLimit = 5,
                                         Window = TimeSpan.FromMinutes(1),
                                         QueueLimit = 0
                                     }));
    options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-ID";

    var correlationId =
        context.Request.Headers[headerName].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(correlationId) ||
        correlationId.Length > 100)
    {
        correlationId = Guid.NewGuid().ToString();
    }

    context.Items["CorrelationId"] = correlationId;

    context.Response.Headers[headerName] = correlationId;

    context.TraceIdentifier = correlationId;

    using (Serilog.Context.LogContext.PushProperty(
        "CorrelationId", correlationId))
    {
        await next();
    }
});

app.Use(async (context, next) =>
{
    var stopWatch = Stopwatch.StartNew();
    var logger = app.Logger;

    logger.LogInformation("Incoming Request: {Method} {Path}", context.Request.Method, context.Request.Path);

    try
    {
        await next();
    }
    catch(Exception ex)
    {
        logger.LogError(
            ex,
            "Request failed: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        throw;
    }
    finally
    {
        stopWatch.Stop();

        logger.LogInformation("Completed Request: {Method} {Path}, StatusCode: {StatusCode}, Duration: {Duration}ms", context.Request.Method, context.Request.Path, context.Response.StatusCode, stopWatch.ElapsedMilliseconds);
    }

});
app.MapHealthChecks("/health");

app.UseRateLimiter(); 
app.MapReverseProxy();
app.MapGet("/", () => "Hello World!");


app.Run();
