using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using TraderLLRequirements.Models;

namespace TraderLLRequirements;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.kobethuy.TraderLLRequirement";
    public override string Name { get; init; } = "Trader LL Requirement";
    public override string Author { get; init; } = "Kobe Thuy";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class TraderLLRequirements(
    ISptLogger<TraderLLRequirements> logger,
    ModHelper modHelper,
    DatabaseService databaseService) : IOnLoad
{
    public required ModConfig ModConfig;
    public required DatabaseService DatabaseService;
    
    public Task OnLoad()
    {
        string pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        
        ModConfig = modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config\\config.json");

        DatabaseService = databaseService;
        
        ModifyTraderLLRequirements();
        
        logger.Success("[TraderLLRequirement] Mod loaded successfully.");
        return Task.CompletedTask;
    }

    private void ModifyTraderLLRequirements()
    {
        if (!ModConfig.Enabled)
            return;

        foreach (KeyValuePair<MongoId, List<ModConfig.LoyaltyLevelReq>> traderEntry in ModConfig.ModTraders)
        {
            Trader? trader = DatabaseService.GetTrader(traderEntry.Key);

            if (trader == null ||
                trader.Base.LoyaltyLevels == null)
            {
                logger.Error($"[TraderLLRequirement] Trader with ID ${traderEntry.Key} not found or has no loyalty levels. Skipping...");
                continue;
            }

            int i = 0;
            
            foreach (TraderLoyaltyLevel loyaltyLevel in trader.Base.LoyaltyLevels)
            {
                if (ModConfig.Debug)
                    logger.Info("[TraderLLRequirement] Proccessing trader " +
                                $"{trader.Base.Nickname} (ID: {traderEntry.Key}) " +
                                $"loyalty level {i + 1}");
                
                if (traderEntry.Value[i].MinLevel > 0)
                {
                    loyaltyLevel.MinLevel = traderEntry.Value[i].MinLevel;
                    
                    if (ModConfig.Debug)
                        logger.Info("[TraderLLRequirement] Set minLevel of trader " +
                                    $"{trader.Base.Nickname} (ID: {traderEntry.Key}) " +
                                    $"loyalty level {i + 1} to {loyaltyLevel.MinLevel}");
                }

                if (traderEntry.Value[i].MinSalesSum > 0)
                {
                    loyaltyLevel.MinSalesSum = traderEntry.Value[i].MinSalesSum;
                    
                    if (ModConfig.Debug)
                        logger.Info("[TraderLLRequirement] Set minSalesSum of trader " +
                                    $"{trader.Base.Nickname} (ID: {traderEntry.Key}) " +
                                    $"loyalty level {i + 1} to {loyaltyLevel.MinSalesSum}");
                }

                if (traderEntry.Value[i].MinStanding > 0)
                {
                    loyaltyLevel.MinStanding = traderEntry.Value[i].MinStanding;
                    
                    if (ModConfig.Debug)
                        logger.Info("[TraderLLRequirement] Set minStanding of trader " +
                                    $"{trader.Base.Nickname} (ID: {traderEntry.Key}) " +
                                    $"loyalty level {i + 1} to {loyaltyLevel.MinStanding}");
                }

                i++;
            }

        }
    }
}