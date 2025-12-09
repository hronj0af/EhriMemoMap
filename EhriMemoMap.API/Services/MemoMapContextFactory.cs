using EhriMemoMap.Data.MemoMap;
using Microsoft.EntityFrameworkCore;

namespace EhriMemoMap.API.Services;

public class MemoMapContextFactory
{
    private readonly IConfiguration _configuration;

    public MemoMapContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public MemoMapContext GetContext(string? city)
    {
        if (string.IsNullOrEmpty(city))
        {
            throw new ArgumentNullException(nameof(city), "City must be provided");
        }

        var connectionString = city.ToLower() switch
        {
            "ricany" => _configuration["RICANY_DB"],
            "pacov" => _configuration["PACOV_DB"],
            _ => throw new ArgumentException($"Unknown city: {city}", nameof(city))
        };

        var optionsBuilder = new DbContextOptionsBuilder<MemoMapContext>();
        optionsBuilder.UseNpgsql(connectionString, x => x
            .UseNodaTime()
            .UseNetTopologySuite())
            .EnableSensitiveDataLogging();

        return new MemoMapContext(optionsBuilder.Options);
    }
}
