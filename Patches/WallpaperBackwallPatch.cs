using HarmonyLib;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 核心逻辑：
    /// 1. 干板墙应用元素皮肤时，在该格子写入 backwall 元素（GroundRenderer 自动渲染自然纹理）
    /// 2. 写入后立即触发干板墙自毁（拆除并归还建材）
    /// 3. backwall 保留，视觉效果 = 纯自然元素背景墙
    /// 
    /// 流程：建造干板墙 → 选元素皮肤 → 皮肤应用 → 写 backwall → 自动拆除干板墙 → 建材归还
    /// 结果：玩家花费建材，获得一面自然元素背景墙 + 建材归还（净消耗 = 0 建材，只是放置背景墙）
    /// </summary>
    public class WallpaperBackwallPatch
    {
        /// <summary>
        /// 皮肤应用时 → 设置 backwall + 自动拆除干板墙
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacade), nameof(BuildingFacade.ApplyBuildingFacade))]
        public static class BuildingFacade_ApplyBuildingFacade_Patch
        {
            public static void Postfix(BuildingFacade __instance)
            {
                if (__instance == null)
                    return;

                // 只对干板墙生效
                var building = __instance.GetComponent<Building>();
                if (building == null || building.Def.PrefabID != "ExteriorWall")
                    return;

                // 关键：只有建造完成的建筑（有 BuildingComplete 组件）才写入 backwall
                // 施工中的蓝图（Constructable）不处理，避免"供应材料时 backwall 被挤掉"的问题
                if (__instance.GetComponent<BuildingComplete>() == null)
                    return;

                // 通过 CurrentFacade 获取当前皮肤 id（不依赖方法参数注入）
                string facadeId = __instance.CurrentFacade;
                if (string.IsNullOrEmpty(facadeId))
                    return;

                // 检查是否是我们的元素皮肤
                if (!ElementSkinsConfig.TryGetElementForFacade(facadeId, out SimHashes elementHash))
                    return;

                Element element = ElementLoader.FindElementByHash(elementHash);
                if (element == null)
                    return;

                int cell = Grid.PosToCell(__instance.transform.GetPosition());
                if (!Grid.IsValidCell(cell))
                    return;

                // 写入 backwall 元素数据（极小 mass，安全温度）
                float safeTemp = Mathf.Clamp(element.defaultValues.temperature, element.lowTemp + 10f, element.highTemp - 10f);
                SimMessages.SetBackwallData(cell, element.idx, 0.001f, safeTemp);

                // 自动拆除干板墙（归还建材给玩家）
                var deconstructable = __instance.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    // 延迟一帧执行，避免在 ApplyBuildingFacade 调用链中删除自身导致问题
                    GameScheduler.Instance.ScheduleNextFrame("ElementSkins_AutoDeconstruct", delegate(object _)
                    {
                        if (deconstructable != null && deconstructable.gameObject != null)
                        {
                            deconstructable.ForceDestroyAndGetMaterials();
                        }
                    });
                }
            }
        }
    }
}
