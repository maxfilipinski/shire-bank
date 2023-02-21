using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using ShireBank.Repository.Data;
using ShireBank.Repository.Repositories;
using ShireBank.Repository.Repositories.Interfaces;
using ShireBank.Server.Interceptors;
using ShireBank.Server.Services;
using ShireBank.Server.Services.Interfaces;
using ShireBank.Shared.Constants;

var logger = LogManager
    .Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddGrpc(options => options.Interceptors.Add<InspectorInterceptor>());
    builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("BankDatabase")));
    builder.Services.AddTransient<IBankAccountRepository, BankAccountRepository>();
    builder.Services.AddTransient<IBankTransactionRepository, BankTransactionRepository>();
    builder.Services.AddSingleton<IChannelService, ChannelService>();

    builder.WebHost.UseUrls(Constants.BankFullAddress);
    builder.Host.UseNLog();

    var app = builder.Build();

    using (var serviceScope = app.Services.CreateScope())
    {
        var dataContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
        await dataContext.Database.MigrateAsync();
    }

    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<CustomerService>();
        endpoints.MapGrpcService<InspectorService>();
    });

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Server stopped working due to exception");
    throw;
}