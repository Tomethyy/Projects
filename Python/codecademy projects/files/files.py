import csv
import json

# Step 1: Collect compromised usernames from the CSV file
compromised_users = []

try:
    with open("passwords.csv", newline="") as password_file:
        password_csv = csv.DictReader(password_file)
        for row in password_csv:
            # Directly append the 'Username' field to the compromised_users list
            compromised_users.append(row["Username"])
except FileNotFoundError:
    print("Error: passwords.csv not found.")
except Exception as e:
    print(f"An error occurred: {e}")

# Step 2: Write compromised users to a text file
try:
    with open("compromised_users.txt", "w") as compromised_user_file:
        for user in compromised_users:
            compromised_user_file.write(user + "\n")
except Exception as e:
    print(f"An error occurred while writing to compromised_users.txt: {e}")

# Step 3: Notify the boss via a JSON file
try:
    with open("boss_message.json", "w") as boss_message:
        boss_message_dict = {
            "recipient": "The Boss",
            "message": "Mission Success"
        }
        json.dump(boss_message_dict, boss_message)
except Exception as e:
    print(f"An error occurred while writing to boss_message.json: {e}")

# Step 4: Write the Slash Null signature to a new CSV file
slash_null_sig = """
 _  _     ___   __  ____             
/ )( \   / __) /  \(_  _)            
) \/ (  ( (_ \(  O ) )(              
\____/   \___/ \__/ (__)             
 _  _   __    ___  __ _  ____  ____  
/ )( \ / _\  / __)(  / )(  __)(    \ 
) __ (/    \( (__  )  (  ) _)  ) D ( 
\_)(_/\_/\_/ \___)(__\_)(____)(____/ 
        ____  __     __   ____  _  _ 
 ___   / ___)(  )   / _\ / ___)/ )( \
(___)  \___ \/ (_/\/    \\___ \) __ (
       (____/\____/\_/\_/(____/\_)(_/
 __ _  _  _  __    __                
(  ( \/ )( \(  )  (  )               
/    /) \/ (/ (_/\/ (_/\             
\_)__)\____/\____/\____/
"""

try:
    with open("new_passwords.csv", "w") as new_passwords_obj:
        new_passwords_obj.write(slash_null_sig)
except Exception as e:
    print(f"An error occurred while writing to new_passwords.csv: {e}")


"FHROPWSVW2BDDOOXLPBA"