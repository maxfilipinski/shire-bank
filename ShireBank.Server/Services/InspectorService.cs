﻿using System.Text;
using Grpc.Core;
using ShireBank.Shared.Protos;

namespace ShireBank.Server.Services;

public class InspectorService : Inspector.InspectorBase
{
    private readonly ILogger<InspectorService> _logger;

    public InspectorService(ILogger<InspectorService> logger)
    {
        _logger = logger;
    }

    public static bool InspectionInProgress { get; set; } = false;

    public override Task<StartInspectionResponse> StartInspection(StartInspectionRequest request, ServerCallContext context)
    {
        InspectionInProgress = true;
        _logger.LogInformation("Starting inspection...");

        return Task.FromResult(new StartInspectionResponse());
    }
    
    public override Task<FinishInspectionResponse> FinishInspection(FinishInspectionRequest request, ServerCallContext context)
    {
        InspectionInProgress = false;
        _logger.LogInformation("Inspection completed");

        return Task.FromResult(new FinishInspectionResponse());
    }
    
    public override async Task GetFullSummary(IAsyncStreamReader<GetFullSummaryRequest> requestStream, IServerStreamWriter<GetFullSummaryResponse> responseStream, ServerCallContext context)
    {
        if (!InspectionInProgress)
            throw new RpcException(new Status(StatusCode.Aborted, "Inspection is not in progress"));

        var sb = new StringBuilder();

        var readTask = Task.Run(async () =>
        {
            await foreach (var message in requestStream.ReadAllAsync())
            {
                sb.AppendLine(message.ToString());
            }
        });

        while (!readTask.IsCompleted)
        {
            await responseStream.WriteAsync(new GetFullSummaryResponse
            {
                Summary = sb.ToString()
            });
        }
    
        // try
        // {
        //     await responseStream.WriteAsync(new GetFullSummaryResponse
        //     {
        //         Summary = requestStream.ToString()
        //     });
        //     await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Error in getting the full summary");
        //     throw;
        // }
    }
}