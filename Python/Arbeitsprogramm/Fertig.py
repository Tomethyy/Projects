import re
import tkinter as tk
from tkinter import filedialog, messagebox
from tkinter import ttk
from pdfminer.high_level import extract_text
from datetime import datetime

def extract_data_from_pdf(pdf_path):
    text = extract_text(pdf_path)
    lines = text.splitlines()

    work_entries = []
    entry = {}

    day_pattern = re.compile(r'^(Mo|Di|Mi|Do|Fr|Sa|So)\s(\d{2}\.\d{2}\.)\s+(\d{2}:\d{2}h)\s-\s(\d{2}:\d{2}h)')
    position_pattern = re.compile(r'#(\d+)\s-\s(.*)')
    function_pattern = re.compile(r'LSKK\s(\S+)')

    for line in lines:
        if day_pattern.match(line):
            if entry:  # If there's already data in entry, save it
                work_entries.append(entry)
                entry = {}

            day_match = day_pattern.match(line)
            start_time_str = day_match.group(3).replace('h', '')
            end_time_str = day_match.group(4).replace('h', '')

            # Calculate hours worked
            start_time = datetime.strptime(start_time_str, "%H:%M")
            end_time = datetime.strptime(end_time_str, "%H:%M")
            hours_worked = (end_time - start_time).seconds / 3600

            entry['Arbeitszeiten'] = f"{day_match.group(2)} {day_match.group(3)} - {day_match.group(4)}"
            entry['Stunden'] = round(hours_worked, 2)

        elif position_pattern.search(line):
            position_match = position_pattern.search(line)
            entry['Position'] = position_match.group(2)

        elif function_pattern.search(line):
            function_match = function_pattern.search(line)
            entry['Position'] += f" {function_match.group(1)}"

        elif "anwesend" in line:
            entry['Zuschläge'] = "N/A"  # Placeholder since "Zuschläge" info isn't clear in provided text

    if entry:  # Add the last entry if there's data
        work_entries.append(entry)

    return work_entries

def populate_table(data):
    for entry in data:
        tree.insert("", "end", values=(entry['Arbeitszeiten'], entry['Position'], entry['Stunden'], entry.get('Zuschläge', '')))

def select_file():
    file_path = filedialog.askopenfilename(filetypes=[("PDF files", "*.pdf")])
    if file_path:
        try:
            data = extract_data_from_pdf(file_path)
            if data:
                populate_table(data)
            else:
                messagebox.showinfo("Info", "No valid data found in the PDF.")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to process PDF: {e}")
    else:
        messagebox.showwarning("Warning", "No file selected.")

root = tk.Tk()
root.title("Arbeitsprogramm")

frame = tk.Frame(root)
frame.pack(pady=20)

label = tk.Label(frame, text="Select the PDF file:")
label.pack(side=tk.LEFT, padx=10)

button = tk.Button(frame, text="Browse", command=select_file)
button.pack(side=tk.LEFT)

columns = ("Arbeitszeiten", "Position", "Stunden", "Zuschläge")
tree = ttk.Treeview(root, columns=columns, show='headings')
tree.heading("Arbeitszeiten", text="Arbeitszeiten")
tree.heading("Position", text="Position")
tree.heading("Stunden", text="Stunden")
tree.heading("Zuschläge", text="Zuschläge")

tree.column("Arbeitszeiten", minwidth=100, width=150)
tree.column("Position", minwidth=100, width=250)
tree.column("Stunden", minwidth=50, width=100)
tree.column("Zuschläge", minwidth=50, width=100)

tree.pack(pady=20, padx=20, fill="both", expand=True)

root.mainloop()
