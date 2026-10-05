import type { CardDetector } from "#card-scanner/detector";
import type { CardMatcher } from "#card-scanner/matcher";
import { resolveScannerIndex } from "~/services/scanner/ScannerIndexSource";

const MODEL_URL = "/scanner/models/swu-detect-fp16.onnx";
const INDEX_BASE = "/scanner/index";

interface ScannerModels {
  detector: CardDetector;
  matcher: CardMatcher;
}

// Loading takes seconds and ~50 MB, so the models are kept for the app's lifetime rather than per visit.
let loading: Promise<ScannerModels> | undefined;

async function loadModels(scannerUrl: string | undefined): Promise<ScannerModels> {
  const [{ CardDetector }, { CardMatcher }] = await Promise.all([
    import("#card-scanner/detector"),
    import("#card-scanner/matcher"),
  ]);

  const detector = new CardDetector(768);
  await detector.load(MODEL_URL);
  // The matcher inherits whichever wasm proxy mode the detector settled on, so these load in order.
  const matcher = new CardMatcher();
  const index = await resolveScannerIndex(scannerUrl, INDEX_BASE);
  await matcher.load(index.base, index.fetchFile);

  return { detector, matcher };
}

export function useScannerModels() {
  if (!loading) {
    loading = loadModels(useRuntimeConfig().public.SCANNER_URL as string | undefined).catch((error) => {
      loading = undefined;
      throw error;
    });
  }
  return loading;
}
