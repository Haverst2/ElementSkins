using System.Collections.Generic;
using Database;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ElementSkins.Patches
{
    /// <summary>
    /// 干板墙元素皮肤 facade 注册 + UI 图标替换。
    /// 
    /// 设计目标：
    ///   - 建造蓝图外观 / 建成后外观 → 纯白壁纸（walls_basic_white_kanim）
    ///   - 蓝图选择面板图标 → 对应元素的掉落物缩略图（element.substance.anim 的 "ui" symbol）
    /// 
    /// AnimFile 保持 walls_basic_white_kanim（决定建筑世界外观），
    /// 通过 Patch UI 图标获取入口，让选择面板显示元素掉落物图标。
    /// </summary>
    public class WallpaperFacadePatch
    {
        /// <summary>
        /// 注册 facade 到 BuildingFacades + PermitResources
        /// </summary>
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

        /// <summary>
        /// Patch BuildingFacadeResource.GetPermitPresentationInfo
        /// 
        /// 影响范围：蓝图预览大图（KleiPermitDioramaVis_Wallpaper）
        /// 效果：显示元素掉落物图标而非白色壁纸缩略图
        /// </summary>
        [HarmonyPatch(typeof(BuildingFacadeResource), nameof(BuildingFacadeResource.GetPermitPresentationInfo))]
        public static class BuildingFacadeResource_GetPermitPresentationInfo_Patch
        {
            public static void Postfix(BuildingFacadeResource __instance, ref PermitPresentationInfo __result)
            {
                if (!ElementSkinsConfig.TryGetElementForFacade(__instance.Id, out SimHashes elementHash))
                    return;

                Sprite sprite = GetElementUISprite(elementHash);
                if (sprite != null)
                {
                    __result.sprite = sprite;
                }
            }
        }

        /// <summary>
        /// Patch Def.GetFacadeUISprite
        /// 
        /// 影响范围：PlanScreen 建造工具栏已选 facade 的图标
        /// 效果：显示元素掉落物图标
        /// </summary>
        [HarmonyPatch(typeof(Def), nameof(Def.GetFacadeUISprite))]
        public static class Def_GetFacadeUISprite_Patch
        {
            public static void Postfix(string facadeID, ref Sprite __result)
            {
                if (string.IsNullOrEmpty(facadeID))
                    return;

                if (!ElementSkinsConfig.TryGetElementForFacade(facadeID, out SimHashes elementHash))
                    return;

                Sprite sprite = GetElementUISprite(elementHash);
                if (sprite != null)
                {
                    __result = sprite;
                }
            }
        }

        /// <summary>
        /// Patch FacadeSelectionPanel.FacadeToggle 构造函数
        /// 
        /// 影响范围：蓝图选择面板中每个小网格图标
        /// 原始逻辑(第431行)：sprite = Def.GetUISpriteFromMultiObjectAnim(Assets.GetAnim(facade.AnimFile), "ui")
        /// 我们在构造完成后替换 FGImage 的 sprite 为元素掉落物图标
        /// </summary>
        [HarmonyPatch(typeof(FacadeSelectionPanel.FacadeToggle), MethodType.Constructor,
            new[] { typeof(string), typeof(string), typeof(GameObject) })]
        public static class FacadeToggle_Ctor_Patch
        {
            public static void Postfix(string buildingFacadeID, GameObject gameObject)
            {
                if (buildingFacadeID == "DEFAULT_FACADE")
                    return;

                if (!ElementSkinsConfig.TryGetElementForFacade(buildingFacadeID, out SimHashes elementHash))
                    return;

                Sprite sprite = GetElementUISprite(elementHash);
                if (sprite == null)
                    return;

                // 替换 FGImage 的 sprite
                HierarchyReferences refs = gameObject.GetComponent<HierarchyReferences>();
                if (refs != null)
                {
                    Image fgImage = refs.GetReference<Image>("FGImage");
                    if (fgImage != null)
                    {
                        fgImage.sprite = sprite;
                    }
                }
            }
        }

        /// <summary>
        /// 获取固体元素掉落物形态的 UI Sprite
        /// </summary>
        private static Sprite GetElementUISprite(SimHashes elementHash)
        {
            Element element = ElementLoader.FindElementByHash(elementHash);
            if (element == null || element.substance == null || element.substance.anim == null)
                return null;

            return Def.GetUISpriteFromMultiObjectAnim(element.substance.anim, "ui", false, "");
        }
    }
}
