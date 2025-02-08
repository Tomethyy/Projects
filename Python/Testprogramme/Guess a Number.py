import random

def game():
    print("Welcome to the Number Guessing Game.\n Please Guess a Number between 1 and 100.")

    number_to_guess = random.randint(1, 100)
    attempts = 0
    guessed_correctly = False

    while not guessed_correctly:
        try: 
            guess = int(input("Enter your guess! "))
            attempts += 1
        
            if guess < number_to_guess:
                print("Too low, try again!")
            elif guess > number_to_guess:
                print("Too high, try again!")
            else:
                print(f"Congratulations! You guessed correctly in {attempts} attempts!")
                guessed_correctly = True
                play_again = input("Do you want to play again? (yes/no): ").lower()
                if play_again == "yes":
                    game()
                else:
                    print("Thanks for playing!")

        except ValueError:
            print("Invalid Input. Please pick a number")

game()