import { cpSync, existsSync, mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const frontend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const source = resolve(frontend, "../SWUCardScanner/web/public");
const target = join(frontend, ".output/public/scanner");
const remote = process.env.NUXT_PUBLIC_SCANNER_URL;

const detector = "models/swu-detect-fp16.onnx";
const index = "index";

if ([detector, `${index}/card-index.json`].every((file) => existsSync(join(source, file)))) {
  mkdirSync(join(target, "models"), { recursive: true });
  cpSync(join(source, detector), join(target, detector));
  cpSync(join(source, index), join(target, index), { recursive: true });
  console.log(`[scanner] Copied local scanner assets from ${source}`);
} else if (remote) {
  await downloadFromRemote();
  console.log(`[scanner] Downloaded scanner assets from ${remote}`);
} else {
  const message =
    `[scanner] No scanner assets in ${source} and NUXT_PUBLIC_SCANNER_URL is not set, ` +
    "so the app will ship without the card scanner.";
  if (process.env.CI) {
    console.error(message);
    process.exit(1);
  }
  console.warn(message);
}

async function downloadFromRemote() {
  const { version } = JSON.parse(await download(`${remote}/index/latest.json`).then((b) => b.toString()));
  const indexBase = `${remote}/index/${version}`;
  const meta = await download(`${indexBase}/card-index.json`);
  const files = ["card-index.bin", "card-pca.bin", JSON.parse(meta.toString()).config.embedder];

  save(detector, await download(`${remote}/detector/swu-detect-fp16.onnx`));
  save(`${index}/card-index.json`, meta);
  for (const file of files) {
    save(`${index}/${file}`, await download(`${indexBase}/${file}`));
  }
}

async function download(url) {
  const response = await fetch(url);
  if (!response.ok) {
    throw new Error(`[scanner] ${url} returned ${response.status}`);
  }
  return Buffer.from(await response.arrayBuffer());
}

function save(file, contents) {
  mkdirSync(dirname(join(target, file)), { recursive: true });
  writeFileSync(join(target, file), contents);
}
