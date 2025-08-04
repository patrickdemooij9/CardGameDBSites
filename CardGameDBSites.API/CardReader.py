import json
import cv2
import easyocr
import re
import os
import argparse
import base64


def clean_text(text, ignore_whitespace, ignore_special_chars):
    if ignore_whitespace:
        text = text.strip()
    if ignore_special_chars:
        text = re.sub(r'[^a-zA-Z0-9]', '', text)
    return text


def extract_region(image, config, image_width, image_height):
    x1 = int((config['PosX1'] / 100) * image_width)
    x2 = int((config['PosX2'] / 100) * image_width)
    y1 = int((config['PosY1'] / 100) * image_height)
    y2 = int((config['PosY2'] / 100) * image_height)
    return image[y1:y2, x1:x2]


def apply_conditions(results_dict, conditions):
    if not conditions:
        return True
    for key, expected_values in conditions.items():
        actual_value = results_dict.get(key, "")
        if actual_value not in expected_values:
            return False
    return True


def process_image(image_path, config_data, reader):
    print(f"\n[Processing] {os.path.basename(image_path)}")

    # Load and upscale image
    image = cv2.imread(image_path)
    h, w = image.shape[:2]
    scale_factor = 2
    image = cv2.resize(image, (int(w * scale_factor), int(h * scale_factor)), interpolation=cv2.INTER_CUBIC)
    h, w = image.shape[:2]

    results = {}

    for field in config_data:
        alias = field["Alias"]

        if field["Type"] == "OCR":
            # Extract and preprocess region
            region = extract_region(image, field, w, h)

            # Save region image
            output_dir = os.path.join("output_regions", os.path.splitext(os.path.basename(image_path))[0])
            os.makedirs(output_dir, exist_ok=True)
            region_path = os.path.join(output_dir, f"{alias}.png")
            cv2.imwrite(region_path, region)

            # OCR
            text_result = reader.readtext(region, detail=0, paragraph=True)
            text = " ".join(text_result) if text_result else ""

            cleaned_text = clean_text(
                text,
                field.get("IgnoreWhitespace", False),
                field.get("IgnoreSpecialCharacters", False)
            )

            results[alias] = cleaned_text

        if field["Type"] == "Constant":
            results[alias] = field["Constant"]

    # Filter fields based on conditions
    final_results = {}
    for field in config_data:
        alias = field["Alias"]
        if apply_conditions(results, field.get("Conditions")):
            final_results[alias] = results[alias]

    # Read original image as base64
    with open(image_path, "rb") as image_file:
        image_bytes = image_file.read()
        base64_str = base64.b64encode(image_bytes).decode("utf-8")

    return final_results, base64_str


def main():
    parser = argparse.ArgumentParser(description="OCR reader for multiple images using a JSON config.")
    parser.add_argument("config_path", help="Path to the config JSON file")
    parser.add_argument("images", nargs="+", help="Paths to one or more image files")
    args = parser.parse_args()

    # Load configuration
    with open(args.config_path, 'r') as f:
        config_data = json.load(f)

    # Initialize OCR reader
    reader = easyocr.Reader(['en'])

    all_results = []

    # Process each image
    for image_path in args.images:
        result, image_base64 = process_image(image_path, config_data, reader)
        result_with_filename = {"image": os.path.basename(image_path), "image_base64": image_base64}
        result_with_filename.update(result)
        all_results.append(result_with_filename)

    # Write results as a JSON array
    os.makedirs("output_results", exist_ok=True)
    output_path = os.path.join("output_results", "results.json")

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(all_results, f, indent=4, ensure_ascii=False)

    print(f"\n All results saved to {output_path}")


if __name__ == "__main__":
    main()
