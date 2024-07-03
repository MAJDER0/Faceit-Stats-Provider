using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Interfaces;
using Faceit_Stats_Provider.Models;
using Faceit_Stats_Provider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Configure HttpClient for Faceit API
builder.Services.AddHttpClient("Faceit", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://open.faceit.com/data/");
    httpClient.DefaultRequestHeaders.Add("Authorization", builder.Configuration.GetValue<string>("FaceitAPI"));
});

// Configure HttpClient for Faceit V1 API
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

builder.Services.AddSingleton<GetTotalEloRetrievesCountFromRedis>();;
builder.Services.AddSingleton<ILoadMoreMatches, LoadMoreMatchesService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
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
