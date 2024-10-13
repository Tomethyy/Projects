gamers = [] 
def add_gamer(gamer, gamers_list):
    if gamer.get("name") and gamer.get("availability"):
        gamers_list.append(gamer)
    else: print("Missing Information")

kimberly = {"name": "Kimberly Warner", "availability": ["Monday", "Tuesday", "Friday"]}

add_gamer(kimberly, gamers)

add_gamer({'name':'Thomas Nelson','availability': ["Tuesday", "Thursday", "Saturday"]}, gamers)
add_gamer({'name':'Joyce Sellers','availability': ["Monday", "Wednesday", "Friday", "Saturday"]}, gamers)
add_gamer({'name':'Michelle Reyes','availability': ["Wednesday", "Thursday", "Sunday"]}, gamers)
add_gamer({'name':'Stephen Adams','availability': ["Thursday", "Saturday"]}, gamers)
add_gamer({'name': 'Joanne Lynn', 'availability': ["Monday", "Thursday"]}, gamers)
add_gamer({'name':'Latasha Bryan','availability': ["Monday", "Sunday"]}, gamers)
add_gamer({'name':'Crystal Brewer','availability': ["Thursday", "Friday", "Saturday"]}, gamers)
add_gamer({'name':'James Barnes Jr.','availability': ["Tuesday", "Wednesday", "Thursday", "Sunday"]}, gamers)
add_gamer({'name':'Michel Trujillo','availability': ["Monday", "Tuesday", "Wednesday"]}, gamers)

def build_daily_frequenzy_table():
    return {"Monday": 0, "Tuesday": 0, "Wednesday": 0, "Thursday": 0, "Friday": 0, "Saturday": 0, "Sunday": 0}

count_availability = build_daily_frequenzy_table()

def calculate_availability(gamers_list, available_frequenzy):
    for gamer in gamers_list:
        for day in gamer["availability"]:
            available_frequenzy[day] += 1

calculate_availability(gamers, count_availability)

def find_best_night(availability_table):
    best_night=  max(availability_table, key=availability_table.get)
    return best_night

game_night = find_best_night(count_availability)

def available_on_night(gamers_list, day):
    return [gamer["name"] for gamer in gamers_list if day in gamer['availability']]

attending_game_night = available_on_night(gamers, game_night)


def send_email(gamers_who_can_attend, day, game):
    for player in gamers_who_can_attend:
        form_email = f"Dear {player}, the day for the next game of {game} has been set. It is the next {day}! Let us have a great time. See you then."
        print(form_email)

print(send_email(attending_game_night, game_night, "Abruplty Goblins!"))

