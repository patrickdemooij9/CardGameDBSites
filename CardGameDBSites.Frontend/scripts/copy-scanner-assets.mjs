import { cpSync, existsSync, mkdirSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const frontend = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const source = resolve(frontend, "../SWUCardScanner/web/public");
const target = join(frontend, ".output/public/scanner");

const detector = "models/swu-detect-fp16.onnx";
const index = "index";

const missing = [detector, `${index}/card-index.json`].filter((file) => !existsSync(join(source, file)));
if (missing.length) {
  console.warn(
    `[scanner] Skipping scanner assets, missing in ${source}: ${missing.join(", ")}. ` +
      "Train the detector and run build_index.py to include the card scanner."
  );
  process.exit(0);
}

mkdirSync(join(target, "models"), { recursive: true });
cpSync(join(source, detector), join(target, detector));
cpSync(join(source, index), join(target, index), { recursive: true });
console.log(`[scanner] Copied scanner assets to ${target}`);
