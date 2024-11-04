using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("Faceit", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://open.faceit.com/data/");
    httpClient.DefaultRequestHeaders.Add("Authorization", builder.Configuration.GetValue<string>("FaceitAPI"));
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddHttpClient("FaceitV1", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
});

// Add memory cache
builder.Services.AddMemoryCache();

// Add Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddTransient<GetTotalEloRetrievesCountFromRedis>();
builder.Services.AddTransient<ILoadMoreMatches, LoadMoreMatchesService>();
builder.Services.AddTransient<IFetchMaxElo, FetchMaxEloService>();
builder.Services.AddTransient<IGetMatchDetails, GetMatchDetailsService>();
builder.Services.AddTransient<IOnlyCsGoStats, OnlyCsGoStatsService>();
builder.Services.AddTransient<IToggleIncludeCsGoStats, ToggleIncludeCsGoStatsService>();
builder.Services.AddTransient<ITogglePlayer, TogglePlayerService>();
builder.Services.AddTransient<IPlayerStatistics, PlayerStatisticsService>();
builder.Services.AddTransient<IHttpClientRetryService, HttpClientRetryService>();
builder.Services.AddTransient<IRetryPolicy, RetryPolicyService>();
builder.Services.AddSingleton<HttpClientManager>();

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use response compression middleware
app.UseResponseCompression();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "loadMoreMatches",
    pattern: "PlayerStats/LoadMoreMatches",
    defaults: new { controller = "PlayerStats", action = "LoadMatches" });

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
