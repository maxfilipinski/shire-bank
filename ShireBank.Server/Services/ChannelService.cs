// https://deniskyashif.com/2019/12/08/csharp-channels-part-1/
// https://learn.microsoft.com/en-us/dotnet/core/extensions/channels
using System.Threading.Channels;
using ShireBank.Server.Services.Interfaces;

namespace ShireBank.Server.Services;

public class ChannelService : IChannelService
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public async Task WriteToChannelAsync(string request, CancellationToken token)
    {
        await _channel.Writer.WriteAsync(request, token);
    }

    public IAsyncEnumerable<string> ReadFromChannelAsync(CancellationToken token)
    {
        return _channel.Reader.ReadAllAsync(token);
    }
}