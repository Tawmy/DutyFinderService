using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using XivApiClient.Abstractions;

namespace XivApiClient.DepdendencyInjection;

public static class ServiceCollectionExtensions
{
    private const string BaseUrl = "https://v2.xivapi.com/api";

    extension(IServiceCollection services)
    {
        public void AddXivApiClient(string baseUrl = BaseUrl)
        {
            var settings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                })
            };

            services.AddRefitClient<IXivApiClient>(settings)
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl))
                .AddStandardResilienceHandler();
        }
    }
}