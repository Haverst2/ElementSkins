using HarmonyLib;
using Database;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 核心逻辑：当干板墙应用元素皮肤时，在该格子的 Sim backwall 层写入目标元素，
    /// 由 GroundRenderer 自动渲染该元素的自然纹理（含大块跨格 worldUV 效果）。
    /// 
    /// 拆除时通过 Deconstructable patch 清除 backwall 数据。
    /// 掉落物由 Deconstructable 控制（= 建造材料），不受 backwall 影响。
    /// </summary>
    public class WallpaperBackwallPatch
    {
        /// <summary>
        /// 玩家切换皮肤时 → 设置 backwall 元素
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacade), nameof(BuildingFacade.ApplyBuildingFacade))]
        public static class BuildingFacade_ApplyBuildingFacade_Patch
        {
            public static void Postfix(BuildingFacade __instance)
            {
                ApplyBackwall(__instance);
            }
        }

        /// <summary>
        /// 存档加载时恢复 backwall（OnSpawn 内部会调用 ApplyBuildingFacade，
        /// 所以上面的 patch 会自动触发。但为了安全，也直接 patch OnSpawn）
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacade), "OnSpawn")]
        public static class BuildingFacade_OnSpawn_Patch
        {
            public static void Postfix(BuildingFacade __instance)
            {
                ApplyBackwall(__instance);
            }
        }

        /// <summary>
        /// 恢复默认外观时清除 backwall
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacade), nameof(BuildingFacade.ApplyDefaultFacade))]
        public static class BuildingFacade_ApplyDefaultFacade_Patch
        {
            public static void Postfix(BuildingFacade __instance)
            {
                ClearBackwall(__instance);
            }
        }

        /// <summary>
        /// 建筑被拆除时清除 backwall。
        /// Patch Deconstructable.TriggerDestroy 而非 BuildingFacade.OnCleanUp
        /// （BuildingFacade 没有 override OnCleanUp）。
        /// </summary>
        [HarmonyPatch(typeof(Deconstructable), "TriggerDestroy")]
        public static class Deconstructable_TriggerDestroy_Patch
        {
            public static void Prefix(Deconstructable __instance)
            {
                var facade = __instance.GetComponent<BuildingFacade>();
                if (facade != null)
                {
                    ClearBackwall(facade);
                }
            }
        }

        private static void ApplyBackwall(BuildingFacade instance)
        {
            if (instance == null)
                return;

            // 只对干板墙（ExteriorWall）生效
            var building = instance.GetComponent<Building>();
            if (building == null || building.Def.PrefabID != "ExteriorWall")
                return;

            string currentFacade = instance.CurrentFacade;
            if (string.IsNullOrEmpty(currentFacade))
                return;

            // 检查是否是我们的元素皮肤
            if (!ElementSkinsConfig.TryGetElementForFacade(currentFacade, out SimHashes elementHash))
                return;

            Element element = ElementLoader.FindElementByHash(elementHash);
            if (element == null)
                return;

            int cell = Grid.PosToCell(instance.transform.GetPosition());
            if (!Grid.IsValidCell(cell))
                return;

            // 写入 backwall 元素数据
            SimMessages.SetBackwallData(cell, element.idx, 100f, element.defaultValues.temperature);
        }

        private static void ClearBackwall(BuildingFacade instance)
        {
            if (instance == null)
                return;

            var building = instance.GetComponent<Building>();
            if (building == null || building.Def.PrefabID != "ExteriorWall")
                return;

            string currentFacade = instance.CurrentFacade;
            if (string.IsNullOrEmpty(currentFacade))
                return;

            // 只清除我们设置的 backwall
            if (!ElementSkinsConfig.TryGetElementForFacade(currentFacade, out _))
                return;

            int cell = Grid.PosToCell(instance.transform.GetPosition());
            if (!Grid.IsValidCell(cell))
                return;

            // 清除：写入 Vacuum idx
            Element vacuum = ElementLoader.FindElementByHash(SimHashes.Vacuum);
            if (vacuum != null)
            {
                SimMessages.SetBackwallData(cell, vacuum.idx, 0f, 0f);
            }
        }
    }
}
