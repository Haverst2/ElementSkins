using System.Collections.Generic;

namespace ElementSkins
{
    /// <summary>
    /// 干板墙元素皮肤配置：定义要注册的固体元素列表及其对应的 facade id / 显示名 / 描述 / animFile。
    /// 
    /// 当前阶段（POC）：复用游戏内已有的 walls kanim（如 walls_basic_white_kanim）来验证 facade 注册流程。
    /// 后续阶段：替换为实时烘焙的元素纹理 kanim。
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
            public string AnimFile;       // kanim 名，后续替换为实时合成的
            public SimHashes Element;     // 对应固体元素

            public WallSkinEntry(SimHashes element, string displayName, string description, string animFile)
            {
                this.Element = element;
                this.FacadeId = "ExteriorWall_" + element.ToString();
                this.DisplayName = displayName;
                this.Description = description;
                this.AnimFile = animFile;
            }
        }

        /// <summary>
        /// 要注册的干板墙元素皮肤列表。
        /// 
        /// POC 阶段使用现有 kanim（walls_basic_white_kanim）作为占位，
        /// 后续替换为 per-元素实时烘焙纹理。
        /// </summary>
        /// <summary>
        /// 要注册的干板墙元素皮肤列表。
        /// 
        /// 当前阶段（验证 SwapAnims）：使用游戏内已有的不同 walls kanim 作为占位，
        /// 验证不同皮肤确实能切换不同外观。
        /// 后续阶段：替换为实时烘焙的 per-元素纹理 kanim。
        /// </summary>
        public static readonly List<WallSkinEntry> WallSkins = new List<WallSkinEntry>
        {
            // 使用游戏内已有的不同 walls kanim，让每个皮肤有视觉区分
            new WallSkinEntry(SimHashes.IgneousRock,   "Igneous Rock",   "A wallpaper with the texture of igneous rock.",   "walls_basic_red_deep_kanim"),
            new WallSkinEntry(SimHashes.Granite,       "Granite",        "A wallpaper with the texture of granite.",        "walls_basic_grey_charcoal_kanim"),
            new WallSkinEntry(SimHashes.SandStone,     "Sandstone",      "A wallpaper with the texture of sandstone.",      "walls_basic_yellow_kanim"),
            new WallSkinEntry(SimHashes.Obsidian,      "Obsidian",       "A wallpaper with the texture of obsidian.",       "walls_basic_black_kanim"),
            new WallSkinEntry(SimHashes.Ice,           "Ice",            "A wallpaper with the texture of ice.",            "walls_basic_blue_cobalt_kanim"),
            new WallSkinEntry(SimHashes.Iron,          "Iron",           "A wallpaper with the texture of iron.",           "walls_basic_grey_kanim"),
            new WallSkinEntry(SimHashes.Copper,        "Copper",         "A wallpaper with the texture of copper.",         "walls_basic_orange_kanim"),
            new WallSkinEntry(SimHashes.Gold,          "Gold",           "A wallpaper with the texture of gold.",           "walls_basic_gold_kanim"),
            new WallSkinEntry(SimHashes.Steel,         "Steel",          "A wallpaper with the texture of steel.",          "walls_basic_white_kanim"),
            new WallSkinEntry(SimHashes.Katairite,     "Abyssalite",     "A wallpaper with the texture of abyssalite.",     "walls_basic_purple_kanim"),
            new WallSkinEntry(SimHashes.MaficRock,     "Mafic Rock",     "A wallpaper with the texture of mafic rock.",     "walls_basic_brown_kanim"),
            new WallSkinEntry(SimHashes.Niobium,       "Niobium",        "A wallpaper with the texture of niobium.",        "walls_basic_green_kelly_kanim"),
            new WallSkinEntry(SimHashes.Diamond,       "Diamond",        "A wallpaper with the texture of diamond.",        "walls_basic_teal_kanim"),
            new WallSkinEntry(SimHashes.Wolframite,    "Wolframite",     "A wallpaper with the texture of wolframite.",     "walls_basic_red_kanim"),
            new WallSkinEntry(SimHashes.Lead,          "Lead",           "A wallpaper with the texture of lead.",           "walls_basic_grey_charcoal_kanim"),
            new WallSkinEntry(SimHashes.Aluminum,      "Aluminum",       "A wallpaper with the texture of aluminum.",       "walls_basic_white_kanim"),
        };
    }
}
