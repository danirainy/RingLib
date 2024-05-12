# Credit to https://chatgpt.com

import argparse
import os
from psd_tools import PSDImage

def parse_psd(input_file):
    # Load the PSD file
    psd = PSDImage.open(input_file)

    # Create the output directory if it doesn't exist
    output_dir = input_file.split('.')[-2]
    os.makedirs(output_dir, exist_ok=True)

    # Extract the base name for naming files
    base_name = os.path.basename(output_dir)

    # Iterate over each layer and save it as an image
    for index, layer in enumerate(psd):
        # Only save layers that are not empty
        if layer.has_pixels():
            # Define the output file path with the base name
            output_path = os.path.join(output_dir, f"{base_name}{index}.png")
            
            # Save the layer as a PNG file
            layer_image = layer.composite()  # Get the composite image of the layer
            layer_image.save(output_path)
            print(f"Saved {output_path}")

def main():
    parser = argparse.ArgumentParser(description='Parse a PSD file and extract each layer as an image.')
    parser.add_argument('--input', type=str, help='Input PSD file', required=True)

    args = parser.parse_args()

    parse_psd(args.input)

if __name__ == '__main__':
    main()
