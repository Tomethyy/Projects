import csv
import os


total_expenses = []


def load_file():
    global total_expenses
    file_name = 'expenses.csv'

    # Check if the file exists
    if not os.path.isfile(file_name):
        # Create a new file with headers if it doesn't exist
        with open(file_name, mode='w', newline='') as file:
            writer = csv.writer(file)
            writer.writerow(['Date', 'Description', 'Amount'])
        return

    # Read the existing file and store all entries
    with open(file_name, mode='r', newline='') as file:
        reader = csv.DictReader(file)
        total_expenses.clear()  # Clear existing data
        total_expenses.extend(
            (row['Date'], row['Description'], float(row['Amount']))
            for row in reader
            if row['Date'].lower() != 'total' and row['Amount']
        )


def save_file():
    with open('expenses.csv', mode='w', newline='') as file:
        writer = csv.writer(file)
        writer.writerow(['Date', 'Description', 'Amount'])
        writer.writerows(expense for expense in total_expenses)
        writer.writerow(['Total', 'Gesamt', str(calculate_total())])  # Add total row

def add_expense():
    while True:
        try:
            date = input('Enter the date (YYYY-MM-DD): ')
            description = input('Enter the description: ')
            amount_str = input('Enter the amount: ').replace(',', '.')
            amount = float(amount_str)
            break
        except ValueError:
            print("Invalid amount. Please enter a valid number.")
    
    total_expenses.append((date, description, amount))
    save_file()
    print(f"Added expense: €{amount:.2f}")

def view_expenses():
    if not total_expenses:
        print("No expenses found.")
        return
        
    print("\nExpense List:")
    print("-" * 50)
    for date, description, amount in total_expenses:
        print(f"Date: {date} | Description: {description} | Amount: €{amount:.2f}")
    print("-" * 50)
    print(f"Total Expenses: €{calculate_total():.2f}\n")

def calculate_total():
    return sum(amount for _, _, amount in total_expenses)

def main():
    load_file()
    while True:
        print('1. Add Expense')
        print('2. View Expenses')
        print('3. Exit')
        choice = input('Enter your choice: ')

        if choice == '1':
            add_expense()
        elif choice == '2':
            view_expenses()
        elif choice == '3':
            break
        else:
            print('Invalid choice. Please try again.')


if __name__ == '__main__':
    main()
