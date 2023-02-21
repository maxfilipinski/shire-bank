using Grpc.Core;
using Grpc.Net.Client;
using NLog;
using ShireBank.Shared.Constants;
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
        
        await inspector.StartInspectionAsync(new StartInspectionRequest());

        var cancelToken = new CancellationTokenSource();
        var summary = inspector.GetFullSummary(new GetFullSummaryRequest(), cancellationToken: cancelToken.Token);

        await foreach(var response in summary.ResponseStream.ReadAllAsync(cancelToken.Token))
            Logger.Info($"Inspected the operation: {response.Summary}");

        Console.ReadKey();
        cancelToken.Cancel();

        await inspector.FinishInspectionAsync(new FinishInspectionRequest());
        Console.ReadKey();
    }
}