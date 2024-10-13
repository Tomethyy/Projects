destinations = ["Paris, France", "Shanghai, China",
                "Los Angeles, USA", "Sao Paulo, Brazil",
                "Cairo, Egypt"]

traveler = ['Erin Wilkes', 'Shanghai, China', ['historical site', 'art']]
attractions = [ [] for _ in destinations ]


def get_destination_index(destination):
    destination_index = destinations.index(destination)
    return destination_index

def get_traveler_location(traveler):
    traveler_destination = traveler[1]
    traveler_destination_index = traveler.index(traveler_destination)
    return traveler_destination_index

def add_attraction(destination, attraction):
    destination_index = get_destination_index(destination)
    if destination_index >= 0:
        attractions_for_destination = attractions[destination_index]
        attractions_for_destination.append(attraction)
        return
    else: pass

add_attraction("Los Angeles, USA", ["Venice Beach", ["beach"]])
add_attraction("Paris, France", ["the Louvre", ["art", "museum"]])
add_attraction("Paris, France", ["Arc de Triomphe", ["historical site", "monument"]])
add_attraction("Shanghai, China", ["Yu Garden", ["garden", "historical site"]])
add_attraction("Shanghai, China", ["Yuz Museum", ["art", "museum"]])
add_attraction("Shanghai, China", ["Oriental Pearl Tower", ["skyscraper", "viewing deck"]])
add_attraction("Los Angeles, USA", ["LACMA", ["art", "museum"]])
add_attraction("Sao Paulo, Brazil", ["São Paulo Zoo", ["zoo"]])
add_attraction("Sao Paulo, Brazil", ["Pátio do Colégio", ["historical site"]])
add_attraction("Cairo, Egypt", ["Pyramids of Giza", ["monument", "historical site"]])
add_attraction("Cairo, Egypt", ["Egyptian Museum", ["museum"]])

