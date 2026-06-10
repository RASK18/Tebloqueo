namespace Tebloqueo;

internal sealed class StatusClient(HttpClient httpClient)
{
    internal const string Endpoint = "https://hayahora.futbol/estado/blocked-any.txt";

    public async Task<BlockingStatus> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(Endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return BlockingStatus.FromContent(content);
        }
        catch (Exception exception)
        {
            return BlockingStatus.FromError(exception);
        }
    }
}
