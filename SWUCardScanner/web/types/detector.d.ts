export interface Detection {
    /** Confidence in [0,1]. */
    score: number;
    /** Axis-aligned box [x1, y1, x2, y2] in source-image pixels. */
    box: [number, number, number, number];
    /** Card corners in source-image pixels, ordered top-left, top-right, bottom-right, bottom-left. */
    corners: [number, number][];
}
export interface DetectOptions {
    /** Minimum confidence to keep a detection. */
    scoreThreshold: number;
    /** IoU above which two boxes are considered the same card. */
    iouThreshold: number;
    /** Hard cap on returned detections. */
    maxDetections: number;
}
export declare const DEFAULT_OPTIONS: DetectOptions;
/** Which onnxruntime backend actually ended up running the model. */
export type Backend = 'webgpu' | 'wasm (worker)' | 'wasm';
export declare class CardDetector {
    private session;
    private inputName;
    private canvas;
    private ctx;
    private buffer;
    /** Square input resolution the model was exported at. */
    readonly size: number;
    /** The backend in use, once a model is loaded. */
    backend: Backend | null;
    /** Milliseconds spent inside the last `detect()` call. */
    lastInferenceMs: number;
    constructor(size?: number);
    get ready(): boolean;
    /**
     * Load a model from a URL or from raw bytes (a file the user picked).
     *
     * Backends are tried best-first:
     *   1. WebGPU        - roughly 7x faster than wasm here, at full precision.
     *   2. wasm (worker) - proxied off the UI thread so the video stays smooth.
     *   3. wasm          - same thread; last resort if the proxy worker won't spawn.
     *
     * WebGPU runs unproxied on purpose. The GPU does the work while JS only submits
     * commands and awaits, so the UI thread stays free without a worker, and the WebGPU
     * backend inside a proxy worker is considerably more fragile.
     */
    load(source: string | ArrayBuffer): Promise<void>;
    /**
     * Run one throwaway inference on a blank frame.
     *
     * WebGPU compiles a shader per operator on first use, which costs several seconds.
     * Paying that here rather than on the first real camera frame is the difference
     * between a slow load and a scanner that appears to hang the moment you point it at
     * something.
     */
    private warmUp;
    dispose(): Promise<void>;
    /**
     * Letterbox a frame into the model's square input.
     *
     * Returns the scale and padding needed to map detections back to source pixels.
     */
    private preprocess;
    detect(frame: CanvasImageSource, srcW: number, srcH: number, options?: Partial<DetectOptions>): Promise<Detection[]>;
}
