namespace Calory.ImageToCalory.Api.AiModelStrategy
{
    public interface IAiModelStrategyResolver
    {
        IAiModelStrategy Resolve(string model);
    }
}
