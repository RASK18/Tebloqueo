export const RELEASE_API_URL = "https://api.github.com/repos/RASK18/Tebloqueo/releases/latest";
export const FALLBACK_DOWNLOAD_URL = "https://github.com/RASK18/Tebloqueo/releases/latest/download/Tebloqueo.exe";

export async function resolveLatestRelease(fetchImplementation, signal) {
  try {
    const response = await fetchImplementation(RELEASE_API_URL, {
      headers: { Accept: "application/vnd.github+json" },
      signal
    });

    if (!response.ok) {
      throw new Error(`GitHub respondió con ${response.status}.`);
    }

    return { ...getReleaseDetails(await response.json()), isFallback: false };
  } catch {
    return {
      downloadUrl: FALLBACK_DOWNLOAD_URL,
      version: null,
      isFallback: true
    };
  }
}

export function getReleaseDetails(release) {
  const asset = Array.isArray(release?.assets)
    ? release.assets.find((candidate) => candidate?.name === "Tebloqueo.exe")
    : null;

  if (!asset?.browser_download_url) {
    throw new Error("La última release no contiene Tebloqueo.exe.");
  }

  return {
    downloadUrl: asset.browser_download_url,
    version: normalizeVersion(release.tag_name)
  };
}

export function normalizeVersion(tagName) {
  if (typeof tagName !== "string" || tagName.trim().length === 0) {
    return null;
  }

  return tagName.trim().replace(/^v/i, "");
}
