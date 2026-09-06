using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Calory.Api.Mcp
{
    [McpServerToolType]
    public class HealthGoalTools
    {
        [McpServerTool]
        [Description("Searches for food items by name.")]
        public string SearchFood(
        [Description("The food name to search for")]
        string query)
        {
            return $"Searching for food: {query}";
        }
    }
}
