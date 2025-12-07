using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace TraderLLRequirements.Models;

public class ModConfig
{
    [JsonPropertyName("modEnabled")]
    public bool Enabled { get; set; }
    [JsonPropertyName("modDebug")]
    public bool Debug { get; set; }
    [JsonPropertyName("traders")]
    public Dictionary<MongoId, List<LoyaltyLevelReq>> ModTraders { get; set; } = new();
    
    public class LoyaltyLevelReq
    {
        [JsonPropertyName("minLevel")]
        public int MinLevel { get; set; }
        [JsonPropertyName("minSalesSum")]
        public long MinSalesSum { get; set; }
        [JsonPropertyName("minStanding")]
        public double MinStanding { get; set; }
    }
}