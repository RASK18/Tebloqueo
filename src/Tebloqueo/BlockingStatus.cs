using System.Net;

namespace Tebloqueo;

internal enum BlockingState
{
    Unknown,
    No,
    Yes
}

internal sealed record BlockingStatus(BlockingState State, int? IpCount, string? Error = null)
{
    internal const int BlockingThreshold = 10;

    public static BlockingStatus FromContent(string content)
    {
        var count = 0;

        foreach (var rawLine in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (!IPAddress.TryParse(line, out _))
            {
                return new BlockingStatus(BlockingState.Unknown, null, $"Línea inválida: {line}");
            }

            count++;
        }

        return new BlockingStatus(count > BlockingThreshold ? BlockingState.Yes : BlockingState.No, count);
    }

    public static BlockingStatus FromError(Exception exception) =>
        new(BlockingState.Unknown, null, exception.Message);
}
