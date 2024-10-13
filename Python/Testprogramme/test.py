def add(x, y):
    return x + y
def subtract(x, y):
    return x - y
def multiply(x, y):
    return x * y
def divide(x ,y):
    if y == 0:
        return "Fehler: Division durch 0"
    return x / y
def calculator():
    print("Einfacher Taschenrechner")
    print("Verfügbare Operationen: +, -, *, /")
    print("Geben Sie 'exit' ein, um das Programm zu beenden.")
    
    while True:
        operation = input("Geben Sie die Operation ein (+, -, *, /): ")
        if operation == 'exit':
            print("Programm beendet.")
            break
        
        if operation not in ('+', '-', '*', '/'):
            print("Ungültige Operation. Bitte versuchen Sie es erneut.")
            continue
        
        try:
            num1 = float(input("Geben Sie die erste Zahl ein: "))
            num2 = float(input("Geben Sie die zweite Zahl ein: "))
        except ValueError:
            print("Ungültige Eingabe. Bitte geben Sie eine gültige Zahl ein.")
            continue

        if operation == '+':
            print(f"Ergebnis: {add(num1, num2)}")
        elif operation == '-':
            print(f"Ergebnis: {subtract(num1, num2)}")
        elif operation == '*':
            print(f"Ergebnis: {multiply(num1, num2)}")
        elif operation == '/':
            print(f"Ergebnis: {divide(num1, num2)}")

if __name__ == "__main__":
    calculator()