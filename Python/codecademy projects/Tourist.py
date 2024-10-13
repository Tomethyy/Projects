destinations = ["Paris, France", "Shanghai, China",
                "Los Angeles, USA", "Sao Paulo, Brazil",
                "Cairo, Egypt"]

traveler = ['Erin Wilkes', 'Shanghai, China', ['historical site', 'art']]


def get_destination_index(destination):
    destination_index = destinations.index(destination)
    return destination_index

def get_traveler_location(traveler):
    traveler_destination = traveler[1]
    traveler_destination_index = traveler.index(traveler_destination)
    return traveler_destination_index

test_destination_index = get_traveler_location(traveler)

print(test_destination_index)


