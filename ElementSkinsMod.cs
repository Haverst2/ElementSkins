using HarmonyLib;
using KMod;
using UnityEngine;

namespace ElementSkins
{
    public sealed class ElementSkinsMod : UserMod2
    {
        public const string ModId = "ElementSkins";

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            Debug.Log($"[{ModId}] Loading element skin facades...");
            harmony.PatchAll();
        }
    }
}
