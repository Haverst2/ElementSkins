using System.Collections.Generic;
using Database;
using HarmonyLib;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 在 BuildingFacades.PostProcess 之前注入干板墙元素皮肤 facade。
    /// 
    /// 时序（确保 Assets/Prefabs 都已就绪）：
    ///   Db.Initialize() → BuildingFacades 构造（从 Blueprints 加载）→ 其他资源加载
    ///   → Db.PostProcess() → BuildingFacades.PostProcess()
    ///     → [本 Prefix] 追加自定义 facade（此时 ExteriorWall prefab 和 kanim 都已存在）
    ///     → 原 PostProcess 继续执行 → 对所有 facade（含我们的）调用 Init()
    /// 
    /// 注册的皮肤使用 PermitRarity.Universal = 默认解锁。
    /// facade id 使用 "ExteriorWall_<Element>" 格式（无 permit_ 前缀 = 免费皮肤）。
    /// </summary>
    public class WallpaperFacadePatch
    {
        [HarmonyPatch(typeof(BuildingFacades), nameof(BuildingFacades.PostProcess))]
        public static class BuildingFacades_PostProcess_Patch
        {
            public static void Prefix(BuildingFacades __instance)
            {
                Debug.Log($"[{ElementSkinsMod.ModId}] Registering {ElementSkinsConfig.WallSkins.Count} element wallpaper facades...");

                int registered = 0;

                foreach (var skin in ElementSkinsConfig.WallSkins)
                {
                    // 防止重复注册（热重载保护）
                    if (__instance.TryGet(skin.FacadeId) != null)
                    {
                        continue;
                    }

                    // 注册 BuildingFacadeResource
                    // 在 PostProcess Prefix 中执行，此时 Assets 和 Prefabs 都已就绪
                    // PostProcess 会对所有 resource 调用 Init()，自动将 facade 加入 BuildingDef.AvailableFacades
                    __instance.Add(
                        id: skin.FacadeId,
                        Name: (LocString)skin.DisplayName,
                        Desc: (LocString)skin.Description,
                        rarity: PermitRarity.Universal,
                        prefabId: "ExteriorWall",
                        animFile: skin.AnimFile,
                        workables: null,
                        requiredDlcIds: null,
                        forbiddenDlcIds: null,
                        data: null
                    );

                    registered++;
                }

                Debug.Log($"[{ElementSkinsMod.ModId}] Successfully registered {registered} element wallpaper facades.");
            }
        }
    }
}
