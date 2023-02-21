// https://learn.microsoft.com/en-us/aspnet/core/grpc/interceptors?view=aspnetcore-6.0
using Grpc.Core;
using Grpc.Core.Interceptors;
using ShireBank.Server.Services;

namespace ShireBank.Server.Interceptors;

public class InspectorInterceptor : Interceptor
{
    private readonly ILogger<InspectorInterceptor> _logger;
    private readonly IChannelService _channelService;

    public InspectorInterceptor(ILogger<InspectorInterceptor> logger, IChannelService channelService)
    {
        _logger = logger;
        _channelService = channelService;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            if (InspectorService.InspectionInProgress)
                await _channelService.WriteToChannelAsync(request.ToString(), context.CancellationToken);
            
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}.");
            throw;
        }
    }
}