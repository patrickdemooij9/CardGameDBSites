# SWU Card Scanner

Point a camera at a table of Star Wars: Unlimited cards and have the app find them.
Inspired by [riftbound-scanner](https://github.com/Nekoraru22/riftbound-scanner), rebuilt
around the `api.sw-unlimited-db.com` card data.

**Milestone 1 — detect cards in a live webcam feed.** Done.
**Milestone 2 — identify which card each detection is.** Done, ~90% top-1.

```
SWUCardScanner/
├── model/                     Python: card art -> dataset -> detector, and -> card index
│   ├── scrape_cards.py        pull every unique card image from the API
│   ├── build_dataset.py       composite them into synthetic YOLO-pose scenes
│   ├── preview_dataset.py     draw the labels back on, to check them
│   ├── train_colab.ipynb      train YOLO11s-pose on a Colab GPU, export ONNX
│   ├── export_embedder.py     export the MobileNetV3 trunk used by the descriptor
│   ├── descriptors.py         the descriptors, and the PCA fit - shared by everything
│   ├── build_index.py         embed all cards -> card-index.bin + card-pca.bin
│   ├── compare_descriptors.py bench descriptors against identical queries
│   ├── eval_matcher.py        identification accuracy vs corner error
│   ├── eval_corner_error.py   how much corner error the real detector actually makes
│   ├── parity_fixture.py      fixture proving the browser agrees with Python
│   └── backgrounds/           drop your own table/playmat photos here
└── web/                       Vite + Vue 3 + Tailwind, onnxruntime-web in the browser
    ├── src/lib/detector.ts    letterbox -> ONNX -> decode -> NMS
    ├── src/lib/unwarp.ts      homography from four corners, plus a blur check
    ├── src/lib/matcher.ts     descriptor -> PCA -> nearest neighbour over the index
    └── src/components/CameraScanner.vue
```

## How it works

Two layers, deliberately kept apart:

1. **Detection** — a YOLO11s-pose model locates every card and regresses its four
   corners as keypoints. One class, `card`. It never learns *which* card it is looking
   at, so **a new set never requires retraining**.
2. **Identification** — the four corners un-warp the card back to a flat 256×256 image,
   which becomes a vector; the nearest of ~3,100 reference vectors wins. A new set means
   re-running the scraper and `build_index.py` — minutes on a laptop, no GPU.

Splitting them this way is what makes the thing maintainable: the expensive, GPU-bound
part is trained once, and keeping up with new releases is a data refresh.

### Why corners and not just boxes

An axis-aligned box around a card lying at an angle contains a lot of table. The four
corners let the next milestone apply a perspective transform and recover a flat, upright
card image before matching — which is the difference between matching against clean
reference art and matching against a trapezoid.

The corners are ordered **top-left, top-right, bottom-right, bottom-left in the card's
own frame**, so the ordering also encodes rotation. Two consequences:

- Horizontal/vertical flip augmentation must stay **off** during training. A flip swaps
  the corner ordering and destroys the orientation signal. `train_colab.ipynb` sets
  `fliplr=0, flipud=0`; don't turn them back on.
- In the web app each corner is drawn in its own colour (red/green/blue/yellow). If the
  colours rotate with the card, orientation is being learnt correctly.

### Training data

There is no annotated dataset of photographed SWU cards, so `build_dataset.py` makes one:
card PNGs composited onto randomised backgrounds under random perspective, then degraded
with blur, noise, JPEG artefacts, colour casts and vignetting.

Details that matter:

- The API serves cards as **RGBA PNGs whose alpha already carries the rounded corners**,
  and the card rectangle spans the full image — so the four corners of the source image
  *are* the four corners of the card. No corner annotation is needed.
- Leaders and bases are landscape; units, events and upgrades are portrait. Leaders are
  double-sided, and `backImageUrl` gives the landscape leader side.
- Roughly 35% of cards get a **sleeve** with a shifted border and glare. The label stays
  on the printed card, not the sleeve.
- Scenes come in four modes — scattered, close-up, binder-grid and overlapping fan — so
  the model sees both a single card held up and a messy table.
- **Distractor rectangles** are painted in. Without them the model learns "any bright
  quad is a card" and fires on phones, coasters and notebooks.
- Cards occluded past 45% lose their label; cards that would fall outside the frame are
  rejected and retried smaller, because corner regression is undefined off-screen.

The single highest-value improvement is dropping 20–50 photos of **your own** table,
playmat and lighting into `model/backgrounds/`. They get used for ~55% of scenes and beat
any amount of procedural variety.

## Running it

### 1. Train the detector

There is no NVIDIA GPU on this machine, so training runs in Colab. Open
`model/train_colab.ipynb` in [Google Colab](https://colab.research.google.com/), set
**Runtime → Change runtime type → T4 GPU**, and work down the cells. It uploads only the
two Python scripts; the card art and the dataset are regenerated on the Colab machine, so
nothing large crosses the wire.

The notebook ends by downloading `swu-detect.onnx`. Put it in `web/public/models/`.

Expect roughly: 5 min to download art, 10–15 min to render 8000 scenes, and **~3.5 min
per epoch on a T4 — so about 4.5 h for the full 80.** That is long enough that Colab may
reclaim the session, so point `project=` at a mounted Drive folder and resume a dropped
run with `YOLO('runs/swu-detect/weights/last.pt').train(resume=True)`.

### 1b. Build the card index

Identification needs a reference vector per card. On a laptop, no GPU:

```bash
python model/scrape_cards.py            # ~3,100 images, a few minutes
python model/export_embedder.py --size 256 --pool 2
python model/build_index.py --augment 8 # writes web/public/index/, ~13.5 MB
```

Re-run the scraper and `build_index.py` when a set drops. The detector is untouched.

To try the data pipeline locally first:

```bash
pip install -r model/requirements.txt
python model/scrape_cards.py --limit 400
python model/build_dataset.py --scenes 60 --out model/dataset_smoke
python model/preview_dataset.py --dataset model/dataset_smoke --n 12
```

Then open `model/preview.jpg` and check the magenta quads sit on the card edges and the
corner colours rotate with each card.

### 2. Run the app

```bash
npm --prefix SWUCardScanner/web install
npm --prefix SWUCardScanner/web run dev
```

Then open http://localhost:5177. Without a model in `web/public/models/` the app still
starts and says so — you can also load any `.onnx` from disk with the file picker, which
is the quickest way to compare two training runs.

To test on a phone, `npm run dev:https` serves over a self-signed cert on your LAN
address; `getUserMedia` refuses to run on plain http anywhere except localhost.

### In the Android app

The Nuxt frontend imports `web/src/lib` directly for its admin-only scanner page (see
`CardGameDBSites.Frontend/agents.md`), and types it against `web/types/`. Run
`npm run types:lib` after changing the lib's exports.

### Notes on the web runtime

- Backends are tried **WebGPU → wasm worker → wasm**. On an Intel Iris Xe, WebGPU runs
  detection at ~130 ms/frame against ~2,000 ms for single-precision wasm — 15× faster at
  full precision, which makes the int8 export unnecessary. Identification adds ~55 ms per
  card, capped at four cards per frame.
- Both models are **warmed up on load**. WebGPU compiles a shader per operator on first
  use, about 2.8 s; without a warm-up pass that lands on the first real frame and looks
  like a hang.
- `vite.config.ts` sets COOP/COEP headers to unlock `SharedArrayBuffer` and multi-threaded
  wasm. **Any production host needs those same two headers**, or the wasm fallback
  silently drops to a single thread.
- Input buffers are reallocated when detached. In proxy mode onnxruntime posts the input
  tensor to its worker as a *transferable*, which neuters the buffer on the main thread —
  reusing it then fails with a size/length mismatch on the second frame.
- `onnxruntime-web` is excluded from Vite dep pre-bundling. It locates its wasm with
  `new URL(..., import.meta.url)`, and esbuild rewrites those URLs during pre-bundling,
  which makes the wasm 404 at runtime.
- The decoder handles both `[1, 13, N]` and `[1, N, 13]` output layouts, and both 2-value
  and 3-value keypoints, so a differently-exported model won't silently produce garbage.

## Milestone 2 — identification

Every design choice here came from `compare_descriptors.py` rather than intuition, and
the measurements contradicted the obvious plan.

### The descriptor is a hybrid, and embeddings alone lose

Benchmarked against identical degraded queries over all 3,118 cards, at three levels of
simulated corner error:

| descriptor | dim | 0% | 2% | 4% |
|---|---|---|---|---|
| `pixels:48` — normalised colour grid | 6912 | 97.0% | 89.0% | 59.0% |
| `pixels:32` | 3072 | 94.0% | 86.7% | 61.3% |
| `onnx` — MobileNetV3 embedding | 2304 | 72.7% | 66.7% | 55.0% |
| `hybrid` — both | 5376 | 95.7% | 90.0% | 76.0% |
| **`hybrid` + PCA-256, whitened** | **256** | **95.3%** | **91.3%** | **82.3%** |

(The last row is scored by card rather than by index row; the others predate that fix and
are marginally pessimistic. The comparison between descriptors is unaffected — every row
was measured against identical queries.)

A plain pixel grid beats the CNN embedding by 24 points. Average-pooled ImageNet
features are *semantic* — they describe what kind of thing a picture shows and discard
instance detail on purpose, which is the opposite of matching near-duplicate art. The
first attempt (global pooling, 576-d) managed 57%.

The embedding still earns its place: at 4% corner error the hybrid holds 81% where pure
pixels collapse to 59%. Convolutional features pool over neighbourhoods, so they tolerate
misalignment that a fixed grid cannot. Pixels supply specificity, the CNN supplies
tolerance.

Whitened PCA then makes the index 21× smaller *and* more robust, because it suppresses
the few high-variance directions — overall brightness, the shared card frame — that
otherwise dominate every dot product.

### Reference augmentation

References are clean scans; queries are photographs. `build_index.py --augment N`
describes each card N+1 times — once clean, N times under camera-like perturbation
(white balance, gamma, a small affine nudge, directional glare, blur, noise, JPEG) — and
averages the result into a centroid that sits nearer to any particular photo.

| jitter | clean references | `--augment 8` |
|---|---|---|
| 0% | 95.3% | **99.0%** |
| 2% | 91.3% | **97.0%** |
| 4% | 82.3% | **91.7%** |
| 6% | 65.0% | **76.7%** |

At the 2% operating point that removes two-thirds of the errors. It also widened the
separation the rejection gates depend on: margin on correct matches rose 0.303 → 0.351
while margin on wrong ones *fell* 0.024 → 0.014, so the gap went from about 13× to 25×.
That second number matters — a change that merely inflated every similarity would have
raised both.

Costs nothing at run time; the index is the same size and the query path is unchanged.
Only the build is slower (~10 min rather than ~2 for 3,118 cards).

**Caveat.** `augment_reference()` deliberately avoids reusing `build_dataset.degrade()`,
so the index is not simply memorising the query generator. But both model the same
physical effects, so treat this as suggestive rather than settled — the real test is a
camera. `--augment 0` reverts.

### Glare, and a fix that didn't work

Veiling glare — light reflecting off the card's gloss into the lens — is the dominant
real-world failure. It is **additive**, so it destroys dark cards first: the same +40 is
a 20% shift on a bright card and a 4× shift on a near-black one. A dark card under a lamp
can collapse from ~0.7 similarity to ~0.26, with the correct card still ranked first but
the margin too small to assert.

The obvious fix is to high-pass the pixel grid: per-channel normalisation cancels a
*uniform* colour cast, but glare is a *gradient*, and subtracting a heavily smoothed copy
would remove it. `--highpass N` implements that, and on a non-augmented index it looked
excellent — 100% vs 96.5% at zero jitter, 96.0% vs 90.0% under simulated glare.

It does not survive contact with the augmented index:

| 2% jitter, 250 queries | augment only | + highpass 6 |
|---|---|---|
| clean | 96.8% | 97.2% |
| with glare | 96.0% | 96.8% |
| **darkest 25% of cards, with glare** | **98.5%** | **98.0%** |
| 4% jitter, clean | 91.7% | 85.0% |
| margin on *wrong* matches | 0.012 | 0.026 |

No gain even on the exact failure mode it targets, while doubling the margin on wrong
matches (weakening the rejection gate) and costing six points when corner error is high —
removing low frequencies also removes the misalignment tolerance they provided. Reference
augmentation already absorbs this much glare.

So it ships **off**. The flag stays because the synthetic glare model is additive only,
while real veiling glare also scatters and reduces contrast multiplicatively; if real
cards disagree with this table, `--highpass 6` is one rebuild away.

The practical fix for a glare failure is to move the light off-axis or tilt the card.

### Which column applies to you

`eval_corner_error.py` runs the trained detector over fresh scenes and reports corner
displacement in the same units as the jitter sweep. Measured: **median 1.53%, p90 2.6%,
p95 3.0%**. So read the 2% column — around 90% top-1 per frame, and the app votes across
frames, so a card held in view settles well above that.

### Rejecting bad matches

A confidently wrong identification is worse than none, so three gates run before any
match is asserted: a Laplacian-variance blur check on the crop, a minimum similarity,
and a minimum *margin* over the runner-up. Correct matches average a margin near 0.30
against 0.02 for wrong ones, which is a wide enough separation for the margin test to do
most of the work.

`identify()` therefore never returns null — it returns the best candidate along with
`accepted` and a `reason`. "No match" has three quite different causes needing three
different fixes, and collapsing them into a silent null makes the thing impossible to
diagnose from the outside.

### Diagnostics

Turn on **Diagnostics** in the sidebar to see the un-warped 256×256 crop the matcher was
actually given, its score, margin and sharpness against their thresholds, the top three
candidates, and live threshold sliders. Freeze the frame first.

This is the fastest way to tell the failure modes apart. Glare and occluded corners are
obvious the moment you look at the crop, and a near-miss between two similar cards looks
completely different from a crop that never had a chance.

Real camera scores run well below the synthetic benchmark — roughly 0.30 against
0.60–0.85 on generated queries. The gap is the domain shift between clean reference scans
and photographs, so calibrate the thresholds from what the diagnostics panel shows on
real frames rather than from the numbers above.

### Python and the browser must agree, exactly

The index is built in Python and searched in JavaScript. Any difference between the two
descriptor implementations degrades matching in a way that looks like a bad model rather
than a bug. Two consequences worth preserving:

- Everything that could differ is written out explicitly — box downscaling instead of
  canvas scaling, a `[1,2,1]/4` binomial blur instead of a Gaussian whose kernel size and
  border handling are library conventions, and a hand-written bilinear perspective warp.
- `parity_fixture.py` renders scenes, records Python's answer, and the browser re-runs
  the same images. This caught a real bug: the pixel grid was being built from OpenCV's
  BGR on one side and RGB on the other, which still matched 7/8 but with scores up to
  0.26 too low. Current agreement is 8/8 with a mean score difference of 0.012, the
  residual being cv2's warp against the JavaScript one.

Run it after any change to `descriptors.py` or `matcher.ts`.

### The variant problem, and the trap it sets

The API returns ~8.7k card variants but only 3,118 unique images — hyperspace and
showcase variants point at the same artwork as the standard printing. **No purely visual
method can separate them.** Identification resolves to a `baseId`; distinguishing the
variant needs a different signal (set symbol, rarity marker, foiling) or a question to
the user.

That much was expected. What wasn't: **46% of index rows share a `baseId` with another
row.** Promos and alternate printings keep separate media files with near-identical art —
Vader's Lightsaber has an organized-play promo sitting at 0.876 similarity to its
standard printing.

Naively, the margin test then rejects those cards for being "too similar to another
card", when the other card is *itself*. Measured on four rendered queries of Vader's
Lightsaber, the margin against the next **row** was 0.013, 0.087, 0.164 and 0.005 — half
of them below the 0.06 threshold. Against the next **different card** it is 0.39 to 0.67.

So margin is measured against the best candidate with a different `baseId`, and the
displayed candidate list is collapsed by card too. `TOP_K` in `matcher.ts` must stay
above the largest number of rows any single card owns (currently 7), or the search can
run out of candidates before finding a genuine rival.

`eval_matcher.py` scores by `baseId` for the same reason — counting a promo as a miss
when the standard printing was the truth measures the wrong thing.

### Where to go next

- The card name is printed on every card, and this repo already has an OCR harness in
  [CardReader.py](../CardGameDBSites.API/CardReader.py). Combining a weak visual match
  with an OCR'd name should beat either alone, particularly for the ~10% tail.
- The index is 13.5 MB. int8 quantisation of the PCA matrix and the index would cut that
  to roughly 4 MB.
