import pandas as pd
import csv

# --- Configuration ---
input_filename = 'QuizListGenerator/QuizList.csv'
output_filename = 'QuizListGenerator/QuizList_LocalizedReady.csv'
# ---------------------

def is_exact_number(value):
    """
    Checks if a value can be converted to a number (integer or float)
    and contains no other text.
    """
    if pd.isna(value):
        return False
    try:
        float(str(value))
        return True
    except (ValueError, TypeError):
        return False

try:
    # Read the original CSV file
    df = pd.read_csv(input_filename, encoding='utf-8')

    # Prepare the list to hold the new formatted data
    localization_data = []

    # Process every row from the original file
    for index, row in df.iterrows():
        question_id_raw = row.get('NO')
        question_text = row.get('QUESTION')

        if pd.isna(question_id_raw) or pd.isna(question_text):
            continue

        question_id = str(int(question_id_raw)).zfill(4)
        
        # Rule 1: Always add the question to the table
        localization_data.append([f'{question_id}.question', '', '', question_text])

        # Rule 2: Skip adding choices for 'True False' questions
        if row.get('TYPE') == 'True False':
            continue

        # Rule 3: Add choices only if they are not exact numbers
        for i in range(1, 5):
            choice_col = f'CHOICE {i}'
            choice_text = row.get(choice_col)
            
            # Check if the choice exists and is not empty
            if pd.notna(choice_text) and str(choice_text).strip() != '':
                # The new filtering condition
                if not is_exact_number(choice_text):
                    localization_data.append([f'{question_id}.choice{i}', '', '', choice_text])

    # Write the results to a new CSV file with 'utf-8-sig' for correct Thai display
    with open(output_filename, 'w', newline='', encoding='utf-8-sig') as file:
        writer = csv.writer(file)
        writer.writerow(['Key', 'English(en)', 'Japanese(ja)', 'Thai(th)'])
        writer.writerows(localization_data)

    print(f"✅ Success! Created '{output_filename}' with all filtering rules applied.")

except FileNotFoundError:
    print(f"❌ Error: The file '{input_filename}' was not found. Please ensure it's in the same directory.")
except Exception as e:
    print(f"An error occurred: {e}")