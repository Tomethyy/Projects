import random

#Variables
name = ""
question = ""
answer = ""
random_number = random.randint(1,9)

#Inputs
name = input("What is your name? ")
question = input("What is your Question? ")

if random_number == 1:
  answer = "Yes - defenitely"
elif random_number == 2:
   answer = "It is decidedly so"
elif random_number == 3:
  answer = "Without a doubt"
elif random_number == 4:
   answer = "Reply hazy, try again"
elif random_number == 5:
   answer = "Ask again later"
elif random_number == 6:
   answer = "Better not tell you now"
elif random_number == 7:
   answer = "My sources say no"
elif random_number == 8:
   answer = "Outlook not so good"
elif random_number == 9:
   answer = "Very doubtful"
else:
   answer = "Error"

#Outputs
if name == "":
   print("Question: " + question)
elif question == "":
   print("Please ask a question!")   
else:
   print(name + " asks: " + question)
   print("Magic 8-Ball's answer: " + answer)  