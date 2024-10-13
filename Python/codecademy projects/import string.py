import string

def caesar_shift(text, shift):
    shifted_text = []
    for char in text:
        if char.isalpha():
            shift_base = ord('a') if char.islower() else ord('A')
            shifted_text.append(chr((ord(char) - shift_base + shift) % 26 + shift_base))
        else:
            shifted_text.append(char)
    return ''.join(shifted_text)

# Function to brute force Vigenère cipher based on key length
def vigenere_brute_force(ciphertext, max_key_length):
    ciphertext = ciphertext.lower().replace(" ", "")
    alphabet = string.ascii_lowercase
    
    for key_length in range(1, max_key_length + 1):
        print(f"Trying key length: {key_length}")
        
        # Break ciphertext into segments according to key length
        segments = [''] * key_length
        for i, char in enumerate(ciphertext):
            segments[i % key_length] += char
        
        # Try Caesar shift brute force for each segment
        for shift in range(26):
            decoded_segments = [caesar_shift(segment, shift) for segment in segments]
            
            # Reconstruct the message
            reconstructed_message = []
            for i in range(len(ciphertext)):
                reconstructed_message.append(decoded_segments[i % key_length][i // key_length])
            
            print(f"Shift {shift}: {''.join(reconstructed_message)}\n")

# Example ciphertext (replace with your own)
ciphertext = "txm srom vkda gl lzlgzr qpdb? fepb ejac! ubr imn tapludwy mhfbz cza ruxzal wg zztcgcexxch!"

# Max key length to try (adjust as necessary)
max_key_length = 7

vigenere_brute_force(ciphertext, max_key_length)
