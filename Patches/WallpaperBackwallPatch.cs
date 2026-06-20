using HarmonyLib;
using Database;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 核心逻辑：当干板墙应用元素皮肤时，在该格子的 Sim backwall 层写入目标元素，
    /// 由 GroundRenderer 自动渲染该元素的自然纹理（含大块跨格 worldUV 效果）。
    /// 
    /// 只 patch ApplyBuildingFacade（已验证可工作）。
    /// OnSpawn 时游戏内部会调用 ApplyBuildingFacade，所以存档加载也会触发。
    /// </summary>
    public class WallpaperBackwallPatch
    {
        /// <summary>
        /// 皮肤应用时 → 设置或清除 backwall 元素
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacade), nameof(BuildingFacade.ApplyBuildingFacade))]
        public static class BuildingFacade_ApplyBuildingFacade_Patch
        {
            public static void Postfix(BuildingFacade __instance, BuildingFacadeResource facade)
            {
                if (__instance == null)
                    return;

                // 只对干板墙生效
                var building = __instance.GetComponent<Building>();
                if (building == null || building.Def.PrefabID != "ExteriorWall")
                    return;

                int cell = Grid.PosToCell(__instance.transform.GetPosition());
                if (!Grid.IsValidCell(cell))
                    return;

                // facade 为 null 时是清除操作（ClearFacade 内部调用）
                if (facade == null)
                {
                    ClearBackwallAt(cell);
                    return;
                }

                // 检查是否是我们的元素皮肤
                if (!ElementSkinsConfig.TryGetElementForFacade(facade.Id, out SimHashes elementHash))
                {
                    // 切到非元素皮肤时，清除之前可能设置的 backwall
                    ClearBackwallAt(cell);
                    return;
                }

                Element element = ElementLoader.FindElementByHash(elementHash);
                if (element == null)
                    return;

                // 写入 backwall 元素数据
                SimMessages.SetBackwallData(cell, element.idx, 100f, element.defaultValues.temperature);
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
                if (__instance == null)
                    return;

                var building = __instance.GetComponent<Building>();
                if (building == null || building.Def.PrefabID != "ExteriorWall")
                    return;

                int cell = Grid.PosToCell(__instance.transform.GetPosition());
                if (Grid.IsValidCell(cell))
                {
                    ClearBackwallAt(cell);
                }
            }
        }

        private static void ClearBackwallAt(int cell)
        {
            Element vacuum = ElementLoader.FindElementByHash(SimHashes.Vacuum);
            if (vacuum != null)
            {
                SimMessages.SetBackwallData(cell, vacuum.idx, 0f, 0f);
            }
        }
    }
}
