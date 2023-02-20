using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using NLog;
using ShireBank.Repository.Data;
using ShireBank.Repository.Repositories;
using ShireBank.Repository.Repositories.Interfaces;
using ShireBank.Server.Services;
using ShireBank.Shared;

var logger = LogManager
    .Setup()
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
    builder.Services.AddGrpc();
    builder.Services.AddDbContext<DataContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("BankDatabase")));
    builder.Services.AddTransient<IBankAccountRepository, BankAccountRepository>();
    builder.Services.AddTransient<IBankTransactionRepository, BankTransactionRepository>();

    builder.WebHost.UseUrls(Constants.BankFullAddress);

    var app = builder.Build();

    using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
    {
        var dataContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
        await dataContext.Database.MigrateAsync();
    }

// Configure the HTTP request pipeline.
    app.MapGrpcService<CustomerService>();
    app.MapGet("/",
        () =>
            "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Server stopped working...");
    throw;
}

// internal static class Program
// {
//     private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
//     
//     private static void Main()
//     {
//         Grpc.Core.Server server = null;
//         
//         try
//         {
//             server = new Grpc.Core.Server()
//             {
//                 Services = { Customer.BindService(new CustomerService()) },
//                 Ports =
//                 {
//                     new ServerPort("localhost", Constants.BankBasePort, ServerCredentials.Insecure)
//                 }
//             };
//             
//             server.Start();
//             Logger.Info("The server is listening on the port: " + Constants.BankBasePort);
//             Console.ReadKey();
//         }
//         catch (IOException exception)
//         {
//             Logger.Error("The server failed to start : " + exception.Message);
//             throw;
//         }
//         finally
//         {
//             server?
//                 .ShutdownAsync()
//                 .Wait();
//         }
//     }
// }