using Grpc.Core;
using ShireBank.Shared.Protos;

namespace ShireBank.Server.Services;

public class InspectorService : Inspector.InspectorBase
{
    private readonly ILogger<InspectorService> _logger;
    private readonly IChannelService _channelService;

    public InspectorService(ILogger<InspectorService> logger, IChannelService channelService)
    {
        _logger = logger;
        _channelService = channelService;
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
    
    public override async Task GetFullSummary(GetFullSummaryRequest request, IServerStreamWriter<GetFullSummaryResponse> responseStream, ServerCallContext context)
    {
        if (!InspectionInProgress)
            throw new RpcException(new Status(StatusCode.Aborted, "Inspection is not in progress"));

        try
        {
            await foreach (var operationMessage in _channelService.ReadFromChannelAsync(context.CancellationToken))
            {
                await responseStream.WriteAsync(new GetFullSummaryResponse
                {
                    Summary = operationMessage
                });
            }
            
            await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in getting the full summary");
            throw;
        }
    }
}