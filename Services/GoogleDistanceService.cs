using System.Text.Json;

namespace FlashFood.Web.Services;

public interface IGoogleDistanceService
{
    Task<decimal?> GetDistanceInKmAsync(string fullAddress, decimal? manualDistanceKm = null);
}

public class GoogleDistanceService(IConfiguration configuration, IHttpClientFactory httpClientFactory) : IGoogleDistanceService
{
    public async Task<decimal?> GetDistanceInKmAsync(string fullAddress, decimal? manualDistanceKm = null)
    {
        var apiKey = configuration["GoogleMaps:ApiKey"];
        var origin = configuration["GoogleMaps:StoreAddress"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(origin))
        {
            return manualDistanceKm;
        }

        var client = httpClientFactory.CreateClient();
        var url =
            $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(fullAddress)}&key={apiKey}";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return manualDistanceKm;
        }

        var raw = await response.Content.ReadAsStringAsync();

        try
        {
            using var json = JsonDocument.Parse(raw);
            var element = json.RootElement
                .GetProperty("rows")[0]
                .GetProperty("elements")[0];

            if (!element.TryGetProperty("distance", out var distanceElement))
            {
                return manualDistanceKm;
            }

            var distanceValue = distanceElement.GetProperty("value").GetDecimal();
            return Math.Round(distanceValue / 1000m, 2);
        }
        catch
        {
            return manualDistanceKm;
        }
    }
}

