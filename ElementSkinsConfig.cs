using System.Collections.Generic;

namespace ElementSkins
{
    /// <summary>
    /// 干板墙元素皮肤配置：定义要注册的固体元素列表及其对应的 facade id / 显示名 / 描述。
    ///
    /// 方案：facade.AnimFile 直接指向元素自身的 kanim（element.substance.anim）。
    ///   - UI 缩略图（建造列表网格 / 工具栏 / 预览大图）全部走
    ///     Def.GetUISpriteFromMultiObjectAnim(Assets.GetAnim(facade.AnimFile), "ui")，
    ///     因元素 kanim 自带 "ui" symbol（掉落物图标），缩略图天然正确，无需任何 UI patch。
    ///   - 建筑施工蓝图 / 建成瞬间外观 = 元素 kanim 样式（建成后由 backwall 方案立即替换为自然纹理）。
    /// </summary>
    public static class ElementSkinsConfig
    {
        /// <summary>
        /// 每个元素皮肤的注册信息
        /// </summary>
        public class WallSkinEntry
        {
            public string FacadeId;       // 如 "ExteriorWall_IgneousRock"
            public string DisplayName;    // 如 "Igneous Rock"
            public string Description;    // 如 "A wallpaper with the texture of igneous rock."
            public SimHashes Element;     // 对应固体元素

            public WallSkinEntry(SimHashes element, string displayName, string description)
            {
                this.Element = element;
                this.FacadeId = "ExteriorWall_" + element.ToString();
                this.DisplayName = displayName;
                this.Description = description;
            }

            /// <summary>
            /// 解析该 facade 使用的 kanim 名。
            ///
            /// 【第1步·地基验证】：所有皮肤暂时统一返回 mod 打包的白墙副本 kanim
            ///   "elementskins_white_kanim"（来自 anim/skins/elementskins_white/）。
            ///   目的：验证"自定义 kanim 作为 facade.AnimFile"的完整链路是否正常
            ///   （建造列表图标、建造蓝图、建筑外观均应显示为白墙，与原版一致）。
            ///
            /// 【第2步·运行时合成】（验证通过后）：改为每个元素返回各自合成的 kanim
            ///   （白墙底图 + 元素 ui 图标），实现 UI 图标=元素、建筑=白墙。
            /// </summary>
            public string ResolveAnimFile()
            {
                return "elementskins_white_kanim";
            }
        }

        /// <summary>
        /// facade id → SimHashes 快速查找表（延迟初始化）
        /// </summary>
        private static Dictionary<string, SimHashes> _facadeToElement;

        /// <summary>
        /// 尝试从 facade id 获取对应的皮肤元素
        /// </summary>
        public static bool TryGetElementForFacade(string facadeId, out SimHashes element)
        {
            if (_facadeToElement == null)
            {
                _facadeToElement = new Dictionary<string, SimHashes>();
                foreach (var skin in WallSkins)
                {
                    _facadeToElement[skin.FacadeId] = skin.Element;
                }
            }
            return _facadeToElement.TryGetValue(facadeId, out element);
        }

        /// <summary>
        /// 要注册的干板墙元素皮肤列表（16 个固体元素）。
        /// AnimFile 在注册时通过 ResolveAnimFile() 运行时获取元素自身 kanim。
        /// </summary>
        public static readonly List<WallSkinEntry> WallSkins = new List<WallSkinEntry>
        {
            new WallSkinEntry(SimHashes.IgneousRock,   "Igneous Rock",   "A wallpaper with the texture of igneous rock."),
            new WallSkinEntry(SimHashes.Granite,       "Granite",        "A wallpaper with the texture of granite."),
            new WallSkinEntry(SimHashes.SandStone,     "Sandstone",      "A wallpaper with the texture of sandstone."),
            new WallSkinEntry(SimHashes.Obsidian,      "Obsidian",       "A wallpaper with the texture of obsidian."),
            new WallSkinEntry(SimHashes.Ice,           "Ice",            "A wallpaper with the texture of ice."),
            new WallSkinEntry(SimHashes.Iron,          "Iron",           "A wallpaper with the texture of iron."),
            new WallSkinEntry(SimHashes.Copper,        "Copper",         "A wallpaper with the texture of copper."),
            new WallSkinEntry(SimHashes.Gold,          "Gold",           "A wallpaper with the texture of gold."),
            new WallSkinEntry(SimHashes.Steel,         "Steel",          "A wallpaper with the texture of steel."),
            new WallSkinEntry(SimHashes.Katairite,     "Abyssalite",     "A wallpaper with the texture of abyssalite."),
            new WallSkinEntry(SimHashes.MaficRock,     "Mafic Rock",     "A wallpaper with the texture of mafic rock."),
            new WallSkinEntry(SimHashes.Niobium,       "Niobium",        "A wallpaper with the texture of niobium."),
            new WallSkinEntry(SimHashes.Diamond,       "Diamond",        "A wallpaper with the texture of diamond."),
            new WallSkinEntry(SimHashes.Wolframite,    "Wolframite",     "A wallpaper with the texture of wolframite."),
            new WallSkinEntry(SimHashes.Lead,          "Lead",           "A wallpaper with the texture of lead."),
            new WallSkinEntry(SimHashes.Aluminum,      "Aluminum",       "A wallpaper with the texture of aluminum."),
        };
    }
}
