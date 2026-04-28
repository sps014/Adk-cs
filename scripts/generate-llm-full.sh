#!/bin/bash

# Change to the root of the repository
cd "$(dirname "$0")/.." || exit 1

OUTPUT_FILE="llm-full.txt"

# Empty the output file
> "$OUTPUT_FILE"

# Add README.md on top if it exists
if [ -f "README.md" ]; then
    echo "# README.md" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    cat "README.md" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    echo "---" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
fi

# Find all markdown files in the repository, excluding specific folders and files
find . -type f -name "*.md" \
    | grep -vE '/\.git/|/\.venv/|/bin/|/obj/' \
    | grep -iEv '/(readme|contributing|license)\.md$' \
    | sort | while read -r file; do
    
    # Clean up the leading './' from the file path
    clean_file="${file#./}"
    
    echo "# $clean_file" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    cat "$file" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
    echo "---" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"
done

echo "Successfully generated $OUTPUT_FILE"
