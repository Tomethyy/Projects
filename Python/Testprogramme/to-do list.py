import csv
import os

to_do_list = {}

def load_file():
    file_name = 'to-do list.csv'

    # Check if the file exists
    if not os.path.isfile(file_name):
        # Create a new file with headers if it doesn't exist
        with open(file_name, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(['ID', 'Description', 'Priority', 'Status', 'Due Date'])
        return
    
    # Read the existing file and store all entries
    with open(file_name, mode='r', newline='') as file:
        reader = csv.DictReader(file)
        to_do_list = [
            (row['ID'], row['Description'], row['Priority'], row['Status'], row['Due Date'])
            for row in reader
        ]
    
def save_file():
    with open('to-do list.csv', mode='w', newline='') as file:
        writer = csv.writer(file)
        writer.writerow(['ID', 'Description', 'Priority', 'Status', 'Due Date'])
        writer.writerows(to_do_list)

 
def add_task():
    try:
        id = len(to_do_list) + 1
        description = input("Enter task description: ")
        priority = input("Enter priority (low/medium/high): ")
        status = "not started"
        date = input("Enter due date (DD-MM-YYYY) [optional]: ")
        
        to_do_list[id] = {
            'Description': description,
            'Priority': priority,
            'Status': status,
            'Due Date': date
        }

        save_file()
        print(f"Added task: {description}")
    except ValueError:
        print("Invalid Input")
    

def view_tasks():
    