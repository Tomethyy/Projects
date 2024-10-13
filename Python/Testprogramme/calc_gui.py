import tkinter as tk

def add():
    result.set(float(entry1.get()) + float(entry2.get()))

def subtract():
    result.set(float(entry1.get()) - float(entry2.get()))

def multiply():
    result.set(float(entry1.get()) * float(entry2.get()))

def divide():
    if float(entry2.get()) == 0:
        result.set("Fehler: Division durch Null")
    else:
        result.set(float(entry1.get()) / float(entry2.get()))

# Hauptfenster erstellen
root = tk.Tk()
root.title("Einfacher Taschenrechner")

# Variablen für die Eingaben und das Ergebnis
entry1 = tk.Entry(root, width=10)
entry1.grid(row=0, column=0, padx=5, pady=5)

entry2 = tk.Entry(root, width=10)
entry2.grid(row=0, column=1, padx=5, pady=5)

result = tk.StringVar()
result_label = tk.Label(root, textvariable=result)
result_label.grid(row=0, column=2, padx=5, pady=5)

# Buttons für die Operationen
button_add = tk.Button(root, text="+", command=add)
button_add.grid(row=1, column=0, padx=5, pady=5)

button_subtract = tk.Button(root, text="-", command=subtract)
button_subtract.grid(row=1, column=1, padx=5, pady=5)

button_multiply = tk.Button(root, text="*", command=multiply)
button_multiply.grid(row=2, column=0, padx=5, pady=5)

button_divide = tk.Button(root, text="/", command=divide)
button_divide.grid(row=2, column=1, padx=5, pady=5)

# Hauptschleife starten
root.mainloop()
