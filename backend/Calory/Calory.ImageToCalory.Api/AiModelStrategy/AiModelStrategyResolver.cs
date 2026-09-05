namespace Calory.ImageToCalory.Api.AiModelStrategy
{
    public class AiModelStrategyResolver : IAiModelStrategyResolver
    {
        private readonly IEnumerable<IAiModelStrategy> _strategies;

        public AiModelStrategyResolver( IEnumerable<IAiModelStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IAiModelStrategy Resolve(string model)
        {
            var strategy = _strategies.FirstOrDefault( x => x.ModelName.Equals( model, StringComparison.OrdinalIgnoreCase));

            if (strategy == null)
            {
                throw new ArgumentException($"Unsupported AI model: {model}");
            }

            return strategy;
        }
    }
}
