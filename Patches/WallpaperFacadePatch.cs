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
    ///   Db.Initialize() → PermitResources 构造 → BuildingFacades 构造（从 Blueprints 加载）
    ///   → PermitResources 构造函数中把 BuildingFacades.resources 拷贝到 Permits.resources
    ///   → Db.PostProcess() → BuildingFacades.PostProcess()
    ///     → [本 Prefix] 追加自定义 facade + 同步到 Permits.resources
    ///     → 原 PostProcess 继续执行 → 对所有 facade（含我们的）调用 Init()
    /// 
    /// ★ 关键：必须同时追加到 Db.Get().Permits.resources，
    ///   否则 FacadeSelectionPanel.RefreshTogglesForBuilding 中
    ///   Db.Get().Permits.TryGet(id) 会找不到 → UI 不显示。
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

                    // 注册 BuildingFacadeResource 到 BuildingFacades
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

                    // ★ 关键：同步追加到 PermitResources.resources
                    // PermitResources 构造时只做了一次 AddRange 拷贝，后续追加不会自动同步
                    // FacadeSelectionPanel 通过 Db.Get().Permits.TryGet(id) 查找，找不到就不显示
                    var newResource = __instance.TryGet(skin.FacadeId);
                    if (newResource != null)
                    {
                        Db.Get().Permits.resources.Add(newResource);
                    }

                    registered++;
                }

                Debug.Log($"[{ElementSkinsMod.ModId}] Successfully registered {registered} element wallpaper facades.");
            }
        }
    }
}
