from .const import (
    SYSTEM_PROMPTS,
    TEMPERATURE_MAP,
    DEFAULT_MODEL,
    FRIENDLY_TEMPS,
    DEFAULT_TEMP_SETTING,
    DEFAULT_PROMPT_STYLE
)
from .api import DeepSeekAPI
from rich.console import Console
import logging
import os

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    filename='deepseek_test.log'
)

console = Console()

def get_friendly_temp_name(setting):
    """Get friendly name from original setting name"""
    return {v: k for k, v in FRIENDLY_TEMPS.items()}.get(setting, setting)

def print_help(console):
    """Print organized help menu"""
    console.print("\n[bold yellow]Available Commands:[/bold yellow]")
    
    console.print("\n[cyan]Chat Settings:[/cyan]")
    console.print("- /memory    Toggle conversation memory")
    console.print("- /clear     Clear conversation history")
    console.print("- /history   Show conversation history")
    
    console.print("\n[cyan]Model Settings:[/cyan]")
    console.print("- /models    List available models")
    console.print("- /model     Show current model")
    console.print("- /model <name>   Switch to different model")
    
    console.print("\n[cyan]Response Style:[/cyan]")
    console.print("- /style     Show available conversation styles")
    console.print("- /style <name>   Switch conversation style")
    
    console.print("\n[cyan]Temperature Modes:[/cyan]")
    console.print("- /mode      Show available temperature modes")
    console.print("- /mode <name>    Switch temperature mode")
    
    console.print("\n[cyan]Account:[/cyan]")
    console.print("- /balance   Check account balance")
    
    console.print("\n[cyan]System:[/cyan]")
    console.print("- /help      Show this help message")
    console.print("- /quit      Exit the program")
    
    console.print("\n[cyan]Chat:[/cyan]")
    console.print("- Just type your message to chat")
    console.print("-" * 50)

def handle_command(cmd, api, console, current_model, current_temp_setting, current_temp, 
                  current_prompt, conversation_active, conversation_history):
    """Handle commands and return whether the command was valid"""
    parts = cmd.split(maxsplit=1)
    base_cmd = parts[0]
    arg = parts[1] if len(parts) > 1 else None
    
    # Command handlers
    if base_cmd == 'quit':
        return True, True  # (valid_command, should_quit)
        
    elif base_cmd == 'help':
        print_help(console)
        return True, False
        
    elif base_cmd == 'memory':
        conversation_active = not conversation_active
        console.print(f"\n[green]Memory: {'Active' if conversation_active else 'Inactive'}[/green]")
        return True, False
        
    elif base_cmd == 'clear':
        conversation_history[:] = [{"role": "system", "content": SYSTEM_PROMPTS[current_prompt]}]
        console.print("\n[green]Conversation history cleared[/green]")
        return True, False
        
    elif base_cmd == 'model':
        if not arg:
            console.print(f"\n[green]Current model: {current_model}[/green]")
            return True, False
        models = api.get_models()
        if arg in models:
            current_model = arg
            console.print(f"\n[green]Switched to model: {current_model}[/green]")
        else:
            console.print(f"\n[red]Invalid model. Available models: {', '.join(models)}[/red]")
        return True, False
        
    elif base_cmd == 'mode':
        if not arg:
            console.print("\n[bold green]Available Modes:[/bold green]")
            for friendly_name, original_name in FRIENDLY_TEMPS.items():
                temp_value = TEMPERATURE_MAP[original_name]
                console.print(f"- {friendly_name}: {temp_value}")
            return True, False
        if arg in FRIENDLY_TEMPS:
            current_temp_setting = FRIENDLY_TEMPS[arg]
            current_temp = TEMPERATURE_MAP[current_temp_setting]
            console.print(f"\n[green]Mode set to: {arg} ({current_temp})[/green]")
        else:
            console.print(f"\n[red]Invalid mode. Available modes: {', '.join(FRIENDLY_TEMPS.keys())}[/red]")
        return True, False
        
    elif base_cmd == 'style':
        if not arg:
            console.print("\n[bold green]Available Styles:[/bold green]")
            for style, prompt in SYSTEM_PROMPTS.items():
                console.print(f"- {style}")
                console.print(f"  {prompt[:100]}...")
            return True, False
        if arg in SYSTEM_PROMPTS:
            current_prompt = arg
            conversation_history[0] = {"role": "system", "content": SYSTEM_PROMPTS[current_prompt]}
            console.print(f"\n[green]Switched to {arg} style[/green]")
        else:
            console.print(f"\n[red]Invalid style. Available styles: {', '.join(SYSTEM_PROMPTS.keys())}[/red]")
        return True, False
    
    return False, False  # Command not recognized

def main():
    api_key = os.getenv("DEEPSEEK_API_KEY", 'sk-19f48089bf8a464a8903d3555cd14aac')
    api = DeepSeekAPI(api_key=api_key)
    current_model = DEFAULT_MODEL
    current_temp_setting = DEFAULT_TEMP_SETTING
    current_temp = TEMPERATURE_MAP[current_temp_setting]
    
    # Initialize conversation with default system prompt
    conversation_active = False
    current_prompt = DEFAULT_PROMPT_STYLE
    conversation_history = [
        {"role": "system", "content": SYSTEM_PROMPTS[current_prompt]}
    ]
    
    console.print("[bold blue]DeepSeek API Test Console[/bold blue]")
    console.print("\n[yellow]Current Settings:[/yellow]")
    console.print(f"Model: {current_model}")
    console.print(f"Mode: {get_friendly_temp_name(current_temp_setting)} ({current_temp})")
    console.print(f"Style: {current_prompt}")
    console.print(f"Memory: {'Active' if conversation_active else 'Inactive'}")
    console.print("\nType /help for commands")
    console.print("-" * 50)
    
    while True:
        user_input = input("\nYou: ").strip()
        
        # Handle commands (now with validation)
        if user_input.startswith('/'):
            cmd = user_input[1:].lower()  # Remove / and lowercase
            valid_command, should_quit = handle_command(
                cmd, api, console, current_model, current_temp_setting, 
                current_temp, current_prompt, conversation_active, 
                conversation_history
            )
            
            if should_quit:
                break
                
            if valid_command:
                continue
            else:
                console.print("\n[red]Invalid command. Type /help for available commands.[/red]")
                continue
        
        # Process chat input
        console.print("\n[bold cyan]DeepSeek's response:[/bold cyan]")
        try:
            # Prepare messages based on conversation mode
            if conversation_active:
                conversation_history.append({"role": "user", "content": user_input})
                messages = conversation_history
            else:
                messages = [
                    {"role": "system", "content": SYSTEM_PROMPTS[current_prompt]},
                    {"role": "user", "content": user_input}
                ]
            
            # Get response
            response_text = ""
            for chunk in api.chat_completion(
                messages=messages,
                stream=True,
                model=current_model,
                temperature=current_temp
            ):
                print(chunk, end='', flush=True)
                response_text += chunk
            print()
            
            # Add response to history if in conversation mode
            if conversation_active:
                conversation_history.append({"role": "assistant", "content": response_text})
            
        except Exception as e:
            console.print(f"\n[red]Error: {str(e)}[/red]")
            logging.error(f"API Error: {str(e)}")

if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        console.print(f"\n[red]Fatal Error: {str(e)}[/red]")
        logging.error(f"Fatal Error: {str(e)}") 