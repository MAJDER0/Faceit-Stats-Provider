using Faceit_Stats_Provider.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


    var proxyList = builder.Configuration.GetSection("Proxies").Get<List<ProxyConfig>>();

    // Select a random proxy from the list
    var randomProxy = proxyList[new Random().Next(proxyList.Count)];

    // Build the proxy URL
    var proxyURL = $"http://{randomProxy.Address}:{randomProxy.Port}";

    // Create the WebProxy and configure the HttpClientHandler
    WebProxy webProxy = new WebProxy
    {
        Address = new Uri(proxyURL),
        Credentials = new NetworkCredential(
            userName: randomProxy.Username,
            password: randomProxy.Password
        )
    };

    // Configure the HttpClientHandler for both Faceit and FaceitV1
    HttpClientHandler httpClientHandler = new HttpClientHandler
    {
        Proxy = webProxy
    };

    HttpClient faceitClient = new HttpClient(httpClientHandler);
    HttpClient faceitV1Client = new HttpClient(httpClientHandler);

    // Configure Faceit HttpClient
    builder.Services.AddHttpClient("Faceit", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://open.faceit.com/data/");
        httpClient.DefaultRequestHeaders.Add("Authorization", builder.Configuration.GetValue<string>("FaceitAPI"));
    });

    // Configure FaceitV1 HttpClient
    builder.Services.AddHttpClient("FaceitV1", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
    });

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();