﻿using eft_dma_radar.Tarkov.Loot;
using eft_dma_radar.UI.Radar;

namespace eft_dma_radar.UI.LootFilters
{
    /// <summary>
    /// Enumerable Loot Filter Class.
    /// </summary>
    internal static class LootFilter
    {
        public static string SearchString;
        public static bool ShowMeds;
        public static bool ShowFood;
        public static bool ShowBackpacks;

        private static bool ShowQuestItems => MainForm.Config.QuestHelper.Enabled;

        /// <summary>
        /// Creates a loot filter based on current Loot Filter settings.
        /// </summary>
        /// <returns>Loot Filter Predicate.</returns>
        public static Predicate<LootItem> Create()
        {
            var search = SearchString?.Trim();
            bool usePrices = string.IsNullOrEmpty(search);
            if (usePrices)
            {
                Predicate<LootItem> p = (x) => // Default Predicate
                {
                    return (x.IsRegularLoot || x.IsValuableLoot || x.IsImportant || x.IsWishlisted) ||
                                (ShowQuestItems && x.IsQuestCondition) ||
                                (LootFilter.ShowBackpacks && x.IsBackpack) ||
                                (LootFilter.ShowMeds && x.IsMeds) ||
                                (LootFilter.ShowFood && x.IsFood);
                };
                return (item) =>
                {
                    if (item is LootAirdrop airdrop)
                    {
                        return true;
                    }
                    if (item is LootCorpse corpse)
                    {
                        return true;
                    }
                    if (p(item))
                    {
                        if (item is LootContainer container)
                        {
                            container.SetFilter(p);
                        }
                        return true;
                    }
                    return false;
                };
            }
            else // Loot Search
            {
                var names = search!.Split(',').Select(a => a.Trim()).ToArray();
                Predicate<LootItem> p = (x) => // Search Predicate
                {
                    return names.Any(a => x.Name.Contains(a, StringComparison.OrdinalIgnoreCase));
                };
                return (item) =>
                {
                    if (item is LootAirdrop airdrop)
                    {
                        return true;
                    }
                    if (item.ContainsSearchPredicate(p))
                    {
                        if (item is LootContainer container)
                        {
                            container.SetFilter(p);
                        }
                        return true;
                    }
                    return false;
                };
            }
        }
    }
}