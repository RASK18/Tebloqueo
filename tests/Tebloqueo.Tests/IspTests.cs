using System.Net;

namespace Tebloqueo.Tests;

public sealed class IspTests
{
    [Theory]
    [InlineData((int)Isp.Todos, "https://hayahora.futbol/estado/blocked-any.txt")]
    [InlineData((int)Isp.Movistar, "https://hayahora.futbol/estado/blocked-movistar.txt")]
    [InlineData((int)Isp.Digi, "https://hayahora.futbol/estado/blocked-digi.txt")]
    [InlineData((int)Isp.Vodafone, "https://hayahora.futbol/estado/blocked-vodafone.txt")]
    [InlineData((int)Isp.Orange, "https://hayahora.futbol/estado/blocked-orange.txt")]
    [InlineData((int)Isp.Masmovil, "https://hayahora.futbol/estado/blocked-masmovil.txt")]
    public void EndpointReturnsExpectedUrl(int ispValue, string expected)
    {
        Assert.Equal(expected, IspCatalog.Endpoint((Isp)ispValue));
    }

    [Theory]
    [InlineData((int)Isp.Todos, "https://hayahora.futbol/estado/blocked-any.txt")]
    [InlineData((int)Isp.Digi, "https://hayahora.futbol/estado/blocked-digi.txt")]
    public async Task StatusClientRequestsSelectedIspEndpoint(int ispValue, string expected)
    {
        var handler = new RecordingHandler();
        using var httpClient = new HttpClient(handler);
        var client = new StatusClient(httpClient);

        var result = await client.CheckAsync((Isp)ispValue);

        Assert.Equal(expected, handler.RequestUri?.AbsoluteUri);
        Assert.Equal(BlockingState.No, result.State);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("1.1.1.1")
            });
        }
    }
}
