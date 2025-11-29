using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace FantasyBiotech
{
    public class MenuController : Mod
    {
        private Vector2 scrollPosition;
        private const float ResetButtonWidth = 160f;
        private const float ResetButtonHeight = 40f;
        private static TaggedString[] s_tabNames;
        private static List<TabRecord> s_tabs;
        public static int SelectedTab;

        public static MenuSettings settings;
        private static ModMetaData _metaData { get; set; }
        public MenuController(ModContentPack content) : base(content)
        {
            settings = GetSettings<MenuSettings>();
            Harmony harmony = new Harmony(id: "fantasyBiotech");
            LongEventHandler.QueueLongEvent(InitializeTabs, "FantasyBiotech_LongEvent_InitTabs", true, null);
            harmony.PatchAll();
            _metaData = ModLister.GetActiveModWithIdentifier("vr.fantasy.biotech", true);
            Log.Message("Fantasy Biotech v " + _metaData.ModVersion);
        }

        public override string SettingsCategory()
        {
            return "FantasyBiotech.ModNameShort".Translate();
        }

        private void InitializeTabs()
        {
            s_tabNames =
            [
                "FantasyBiotech_Settings_HeaderConstructs".Translate(),
                "FantasyBiotech_Settings_HeaderGenetics".Translate(),
                "FantasyBiotech_Settings_HeaderVampires".Translate(),
            ];
            s_tabs = [];
            for (int i = 0; i < s_tabNames.Length; i++)
            {
                int capturedIndex = i;
                s_tabs.Add(new TabRecord(s_tabNames[i], delegate
                {
                    SelectedTab = capturedIndex;
                }, () => SelectedTab == capturedIndex));
            }
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect windowRect = new Rect(inRect.x, inRect.y + 30f, inRect.width, inRect.height - 40f);
            Widgets.DrawMenuSection(windowRect);
            TabDrawer.DrawTabs(windowRect, s_tabs, 200f);
            Rect scrollRect = windowRect.ContractedBy(10f);
            float contentHeight = settings.GetOrCalculateHeightForTab(SelectedTab, scrollRect.width - 16f);
            Rect inner = new Rect(0f, 0f, scrollRect.width - 16f, contentHeight);
            Widgets.BeginScrollView(scrollRect, ref this.scrollPosition, inner, true);
            Listing_Standard list = new Listing_Standard();
            list.Begin(inner);
            settings.DoWindowContents(list);
            list.End();
            Widgets.EndScrollView();
            Rect resetButtonRect = new Rect(inRect.xMin + 200f, inRect.y + inRect.height + 5f, ResetButtonWidth, ResetButtonHeight);
            if (Widgets.ButtonText(resetButtonRect, "FantasyBiotech_ResetDefault".Translate()))
            {
                settings.ResetToDefaults();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

    }
}
