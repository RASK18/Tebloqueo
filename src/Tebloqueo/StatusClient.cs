namespace Tebloqueo;

internal sealed class StatusClient(HttpClient httpClient)
{
    public async Task<BlockingStatus> CheckAsync(Isp isp, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(IspCatalog.Endpoint(isp), cancellationToken);
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
