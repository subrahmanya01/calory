namespace Calory.ImageToCalory.Api.Services
{
    public interface IPromptService
    {
        Task<string> GetPrompt(AiPrompt prompt);
    }
}
