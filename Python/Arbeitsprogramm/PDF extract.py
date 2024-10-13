from pdfminer.high_level import extract_text
import re


pdf_path = "Python/Arbeitsprogramm/Files/stundennachweise_8_2024.pdf"
text = extract_text(pdf_path)
lines = text.splitlines()

work_entries = []
pattern = re.compile(r'(\w{2})\s(\d{2}\.\d{2}\.)\s+(\d{2}:\d{2}h) - (\d{2}:\d{2}h)\s+.*\s+(\d+\.\d+)\s+\d+\s+\w+')

for line in lines:
    match = pattern.search(line)
    if match:
        day_of_week, date, start_time, end_time, hours_worked = match.groups()
        work_entries.append({
            'Day': day_of_week,
            'Date': date,
            'Start Time': start_time,
            'End Time': end_time,
            'Hours Worked': float(hours_worked)
        })

# Print the parsed work entries
for entry in work_entries:
    print(entry)

print(text)