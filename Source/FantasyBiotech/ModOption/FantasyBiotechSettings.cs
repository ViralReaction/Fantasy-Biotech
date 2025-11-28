using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System;

namespace FantasyBiotech
{
    public class FantasyBiotechSettings : ModSettings
    {
        public static FantasyBiotechSettings Instance { get; private set; }

        public static Dictionary<string, bool> settingMode = [];

        public bool replaceMechanoids = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref replaceMechanoids, "replaceMechanoids", true);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                settingMode ??= [];
            }

        }
        public IEnumerable<string> toggleSettings
        {
            get
            {
                return GetType().GetFields().Where(p => p.FieldType == typeof(bool) && (bool)p.GetValue(this)).Select(p => p.Name);
            }
        }
        private Vector2 scrollPosition;
        public void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect = new(inRect.x, inRect.y, inRect.width - 20f, 2000f);
            float contentHeight = 700f;
            Widgets.BeginScrollView(inRect, ref this.scrollPosition, new Rect(inRect.x, inRect.y, rect.width, contentHeight), true);
            Listing_Standard options = new();
            options.Begin(rect);
            options.GapLine();
            Text.Font = GameFont.Medium;
            options.Label("FantasyBiotech_Settings_Options".Translate());
            options.Gap();
            Text.Font = GameFont.Small;
            options.CheckboxLabeled("FantasyBiotech_Settings_Mech_Replace_Title".Translate(), ref replaceMechanoids, "FantasyBiotech_Settings_Mech_Replace_Desc".Translate());
            options.GapLine();
            options.Gap();

            // Reset to Default Button
            options.GapLine();
            options.Gap();
            if (options.ButtonText("FantasyBiotech_ResetDefault".Translate()))
            {
                ResetSettingsToDefault();
            }
            contentHeight = options.CurHeight +100f;
            options.End();
            Widgets.EndScrollView();
            rect.height = contentHeight;
        }
        private void ResetSettingsToDefault()
        {
            replaceMechanoids = true;

        }
    }
}
