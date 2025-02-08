from openai import OpenAI
from datetime import datetime
import json
import os
import readline
from rich.console import Console
from rich.syntax import Syntax
from rich.markdown import Markdown
import tiktoken
import logging

# Set up logging
logging.basicConfig(level=logging.DEBUG, format="%(asctime)s - %(levelname)s - %(message)s")

console = Console()

PERSONALITIES = {
    "default": "You are a helpful assistant",
    "coder": "You are an expert programmer who provides detailed code explanations and examples",
    "teacher": "You are a patient teacher who breaks down complex concepts into simple explanations",
    "analyst": "You are a data analyst who excels at interpreting and explaining data patterns"
}

class ChatConfig:
    def __init__(self):
        self.temperature = 0.7
        self.max_tokens = 8000
        self.personality = "default"
        self.model = "deepseek-reasoner"  # Confirm this is the correct model name
        self.input_cache_miss_price = 0.55  # Price per million input cache miss tokens
        self.input_cache_hit_price = 0.14   # Price per million input cache hit tokens
        self.output_price = 2.19            # Price per million output tokens
        



def count_tokens(messages, model="deepseek-reasoner"):
    """Estimate token count for the messages"""
    encoding = tiktoken.encoding_for_model("gpt-3.5-turbo")  # Using as approximation
    total_tokens = 0
    for message in messages:
        total_tokens += len(encoding.encode(message["content"])) + 4  # 4 tokens for message format
    return total_tokens

def format_code_response(response):
    """Format code blocks in the response with syntax highlighting"""
    parts = response.split("```")
    formatted_parts = []
    
    for i, part in enumerate(parts):
        if i % 2 == 0:  # Regular text
            formatted_parts.append(part)
        else:  # Code block
            lang = part.split('\n')[0] if part.split('\n')[0] else "python"
            code = '\n'.join(part.split('\n')[1:]) if part.split('\n')[0] else part
            syntax = Syntax(code, lang, theme="monokai")
            formatted_parts.append(syntax)
    
    return formatted_parts

def create_chat_client():
    # Ensure the base_url and api_key are correct for DeepSeek
    return OpenAI(api_key="sk-19f48089bf8a464a8903d3555cd14aac", base_url="https://api.deepseek.com")

def save_conversation(messages, config, filename=None):
    if not filename:
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"chat_history_{timestamp}.json"
    
    data = {
        "messages": messages,
        "config": {
            "temperature": config.temperature,
            "personality": config.personality,
            "model": config.model
        }
    }
    
    with open(f"chat_logs/{filename}", 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2)
    return filename

def load_conversation(filename):
    with open(f"chat_logs/{filename}", 'r', encoding='utf-8') as f:
        data = json.load(f)
    config = ChatConfig()
    config.temperature = data["config"]["temperature"]
    config.personality = data["config"]["personality"]
    config.model = data["config"]["model"]
    return data["messages"], config

def export_markdown(messages, filename):
    with open(f"chat_logs/{filename}", 'w', encoding='utf-8') as f:
        for msg in messages:
            if msg["role"] != "system":
                f.write(f"## {msg['role'].title()}\n\n")
                f.write(f"{msg['content']}\n\n")

def calculate_cost(input_tokens, output_tokens, config, cache_hit_ratio=0.3):
    """Calculate the cost of a conversation"""
    cache_miss_tokens = input_tokens * (1 - cache_hit_ratio)
    cache_hit_tokens = input_tokens * cache_hit_ratio
    
    return (
        (cache_miss_tokens / 1_000_000) * config.input_cache_miss_price +
        (cache_hit_tokens / 1_000_000) * config.input_cache_hit_price +
        (output_tokens / 1_000_000) * config.output_price
    )

def chat_with_ai(client):
    os.makedirs("chat_logs", exist_ok=True)
    config = ChatConfig()
    
    console.print("[bold blue]Enhanced DeepSeek AI Chat[/bold blue]")
    console.print("\n[yellow]Commands:[/yellow]")
    console.print("- 'quit': Exit the chat")
    console.print("- 'toggle': Switch memory mode")
    console.print("- 'save': Save conversation")
    console.print("- 'load <filename>': Load conversation")
    console.print("- 'clear': Clear conversation")
    console.print("- 'temp <0.0-1.0>': Set temperature")
    console.print("- 'personality <type>': Change AI personality")
    console.print("- 'export <filename>': Export as markdown")
    console.print("- 'tokens': Show token count")
    console.print("- 'help': Show commands")
    console.print("- 'price': Show current pricing")
    console.print("- 'setprice <cache_miss> <cache_hit> <output>': Set pricing")
    console.print("- 'cache <0.0-1.0>': Set cache hit ratio")
    console.print("-" * 50)
    
    keep_history = False
    messages = [
        {"role": "system", "content": PERSONALITIES[config.personality]}
    ]
    
    console.print(f"\nCurrent mode: [green]{'Stateful' if keep_history else 'Stateless'}[/green]")
    
    total_cost = 0.0
    cache_hit_ratio = 0.0  # Default cache hit ratio (0% cache hits)
    
    while True:
        user_input = input("\nYou: ").strip()
        
        if user_input.lower() == 'quit':
            if keep_history and len(messages) > 1:
                save_filename = save_conversation(messages, config)
                console.print(f"\n[green]Conversation saved to: {save_filename}[/green]")
            break
            
        if user_input.lower() == 'tokens':
            token_count = count_tokens(messages)
            console.print(f"\n[yellow]Current token count: {token_count}[/yellow]")
            continue
            
        if user_input.lower().startswith('temp '):
            try:
                new_temp = float(user_input[5:])
                if 0 <= new_temp <= 1:
                    config.temperature = new_temp
                    console.print(f"\n[green]Temperature set to: {new_temp}[/green]")
                else:
                    console.print("\n[red]Temperature must be between 0 and 1[/red]")
            except ValueError:
                console.print("\n[red]Invalid temperature value[/red]")
            continue
            
        if user_input.lower().startswith('personality '):
            new_personality = user_input[12:].strip()
            if new_personality in PERSONALITIES:
                config.personality = new_personality
                messages[0]["content"] = PERSONALITIES[new_personality]
                console.print(f"\n[green]Switched to {new_personality} personality[/green]")
            else:
                console.print(f"\n[red]Available personalities: {', '.join(PERSONALITIES.keys())}[/red]")
            continue
            
        if user_input.lower().startswith('export '):
            filename = user_input[7:].strip()
            if not filename.endswith('.md'):
                filename += '.md'
            export_markdown(messages, filename)
            console.print(f"\n[green]Exported to: {filename}[/green]")
            continue
            
        if user_input.lower() == 'toggle':
            keep_history = not keep_history
            messages = [{"role": "system", "content": PERSONALITIES[config.personality]}]
            console.print(f"\nSwitched to: [green]{'Stateful' if keep_history else 'Stateless'}[/green]")
            continue
            
        if user_input.lower() == 'save':
            save_filename = save_conversation(messages, config)
            console.print(f"\n[green]Conversation saved to: {save_filename}[/green]")
            continue
            
        if user_input.lower().startswith('load '):
            filename = user_input[5:].strip()
            if not filename.endswith('.json'):
                filename += '.json'
            messages, config = load_conversation(filename)
            console.print(f"\n[green]Conversation loaded from: {filename}[/green]")
            continue
            
        if user_input.lower() == 'clear':
            messages = [{"role": "system", "content": PERSONALITIES[config.personality]}]
            console.print("\n[green]Conversation cleared[/green]")
            continue
            
        if user_input.lower() == 'help':
            console.print("\n[yellow]Available commands:[/yellow]")
            console.print("- 'quit': Exit the chat")
            console.print("- 'toggle': Switch memory mode")
            console.print("- 'save': Save conversation")
            console.print("- 'load <filename>': Load conversation")
            console.print("- 'clear': Clear conversation")
            console.print("- 'temp <0.0-1.0>': Set temperature")
            console.print("- 'personality <type>': Change AI personality")
            console.print("- 'export <filename>': Export as markdown")
            console.print("- 'tokens': Show token count")
            console.print("- 'help': Show commands")
            console.print("- 'price': Show current pricing")
            console.print("- 'setprice <cache_miss> <cache_hit> <output>': Set pricing")
            console.print("- 'cache <0.0-1.0>': Set cache hit ratio")
            continue
            
        if user_input.lower() == 'price':
            console.print(f"\n[yellow]Current pricing:[/yellow]")
            console.print(f"Input Cache Miss: ${config.input_cache_miss_price:.2f} per million tokens")
            console.print(f"Input Cache Hit: ${config.input_cache_hit_price:.2f} per million tokens")
            console.print(f"Output: ${config.output_price:.2f} per million tokens")
            console.print(f"Total cost so far: ${total_cost:.4f}")
            continue
            
        if user_input.lower().startswith('setprice '):
            try:
                parts = user_input[9:].split()
                if len(parts) == 3:
                    config.input_cache_miss_price = float(parts[0])
                    config.input_cache_hit_price = float(parts[1])
                    config.output_price = float(parts[2])
                    console.print(f"\n[green]Pricing updated:[/green]")
                    console.print(f"Input Cache Miss: ${config.input_cache_miss_price:.2f} per million tokens")
                    console.print(f"Input Cache Hit: ${config.input_cache_hit_price:.2f} per million tokens")
                    console.print(f"Output: ${config.output_price:.2f} per million tokens")
                else:
                    console.print("\n[red]Usage: setprice <cache_miss> <cache_hit> <output>[/red]")
            except ValueError:
                console.print("\n[red]Invalid price values[/red]")
            continue
            
        if user_input.lower().startswith('cache '):
            try:
                new_ratio = float(user_input[6:])
                if 0 <= new_ratio <= 1:
                    cache_hit_ratio = new_ratio
                    console.print(f"\n[green]Cache hit ratio set to: {new_ratio:.2%}[/green]")
                else:
                    console.print("\n[red]Cache ratio must be between 0 and 1[/red]")
            except ValueError:
                console.print("\n[red]Invalid cache ratio value[/red]")
            continue
            
        try:
            current_messages = messages + [{"role": "user", "content": user_input}] if keep_history else [
                {"role": "system", "content": PERSONALITIES[config.personality]},
                {"role": "user", "content": user_input}
            ]
            
            logging.info("Sending request to DeepSeek API...")
            response = client.chat.completions.create(
                model=config.model,
                messages=current_messages,
                temperature=config.temperature,
                max_tokens=config.max_tokens,
                stream=True
            )
            logging.info("Received response stream from DeepSeek API")
            
            ai_response = ""
            console.print("\n[bold cyan]AI:[/bold cyan]")
            chunk_received = False
            
            for chunk in response:
                chunk_received = True
                logging.debug(f"Received chunk: {chunk}")  # Debug the chunk structure
                
                if hasattr(chunk.choices[0], 'delta'):
                    delta = chunk.choices[0].delta
                    if hasattr(delta, 'content') and delta.content:
                        content = delta.content
                        ai_response += content
                        print(content, end="", flush=True)
                elif hasattr(chunk.choices[0], 'text'):  # Some APIs use 'text' instead of 'delta'
                    content = chunk.choices[0].text
                    ai_response += content
                    print(content, end="", flush=True)
            
            if not chunk_received:
                logging.warning("No chunks received from the stream")
                console.print("\n[yellow]Warning: No response received from DeepSeek[/yellow]")
            elif not ai_response:
                logging.warning("Chunks received but no content extracted")
                console.print("\n[yellow]Warning: Received response but couldn't extract content[/yellow]")
            
            print()  # Newline after streaming
            
            if keep_history:
                messages.append({"role": "user", "content": user_input})
                messages.append({"role": "assistant", "content": ai_response})
            
            # Calculate cost
            input_tokens = count_tokens([{"role": "user", "content": user_input}])
            output_tokens = count_tokens([{"role": "assistant", "content": ai_response}])
            total_cost += calculate_cost(input_tokens, output_tokens, config, cache_hit_ratio)
            
        except Exception as e:
            console.print(f"\n[red]Error: {str(e)}[/red]")
            logging.error(f"API Error: {str(e)}", exc_info=True)  # Added exc_info for full traceback
            
    console.print(f"\n[yellow]Total conversation cost: ${total_cost:.4f}[/yellow]")
    
if __name__ == "__main__":
    client = create_chat_client()
    chat_with_ai(client)