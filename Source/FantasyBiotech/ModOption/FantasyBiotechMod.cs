using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace FantasyBiotech
{
    public class FantasyBiotech_Mod : Mod
    {
        public static FantasyBiotechSettings settings;
        private static ModMetaData _metaData { get; set; }
        public FantasyBiotech_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<FantasyBiotechSettings>();
            Harmony harmony = new Harmony(id: "fantasyBiotech");
            harmony.PatchAll();
            _metaData = ModLister.GetActiveModWithIdentifier("vr.fantasy.biotech", true);
            Log.Message("Fantasy Biotech v " + _metaData.ModVersion);
        }

        public override string SettingsCategory()
        {
            return "FantasyBiotech.ModNameShort".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }

    }
}
