namespace Calory.ImageToCalory.Api.Services
{
    public enum AiPrompt
    {
        ImageToCalory = 1,
    }

    public interface IPromptService
    {
        Task<string> GetPrompt(AiPrompt prompt);
    }
    public class PromptService : IPromptService
    {
        private static Dictionary<AiPrompt, string> _prompts = new Dictionary<AiPrompt, string>();
        public async Task<string> GetPrompt(AiPrompt prompt)
        {
            var promptEnumToString = prompt.ToString();

            if (_prompts.TryGetValue(prompt, out var promptText))
            {
                return promptText;
            }
            else
            {
                var res = await File.ReadAllTextAsync($"Prompts/{promptEnumToString}.txt");
                _prompts[prompt] = res;
                return res;
            }
        }

    }
}
