namespace AdventureTime.Application.Config;

// Configuration class for OpenAI
public class OpenAiConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-5"; // Can be "gpt-5", "gpt-5-mini", or "gpt-5-nano"
}