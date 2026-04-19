from langchain_ollama import ChatOllama
from config import settings

def get_llm(fast: bool = False) -> ChatOllama:
    model = settings.ollama_fast_model if fast else settings.ollama_model
    return ChatOllama(model=model, base_url=settings.ollama_base_url, temperature=0)
