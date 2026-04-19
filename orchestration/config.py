from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    ollama_base_url: str = "http://localhost:11434"
    ollama_model: str = "qwen2.5:14b"
    ollama_fast_model: str = "qwen2.5:3b"
    dotnet_api_base_url: str = "http://localhost:5000"

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

settings = Settings()
