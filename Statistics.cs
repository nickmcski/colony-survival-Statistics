﻿using NetworkUI;
using NetworkUI.Items;
using UnityEngine;
using Shared;
using Jobs;
using static ModLoader;
using System.Collections.Generic;
using Recipes;
using NPC;
using System;
using Pipliz.JSON;

namespace grasmanek94.Statistics
{
    [ModManager]
    public static class Statistics
    {
        static Dictionary<Colony, ColonyStatistics> colonyStats;

        [ModCallback(EModCallbackType.OnAssemblyLoaded, "OnAssemblyLoaded")]
        static void OnAssemblyLoaded(string assemblyPath)
        {
            colonyStats = new Dictionary<Colony, ColonyStatistics>();
        }

        [ModCallback(EModCallbackType.OnConstructTooltipUI, "OnConstructTooltipUI")]
        static void OnConstructTooltipUI(ConstructTooltipUIData data)
        {
            if(data.hoverType != ETooltipHoverType.Item)
            {
                return;
            }

            TimedItemStatistics stats = GetColonyStats(data.player.ActiveColony).GetTimedItemStats(data.hoverItem);
      
            var statlist = stats.Averages();

            foreach (var stat in statlist)
            {
                string span = new TimeSpan(stats.PeriodsToGameHours(stat.Periods), 0, 0).ToHumanReadableString();

                data.menu.Items.Add(new Line(Color.white, 2, -1, 10, 2));

                data.menu.Items.Add(new Label(new LabelData("Last " + span + " average:", TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                data.menu.Items.Add(new Label(new LabelData("Produced / Consumed: " + stat.AverageProduced.ToString() + " / " + stat.AverageConsumed.ToString(), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                // data.menu.Items.Add(new Label(new LabelData("Producers / Consumers: " + stat.AverageProducers.ToString() + " / " + stat.AverageConsumers.ToString(), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
                data.menu.Items.Add(new Label(new LabelData("Inventory Added / Removed: " + stat.AverageInventoryAdded.ToString() + " / " + stat.AverageInventoryRemoved.ToString(), TextAnchor.MiddleLeft, 17, LabelData.ELocalizationType.Sentence), -1));
            }
        }

        static void AddProducerConsumer(NPCBase npc, IJob job)
        {
            if (npc == null || job == null || npc.Colony == null || !job.IsValid)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);

            // add producer and consumer here somehow
        }

        static void RemoveProducerConsumer(NPCBase npc, IJob job)
        {
            if (npc == null || job == null || npc.Colony == null || !job.IsValid)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(npc.Colony);

            // remove producer and consumer here somehow
        }

        [ModCallback(EModCallbackType.OnNPCDied, "OnNPCDied")]
        static void OnNPCDied(NPCBase npc)
        {
            RemoveProducerConsumer(npc, npc?.Job);
        }

        [ModCallback(EModCallbackType.OnNPCLoaded, "OnNPCLoaded")]
        static void OnNPCLoaded(NPCBase npc, JSONNode json)
        {
            AddProducerConsumer(npc, npc?.Job);
        }

        [ModCallback(EModCallbackType.OnNPCJobChanged, "OnNPCJobChanged")]
        static void OnNPCJobChanged(ValueTuple<NPCBase, IJob, IJob> data)
        {
            NPCBase npc = data.Item1;
            IJob oldJob = data.Item2;
            IJob newJob = data.Item3;

            if (npc == null || npc.Colony == null)
            {
                return;
            }

            RemoveProducerConsumer(npc, oldJob);
            AddProducerConsumer(npc, newJob);
        }

        [ModCallback(EModCallbackType.OnNPCGathered, "OnNPCGathered")]
        static void OnNPCGathered(IJob job, Vector3Int pos, List<ItemTypes.ItemTypeDrops> items)
        {
            if (job == null || job.NPC == null || job.NPC.Colony == null || items == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(job.NPC.Colony);

            foreach (var item in items)
            {
                stats.GetTimedItemStats(item.Type).Produce(item.Amount);
            }
        }

        [ModCallback(EModCallbackType.OnNPCCraftedRecipe, "OnNPCCraftedRecipe")]
        static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> result)
        {
            if (job == null || job.NPC == null || job.NPC.Colony == null)
            {
                return;
            }

            ColonyStatistics stats = GetColonyStats(job.NPC.Colony);

            if(recipe != null)
            {
                foreach(var item in recipe.Requirements)
                {
                    stats.GetTimedItemStats(item.Type).Consume(item.Amount);
                }
            }

            if (result != null)
            {
                foreach (var item in result)
                {
                    stats.GetTimedItemStats(item.Type).Produce(item.Amount);
                }
            }
        }

        // var other_sources = BlockFarmAreaJobDefinition / PlacedBlockType
        // var sources = ServerManager.RecipeStorage.SourceBlockTypesPerProductionItem;

        /*
            Stats:
                - Production (Output)
                - Consumption (Input)
                - Requirement
                - Worker amount
                - Amount of farms currently producing
                - Amount of crafting places currently producing
        */

        // Stockpile class (Harmony)
        //  public bool TryRemove(ushort type, int amount = 1, bool sendUpdate = true)
        //  public bool TryRemove(IList<InventoryItem> toRemoveItems)
        //  public bool TryRemove(InventoryItem item)
        //  public void Add(ushort type, int amount = 1)
        //  public void Add(InventoryItem item)
        //  public void Add(IList<InventoryItem> list)
        //  public void AddEnumerable<T>(T toAddItems) where T : IEnumerable<InventoryItem>
        //  public bool TryRemoveFood(ref float currentFood, float desiredFoodAddition)
        //  public void Clear()
        // AreaJobTracker
        // BlockJobInstance??

        /*[ModCallback(EModCallbackType.OnLoadingColony, "OnLoadingColony")]
        static void OnLoadingColony(Colony colony, JSONNode node)
        {
            GetColonyStats(colony);
        }

        [ModCallback(EModCallbackType.OnCreatedColony, "OnCreatedColony")]
        static void OnCreatedColony(Colony colony)
        {
            GetColonyStats(colony);
        }

        [ModCallback(EModCallbackType.OnActiveColonyChanges, "OnActiveColonyChanges")]
        static void OnActiveColonyChanges(Players.Player player, Colony oldColony, Colony newColony)
        {
            GetColonyStats(oldColony);
            GetColonyStats(newColony);
        }

        [ModCallback(EModCallbackType.OnPlayerRespawn, "OnPlayerRespawn")]
        static void OnPlayerRespawn(Players.Player player)
        {
            GetColonyStats(player.ActiveColony);
        }*/

        static ColonyStatistics GetColonyStats(Colony colony)
        {
            if(colony == null)
            {
                return null;
            }

            ColonyStatistics stats;
            if(!colonyStats.TryGetValue(colony, out stats))
            {
                stats = new ColonyStatistics();
                colonyStats.Add(colony, stats);
            }

            return stats;
        }
    }
}
