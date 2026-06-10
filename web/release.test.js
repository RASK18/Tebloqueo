import test from "node:test";
import assert from "node:assert/strict";
import { FALLBACK_DOWNLOAD_URL, getReleaseDetails, normalizeVersion, resolveLatestRelease } from "./release.js";

test("localiza Tebloqueo.exe y normaliza la versión", () => {
  const details = getReleaseDetails({
    tag_name: "v1.0.2",
    assets: [
      { name: "update.json", browser_download_url: "https://example.test/update.json" },
      { name: "Tebloqueo.exe", browser_download_url: "https://example.test/Tebloqueo.exe" }
    ]
  });

  assert.deepEqual(details, {
    downloadUrl: "https://example.test/Tebloqueo.exe",
    version: "1.0.2"
  });
});

test("falla si la release no contiene el ejecutable", () => {
  assert.throws(
    () => getReleaseDetails({ tag_name: "v1.0.2", assets: [] }),
    /no contiene Tebloqueo\.exe/
  );
});

test("normaliza versiones y tolera etiquetas ausentes", () => {
  assert.equal(normalizeVersion("V2.1.0"), "2.1.0");
  assert.equal(normalizeVersion(null), null);
});

test("usa la descarga fallback cuando GitHub no responde", async () => {
  const details = await resolveLatestRelease(async () => {
    throw new Error("Sin conexión");
  });

  assert.deepEqual(details, {
    downloadUrl: FALLBACK_DOWNLOAD_URL,
    version: null,
    isFallback: true
  });
});
