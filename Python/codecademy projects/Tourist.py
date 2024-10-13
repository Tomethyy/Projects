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

def find_attractions(destination, interests):
    destination_index = get_destination_index(destination)
    attractions_in_city = attractions[destination_index]
    attractions_with_interests = []
    
    for possible_attraction in attractions_in_city:
        attraction_tags = possible_attraction[1]

        for interest in interests:
            if interest in attraction_tags:
                attractions_with_interests.append(possible_attraction[0])
                break
    
    return attractions_with_interests

def get_attractions_for_travelers(traveler):
    traveler_destination = traveler[1]
    traveler_interests = traveler[2]

    traveler_attractions = find_attractions(traveler_destination, traveler_interests)
    interests_string = f"Hi {traveler[0]}, we think you'll like these places around {traveler_destination}: "

    for i in traveler_attractions:
        interests_string += i

    return interests_string

smills_france = get_attractions_for_travelers(['Dereck Smill', 'Paris, France', ['monument']])

print(smills_france)

