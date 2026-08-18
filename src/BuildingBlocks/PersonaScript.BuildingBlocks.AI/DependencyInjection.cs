using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Configuration;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.AI.Providers;
using PersonaScript.BuildingBlocks.AI.Resilience;
using Polly;
using Polly.Retry;

namespace PersonaScript.BuildingBlocks.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAIBuildingBlock(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LLMOptions>(configuration.GetSection(LLMOptions.SectionName));
        services.AddSingleton<ILLMJsonParser, LLMJsonParser>();

        // Resiliência HTTP Polly: Retry 3 vezes com Backoff Exponencial e Jitter
        var resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .Build();

        services.AddHttpClient<OpenAILLMProvider>()
            .AddResilienceHandler("OpenAiResilience", (builder, _) =>
            {
                builder.AddPipeline(resiliencePipeline);
            });

        services.AddHttpClient<GeminiLLMProvider>()
            .AddResilienceHandler("GeminiResilience", (builder, _) =>
            {
                builder.AddPipeline(resiliencePipeline);
            });

        services.AddHttpClient<AnthropicLLMProvider>()
            .AddResilienceHandler("AnthropicResilience", (builder, _) =>
            {
                builder.AddPipeline(resiliencePipeline);
            });

        services.AddTransient<MockLLMProvider>();

        // Registra ILLMProvider principal envelopado com o FallbackDecorator
        services.AddTransient<ILLMProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LLMOptions>>().Value;
            var jsonParser = sp.GetRequiredService<ILLMJsonParser>();
            var logger = sp.GetService<ILogger<FallbackLLMProviderDecorator>>();

            var primaryProvider = CreateProvider(options.PrimaryProvider, sp, options, jsonParser);

            var fallbackProviders = new List<ILLMProvider>();
            foreach (var fallbackType in options.FallbackProviders)
            {
                if (fallbackType != options.PrimaryProvider)
                {
                    fallbackProviders.Add(CreateProvider(fallbackType, sp, options, jsonParser));
                }
            }

            return new FallbackLLMProviderDecorator(primaryProvider, fallbackProviders, jsonParser, logger);
        });

        return services;
    }

    private static ILLMProvider CreateProvider(
        LLMProviderType type,
        IServiceProvider sp,
        LLMOptions options,
        ILLMJsonParser jsonParser)
    {
        return type switch
        {
            LLMProviderType.OpenAI => new OpenAILLMProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OpenAILLMProvider)),
                jsonParser,
                options.OpenAiApiKey,
                options.OpenAiEndpoint),

            LLMProviderType.GoogleGemini => new GeminiLLMProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(GeminiLLMProvider)),
                jsonParser,
                options.GeminiApiKey),

            LLMProviderType.Anthropic => new AnthropicLLMProvider(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(AnthropicLLMProvider)),
                jsonParser,
                options.AnthropicApiKey),

            LLMProviderType.Mock => sp.GetRequiredService<MockLLMProvider>(),

            _ => sp.GetRequiredService<MockLLMProvider>()
        };
    }
}
