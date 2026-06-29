using Database;
using HarmonyLib;
using UnityEngine;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 干板墙元素皮肤 facade 注册。
    ///
    /// 方案：facade.AnimFile 直接指向元素自身 kanim（element.substance.anim）。
    ///   - UI 缩略图（建造列表网格 / 工具栏 / 预览大图）均走
    ///     Def.GetUISpriteFromMultiObjectAnim(Assets.GetAnim(facade.AnimFile), "ui")，
    ///     元素 kanim 自带 "ui" symbol（掉落物图标），缩略图天然正确，无需任何 UI patch。
    ///   - 建筑施工蓝图 / 建成瞬间外观 = 元素 kanim 样式，建成后由 backwall 方案立即替换为自然纹理。
    ///
    /// 时序：Assets.LoadAnims（kanim 就绪）→ SubstanceListHookup（substance.anim 绑定）
    ///   → CreatePrefabs → Db 初始化 → BuildingFacades.PostProcess（本 Prefix）。
    ///   因此注册时 element.substance.anim 已就绪。
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
                    if (__instance.TryGet(skin.FacadeId) != null)
                        continue;

                    // AnimFile 运行时取元素自身 kanim，使 UI 缩略图自动显示元素掉落物图标
                    string animFile = skin.ResolveAnimFile();

                    __instance.Add(
                        id: skin.FacadeId,
                        Name: (LocString)skin.DisplayName,
                        Desc: (LocString)skin.Description,
                        rarity: PermitRarity.Universal,
                        prefabId: "ExteriorWall",
                        animFile: animFile,
                        workables: null,
                        requiredDlcIds: null,
                        forbiddenDlcIds: null,
                        data: null
                    );

                    // 同步追加到 PermitResources.resources（否则 FacadeSelectionPanel 找不到 → UI 不显示）
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
