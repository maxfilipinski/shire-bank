// https://deniskyashif.com/2019/12/08/csharp-channels-part-1/
// https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
using System.Threading.Channels;

namespace ShireBank.Server.Services;

public interface IChannelService
{
    public Task WriteToChannelAsync(string request, CancellationToken token);
    public IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken token);
}

public class ChannelService : IChannelService
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public ChannelWriter<string> ChannelWriter => _channel.Writer;
    public ChannelReader<string> ChannelReader => _channel.Reader;

    public async Task WriteToChannelAsync(string request, CancellationToken token)
    {
        await ChannelWriter.WriteAsync(request, token);
    }

    public IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken token)
    {
        return ChannelReader.ReadAllAsync(token);
    }
}