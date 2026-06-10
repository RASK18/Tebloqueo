using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tebloqueo;

internal sealed record UpdateManifest(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("assetUrl")] string AssetUrl,
    [property: JsonPropertyName("sha256")] string Sha256);

internal sealed record UpdateCheckResult(string? DownloadedPath = null, string? Error = null);

internal sealed class UpdateService(HttpClient httpClient, Version currentVersion)
{
    internal const string ManifestUrl = "https://github.com/RASK18/Tebloqueo/releases/latest/download/update.json";
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<UpdateCheckResult> CheckAndDownloadAsync(CancellationToken cancellationToken = default)
    {
        string? temporaryPath = null;

        try
        {
            using var manifestResponse = await httpClient.GetAsync(ManifestUrl, cancellationToken);
            manifestResponse.EnsureSuccessStatusCode();

            await using var manifestStream = await manifestResponse.Content.ReadAsStreamAsync(cancellationToken);
            var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(
                manifestStream,
                SerializerOptions,
                cancellationToken);

            var validationError = ValidateManifest(manifest, currentVersion, out var availableVersion);
            if (validationError is not null)
            {
                return new UpdateCheckResult(Error: validationError);
            }

            if (availableVersion! <= currentVersion)
            {
                return new UpdateCheckResult();
            }

            temporaryPath = Path.Combine(Path.GetTempPath(), $"Tebloqueo-update-{Guid.NewGuid():N}.exe");
            using var assetResponse = await httpClient.GetAsync(manifest!.AssetUrl, cancellationToken);
            assetResponse.EnsureSuccessStatusCode();

            await using (var source = await assetResponse.Content.ReadAsStreamAsync(cancellationToken))
            await using (var destination = File.Create(temporaryPath))
            {
                await source.CopyToAsync(destination, cancellationToken);
            }

            string actualHash;
            await using (var downloadedFile = File.OpenRead(temporaryPath))
            {
                actualHash = Convert.ToHexString(await SHA256.HashDataAsync(downloadedFile, cancellationToken));
            }

            if (!actualHash.Equals(manifest.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(temporaryPath);
                return new UpdateCheckResult(Error: "El hash SHA-256 de la actualización no coincide.");
            }

            return new UpdateCheckResult(DownloadedPath: temporaryPath);
        }
        catch (Exception exception)
        {
            if (temporaryPath is not null)
            {
                TryDelete(temporaryPath);
            }

            return new UpdateCheckResult(Error: exception.Message);
        }
    }

    internal static string? ValidateManifest(UpdateManifest? manifest, Version currentVersion, out Version? availableVersion)
    {
        availableVersion = null;

        if (manifest is null ||
            !System.Version.TryParse(manifest.Version, out availableVersion) ||
            !Uri.TryCreate(manifest.AssetUrl, UriKind.Absolute, out var assetUri) ||
            assetUri.Scheme != Uri.UriSchemeHttps ||
            !assetUri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) ||
            !assetUri.AbsolutePath.StartsWith("/RASK18/Tebloqueo/releases/download/", StringComparison.OrdinalIgnoreCase) ||
            !assetUri.AbsolutePath.EndsWith("/Tebloqueo.exe", StringComparison.OrdinalIgnoreCase) ||
            manifest.Sha256.Length != 64 ||
            !manifest.Sha256.All(Uri.IsHexDigit))
        {
            return "El manifiesto de actualización no es válido.";
        }

        return null;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // The next cleanup pass will remove abandoned update files.
        }
    }
}
