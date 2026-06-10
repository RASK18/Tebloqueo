import { FALLBACK_DOWNLOAD_URL, resolveLatestRelease } from "./release.js";

const downloadButton = document.querySelector("#download-button");
const releaseStatus = document.querySelector("#release-status");

async function loadLatestRelease() {
  if (!downloadButton || !releaseStatus) {
    return;
  }

  downloadButton.href = FALLBACK_DOWNLOAD_URL;
  const controller = new AbortController();
  const timeout = window.setTimeout(() => controller.abort(), 6000);

  try {
    const details = await resolveLatestRelease(fetch, controller.signal);
    downloadButton.href = details.downloadUrl;

    if (details.isFallback) {
      releaseStatus.classList.add("is-error");
      releaseStatus.querySelector("i").className = "bi bi-info-circle";
      releaseStatus.querySelector("span").textContent = "Descarga directa de la última release disponible";
      return;
    }

    releaseStatus.classList.add("is-ready");
    releaseStatus.querySelector("i").className = "bi bi-check-circle";
    releaseStatus.querySelector("span").textContent = details.version
      ? `Última versión disponible: ${details.version}`
      : "Última versión preparada para descargar";
  } finally {
    window.clearTimeout(timeout);
  }
}

loadLatestRelease();
