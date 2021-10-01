using EventsSample;
using Google.Cloud.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHostedService<SubscriberService>();
builder.Services.AddSingleton<PublisherService>();
builder.Services.AddScoped<EventRespository>();

if (builder.Environment.IsProduction()) 
{
    builder.Logging.AddConsoleFormatter<GoogleCloudConsoleFormatter, GoogleCloudConsoleFormatterOptions>(
            options => options.IncludeScopes = true)
        .AddConsole(options => 
            options.FormatterName = nameof(GoogleCloudConsoleFormatter));
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(endpoints =>
    endpoints.MapHub<NotifyHub>("/notifyhub")
);

app.MapGet("/events", (HttpContext http, EventRespository events) => 
    events.Get()
);

app.MapPost("/events", async (HttpContext http, EventRespository events) => {
    if (!http.Request.HasJsonContentType())
    {
        http.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        return;
    }
    
    var eventItem = await http.Request.ReadFromJsonAsync<Event>();
    
    if (eventItem != null)
    {
        await events.CreateAsync(eventItem);
        await http.Response.WriteAsJsonAsync(eventItem);    
    }
    else
    {
        http.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
});

app.MapDelete("/events/{id}", (HttpContext http, EventRespository events, string id) => {
    events.Delete(id);
    app.Logger.LogInformation($"Deleted id {id}");
    http.Response.StatusCode = StatusCodes.Status200OK;
});

try 
{
    app.Run();
}
catch (Exception e)
{
    app.Logger.LogCritical(e, "Unhandled exception");
}