#!/usr/bin/env python3
"""
Step 4.2: Validate Canonical JSON Structure
Checks all JSON files in templates/production-data/ for the required six-field envelope.
"""

import json
import os
import sys

REQUIRED_FIELDS = [
    "repositoryName",
    "schemaVersion",
    "revision",
    "updatedAtUtc",
    "updatedByUserId",
    "records"
]

DATA_DIR = "templates/production-data"

def validate_envelope(filepath):
    """Validate a single JSON file for canonical structure."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        missing_fields = []
        for field in REQUIRED_FIELDS:
            if field not in data:
                missing_fields.append(field)
        
        if missing_fields:
            return False, f"Missing fields: {', '.join(missing_fields)}"
        
        if not isinstance(data["records"], list):
            return False, "'records' field is not a list"
        
        return True, "Valid"
    
    except json.JSONDecodeError as e:
        return False, f"Invalid JSON: {str(e)}"
    except Exception as e:
        return False, f"Error reading file: {str(e)}"

def main():
    print(f"Validating JSON envelopes in {DATA_DIR}/...")
    print("-" * 60)
    
    if not os.path.exists(DATA_DIR):
        print(f"ERROR: Directory {DATA_DIR} does not exist!")
        sys.exit(1)
    
    json_files = [f for f in os.listdir(DATA_DIR) if f.endswith('.json')]
    total = len(json_files)
    valid_count = 0
    invalid_count = 0
    
    for filename in sorted(json_files):
        filepath = os.path.join(DATA_DIR, filename)
        is_valid, message = validate_envelope(filepath)
        
        if is_valid:
            print(f"✓ {filename}: {message}")
            valid_count += 1
        else:
            print(f"✗ {filename}: {message}")
            invalid_count += 1
    
    print("-" * 60)
    print(f"Total: {total} | Valid: {valid_count} | Invalid: {invalid_count}")
    
    if invalid_count > 0:
        print("\nVALIDATION FAILED: Some files do not meet the canonical structure.")
        sys.exit(1)
    else:
        print("\nVALIDATION PASSED: All files have correct canonical structure.")
        sys.exit(0)

if __name__ == "__main__":
    main()
