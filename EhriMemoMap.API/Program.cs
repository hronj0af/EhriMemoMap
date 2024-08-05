using EhriMemoMap.API.Services;
using EhriMemoMap.Data;
using EhriMemoMap.Services;
using EhriMemoMap.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory());
});


builder.Services.AddScoped<MapLogicService>();
builder.Services.AddScoped<SolrService>();
builder.Services.AddHttpClient();

// info about "Safe storage of app secrets in development in ASP.NET Core": https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows#secret-manager
//var connectionString = builder.Configuration["MemoMap:ConnectionString"];
// [Environment]::SetEnvironmentVariable("MemoMap:ConnectionString", "your_connection_string", "Machine")
var connectionString = Environment.GetEnvironmentVariable("MemoMap:ConnectionString", EnvironmentVariableTarget.Machine);
builder.Services.AddDbContext<MemogisContext>(options => options.UseNpgsql(connectionString));

var cors = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(cors, builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseCors(cors);

var proxyClient = new HttpClient();

app.MapGet("/", () => "Hello World...");

app.MapGet("/getdistrictstatistics", (bool total, DateTime? timeLinePoint, MapLogicService service, HttpContext context) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache");
    var parameters = new DistrictStatisticsParameters
    {
        Total = total,
        TimeLinePoint = timeLinePoint
    };
    return service.GetDistrictStatistics(parameters);
});

app.MapPost("/getmapobjects", (MapObjectParameters parameters, MapLogicService service) => service.GetMapObjects(parameters));

app.MapPost("/getwelcomedialogstatistics", (MapLogicService service) => service.GetWelcomeDialogStatistics());

app.MapPost("/getplaces", (PlacesParameters parameters, MapLogicService service) => service.GetPlaces(parameters));

app.MapPost("/getsolrplaces", (SolrQueryParameters parameters, SolrService service) => service.SolrExecuteDocument(parameters));

app.MapGet("/wmsProxy", async (HttpContext context) =>
{
    var url = "https://geodata.ehri-project.eu/geoserver/ehri/wms?" + context.Request.QueryString.Value;
    var imageFileName = url.GetHashCode() + ".png";
    var imageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache/" + imageFileName);

    context.Response.Headers.Append("Cache-Control", "public,max-age=1000000");

    if (File.Exists(imageFilePath))
        return Results.File(File.ReadAllBytes(imageFilePath), "image/png", imageFileName);

    var result = await proxyClient.GetByteArrayAsync(url);
    File.WriteAllBytes(imageFilePath, result);
    return Results.File(result, "image/png", imageFileName);
});

app.Run();
