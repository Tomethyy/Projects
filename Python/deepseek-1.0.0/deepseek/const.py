import os

DEEPSEEK_API_KEY = os.environ.get('DEEPSEEK_API_KEY')

# Default settings
DEFAULT_MODEL = 'deepseek-chat'
DEFAULT_TEMP_SETTING = 'General Setting'
DEFAULT_PROMPT_STYLE = 'tutor'
DEFAULT_USR_PROM = "Hello, how can I help you?"

# Temperature settings with friendly names
FRIENDLY_TEMPS = {
    "balanced": "General Setting",      # 1.0  - Balanced responses
    "precise": "Coding/Math",          # 0.0  - Exact, deterministic responses
    "analysis": "Data Cleaning/Analysis", # 1.0  - Analytical responses
    "chatty": "General Conversation",   # 1.3  - More varied, conversational
    "translate": "Translation",         # 1.3  - Good for translations
    "creative": "Creative Writing"      # 1.5  - Most creative responses
}

# Temperature values
TEMPERATURE_MAP = {
    'General Setting': 1.0,
    'Coding/Math': 0,
    'Data Cleaning/Analysis': 1.0,
    'General Conversation': 1.3,
    'Translation': 1.3,
    'Creative Writing': 1.5
}

# API endpoints
API_USER_BAL = "https://api.deepseek.com/user/balance"
API_CHAT_COM = "https://api.deepseek.com/chat/completions"
API_CHAT_FIM = "https://api.deepseek.com/beta/completions"
API_CHAT_MOD = "https://api.deepseek.com/models"

# System prompts
DEFAULT_SYS_PROM = "You are a tutor that always responds in the Socratic style. You never give the student the answer, but always try to ask just the right question to help them learn to think for themselves. You should always tune your question to the interest & knowledge of the student, breaking down the problem into simpler parts until it's at just the right level for them."

SYSTEM_PROMPTS = {
    "tutor": DEFAULT_SYS_PROM,  # Socratic teaching style
    "assistant": "You are a helpful assistant",  # Default helpful style
    "coder": "You are a Python Tutor AI, dedicated to helping users learn Python and build end-to-end projects using Python and its related libraries. Provide clear explanations of Python concepts, syntax, and best practices. Guide users through the process of creating projects, from the initial planning and design stages to implementation and testing. Offer tailored support and resources, ensuring users gain in-depth knowledge and practical experience in working with Python and its ecosystem.",
    "analyst": "You are a data analyst who excels at interpreting and explaining data patterns"
}
