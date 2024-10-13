from datetime import datetime


class Menu:

    def __init__(self, name, items, start_time, end_time):
        self.name = name
        self.items = items
        self.start_time = datetime.strptime(start_time, "%I %p").time()
        self.end_time = datetime.strptime(end_time, "%I %p").time()

    def __str__(self):
        return f"{self.name} menu is available from {self.start_time} to {self.end_time}"

    def calculate_bill(self, *purchased_items):
        total = 0
        for i in purchased_items:
            if i in self.items:
                total += self.items[i]
            else: print(f"{i} is not available on the {self.name} menu")
        return total

brunch = Menu(
    name = "brunch",
    items = {'pancakes': 7.50, 'waffles': 9.00, 'burger': 11.00, 'home fries': 4.50, 'coffee': 1.50, 'espresso': 3.00, 'tea': 1.00, 'mimosa': 10.50, 'orange juice': 3.50},
    start_time = "11 am",
    end_time = "4 pm"
)

early_bird = Menu(
    name = "early bird",
    items = {'salumeria plate': 8.00, 'salad and breadsticks (serves 2, no refills)': 14.00, 'pizza with quattro formaggi': 9.00, 'duck ragu': 17.50, 'mushroom ravioli (vegan)': 13.50, 'coffee': 1.50, 'espresso': 3.00,},
    start_time = "3 pm",
    end_time = "6 pm"
)

dinner = Menu(
    name = "dinner",
    items = {'crostini with eggplant caponata': 13.00, 'caesar salad': 16.00, 'pizza with quattro formaggi': 11.00, 'duck ragu': 19.50, 'mushroom ravioli (vegan)': 13.50, 'coffee': 2.00, 'espresso': 3.00,},
    start_time = "5 pm",
    end_time = "11 pm"
)

kids = Menu(
    name = "kids",
    items = {'chicken nuggets': 6.50, 'fusilli with wild mushrooms': 12.00, 'apple juice': 3.00},
    start_time = "11 am",
    end_time = "9 pm"
)

arepas_menu = Menu(
    name = "arepas",
    items = {'arepa pabellon': 7.00, 'pernil arepa': 8.50, 'guayanes arepa': 8.00, 'jamon arepa': 7.50},
    start_time = "10 am",
    end_time = "8 pm"
)
#print(brunch.calculate_bill("waffles", "burger", "pancakes"))
#print(early_bird.calculate_bill("salumeria plate", "mushroom ravioli (vegan)"))

class Franchise:

    def __init__(self, address, menus):
        self.address = address
        self.menus = menus
    
    def __str__(self) -> str:
        return f"The address is: {self.address}"
    
    def available_menus(self, time_str):
        time = datetime.strptime(time_str, "%I %p").time()
        available = []
        for menu in self.menus:
            if menu.start_time <= time <= menu.end_time:
                available.append(menu)
        return available



flagship_store = Franchise(
    address = "1232 West End Road",
    menus = [brunch, early_bird, dinner, kids]
)
new_installment = Franchise(
    address = "12 East Mulberry Street",
    menus = [brunch, early_bird, dinner, kids]
)

arepas_place = Franchise(
    address = "189 Fitzgerald Avenue",
    menus = [arepas_menu]
)


class Business:

    def __init__(self, name, franchises):
        self.name = name
        self.franchises = franchises
    
    def __str__(self):
        return f"{self.name} has franchises at:\n" + ",\n".join([franchise.address for franchise in self.franchises])
    
basta = Business(
    name = "Basta Fazoolin' with my Heart",
    franchises = [flagship_store, new_installment]
)

arepa = Business(
    name = "Take a' Arepa",
    franchises = [arepas_place]
)
