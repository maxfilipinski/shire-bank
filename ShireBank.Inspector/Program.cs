using Grpc.Core;
using Grpc.Net.Client;
using NLog;
using ShireBank.Shared;
using ShireBank.Shared.Protos;

namespace ShireBank.Inspector;

internal static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private static async Task Main()
    {
        using var channel = GrpcChannel.ForAddress(Constants.BankFullAddress, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });

        var inspector = new ShireBank.Shared.Protos.Inspector.InspectorClient(channel);
        Logger.Info("Started inspecting...");
        await inspector.StartInspectionAsync(new StartInspectionRequest());

        var cancelToken = new CancellationTokenSource();
        var summary = inspector.GetFullSummary(cancellationToken: cancelToken.Token);

        await foreach(var response in summary.ResponseStream.ReadAllAsync(cancelToken.Token))
            Logger.Info($"Inspected {response.Summary}");

        Console.ReadKey();
        cancelToken.Cancel();

        await inspector.FinishInspectionAsync(new FinishInspectionRequest());
        Logger.Info("Inspecting completed");

        Console.ReadKey();
    }
}