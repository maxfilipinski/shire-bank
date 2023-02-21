namespace ShireBank.Server.Services.Interfaces;

public interface IChannelService
{
    Task WriteToChannelAsync(string request, CancellationToken token);
    IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken token);
}