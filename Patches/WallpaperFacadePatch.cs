using System.Collections.Generic;
using Database;
using HarmonyLib;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 在 Db.PostProcess 之前注入干板墙元素皮肤 facade，并手动调用 Init() 确保皮肤
    /// 被加入 BuildingDef.AvailableFacades。
    /// 
    /// 时序：
    ///   Db.Initialize() → BuildingFacades 构造（从 Blueprints 加载）
    ///   → [本 Postfix] 追加自定义 facade + 手动 Init()
    ///   → Db.PostProcess() → BuildingFacades.PostProcess()（对已有 facade 再次 Init，幂等安全）
    /// 
    /// 注册的皮肤使用 PermitRarity.Universal = 默认解锁。
    /// facade id 使用 "ExteriorWall_<Element>" 格式（无 permit_ 前缀 = 免费皮肤）。
    /// </summary>
    public class WallpaperFacadePatch
    {
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix(Db __instance)
            {
                Debug.Log($"[{ElementSkinsMod.ModId}] Registering {ElementSkinsConfig.WallSkins.Count} element wallpaper facades...");

                var facadesDb = Db.GetBuildingFacades();
                int registered = 0;

                foreach (var skin in ElementSkinsConfig.WallSkins)
                {
                    // 防止重复注册（热重载保护）
                    if (facadesDb.TryGet(skin.FacadeId) != null)
                    {
                        continue;
                    }

                    // 检查 animFile 是否存在（避免注册无效 kanim 导致崩溃）
                    KAnimFile animFile;
                    if (!Assets.TryGetAnim(skin.AnimFile, out animFile))
                    {
                        Debug.LogWarning($"[{ElementSkinsMod.ModId}] Skipping {skin.FacadeId}: animFile '{skin.AnimFile}' not found.");
                        continue;
                    }

                    // 注册 BuildingFacadeResource
                    facadesDb.Add(
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

                    // 手动调用 Init()，将 facade 注册进 BuildingDef.AvailableFacades
                    // （Db.PostProcess 会再调一次，但 Init() 是幂等的——AddFacade 内部有去重）
                    var resource = facadesDb.TryGet(skin.FacadeId);
                    if (resource != null)
                    {
                        resource.Init();
                    }

                    registered++;
                }

                Debug.Log($"[{ElementSkinsMod.ModId}] Successfully registered {registered} element wallpaper facades.");
            }
        }
    }
}
