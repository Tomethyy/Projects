import datetime as dt
from decimal import ROUND_HALF_UP, Decimal
from random import randint, choice
import custom_module

current_date = dt.date.today()
current_year = current_date.year
target_year = randint(0, 10000)
destinations = ["Rome", "Berlin", "London", "Dresden", "Leipzig", "Qatar", "München"]
destination = choice(destinations)
time = dt.datetime.now().time()
base_cost = Decimal("12.349")
cost_multiplier = abs(Decimal(current_year - target_year))
final_cost = base_cost + cost_multiplier
final_cost_rounded = final_cost.quantize(Decimal("0.01"), rounding= ROUND_HALF_UP)

#print(f"It is the {current_date} at {time}")

#print(final_cost_rounded)

print(custom_module.generate_time_travel_message(target_year, destination, final_cost_rounded))