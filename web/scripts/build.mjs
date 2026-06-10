import { cp, mkdir, rm } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const dist = resolve(webRoot, "dist");

const files = [
  [".nojekyll", ".nojekyll"],
  ["index.html", "index.html"],
  ["styles.css", "styles.css"],
  ["app.js", "app.js"],
  ["release.js", "release.js"],
  ["assets/logo-tebloqueo.png", "assets/logo-tebloqueo.png"],
  ["assets/blocked-network.png", "assets/blocked-network.png"],
  ["assets/tray-menu.png", "assets/tray-menu.png"],
  ["node_modules/bootstrap-icons/font/bootstrap-icons.min.css", "vendor/bootstrap-icons/bootstrap-icons.min.css"],
  ["node_modules/bootstrap-icons/font/fonts/bootstrap-icons.woff2", "vendor/bootstrap-icons/fonts/bootstrap-icons.woff2"],
  ["node_modules/bootstrap-icons/font/fonts/bootstrap-icons.woff", "vendor/bootstrap-icons/fonts/bootstrap-icons.woff"],
  ["node_modules/bootstrap-icons/LICENSE", "vendor/bootstrap-icons/LICENSE"]
];

await rm(dist, { recursive: true, force: true });

for (const [source, destination] of files) {
  const destinationPath = resolve(dist, destination);
  await mkdir(dirname(destinationPath), { recursive: true });
  await cp(resolve(webRoot, source), destinationPath);
}

console.log(`Web construida en ${dist}`);
