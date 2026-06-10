using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tebloqueo.Tests;

public sealed class UpdateServiceTests
{
    [Fact]
    public void ValidateManifestAcceptsExpectedGitHubReleaseAsset()
    {
        var manifest = new UpdateManifest(
            "1.0.2",
            "https://github.com/RASK18/Tebloqueo/releases/download/v1.0.2/Tebloqueo.exe",
            new string('A', 64));

        var error = UpdateService.ValidateManifest(manifest, new Version(1, 0, 1), out var availableVersion);

        Assert.Null(error);
        Assert.Equal(new Version(1, 0, 2), availableVersion);
    }

    [Theory]
    [InlineData("http://github.com/RASK18/Tebloqueo/releases/download/v1.0.2/Tebloqueo.exe")]
    [InlineData("https://example.com/RASK18/Tebloqueo/releases/download/v1.0.2/Tebloqueo.exe")]
    [InlineData("https://github.com/Other/Repo/releases/download/v1.0.2/Tebloqueo.exe")]
    public void ValidateManifestRejectsUnexpectedAssetUrls(string assetUrl)
    {
        var manifest = new UpdateManifest("1.0.2", assetUrl, new string('A', 64));

        var error = UpdateService.ValidateManifest(manifest, new Version(1, 0, 1), out _);

        Assert.NotNull(error);
    }

    [Fact]
    public async Task CheckAndDownloadVerifiesAndReturnsNewExecutable()
    {
        var executable = Encoding.UTF8.GetBytes("fake executable");
        var hash = Convert.ToHexString(SHA256.HashData(executable));
        var assetUrl = "https://github.com/RASK18/Tebloqueo/releases/download/v1.0.2/Tebloqueo.exe";
        var manifest = JsonSerializer.Serialize(new UpdateManifest("1.0.2", assetUrl, hash));
        using var httpClient = new HttpClient(new FakeHandler(manifest, executable));
        var service = new UpdateService(httpClient, new Version(1, 0, 1));

        var result = await service.CheckAndDownloadAsync();

        Assert.Null(result.Error);
        Assert.NotNull(result.DownloadedPath);
        Assert.Equal(executable, await File.ReadAllBytesAsync(result.DownloadedPath!));
        File.Delete(result.DownloadedPath!);
    }

    [Fact]
    public async Task CheckAndDownloadRejectsWrongHash()
    {
        var executable = Encoding.UTF8.GetBytes("fake executable");
        var assetUrl = "https://github.com/RASK18/Tebloqueo/releases/download/v1.0.2/Tebloqueo.exe";
        var manifest = JsonSerializer.Serialize(new UpdateManifest("1.0.2", assetUrl, new string('A', 64)));
        using var httpClient = new HttpClient(new FakeHandler(manifest, executable));
        var service = new UpdateService(httpClient, new Version(1, 0, 1));

        var result = await service.CheckAndDownloadAsync();

        Assert.Null(result.DownloadedPath);
        Assert.Contains("SHA-256", result.Error);
    }

    private sealed class FakeHandler(string manifest, byte[] executable) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContent content = request.RequestUri?.AbsoluteUri == UpdateService.ManifestUrl
                ? new StringContent(manifest, Encoding.UTF8, "application/json")
                : new ByteArrayContent(executable);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
