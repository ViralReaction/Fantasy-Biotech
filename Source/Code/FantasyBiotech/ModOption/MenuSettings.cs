using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using System;
using System.Reflection;

namespace FantasyBiotech
{
    public class MenuSettings : ModSettings
    {
        #region Settings

        public bool replaceMechanoids = true;

        private bool basicDeathrestCasket = true;
        private bool renameSanguophage = true;
        private bool medievalVampireScenario = true;
        private bool medievalSanguophageFaction = true;

        private bool medievalGoJuice = true;

        #endregion

        private readonly Dictionary<int, float> _cachedTabHeights = [];
        private readonly HashSet<int> _dirtyTabs = [];

        public IEnumerable<string> toggleSettings
        {
            get
            {
                return GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(f => f.FieldType == typeof(bool) && (bool)f.GetValue(this))
                        .Select(f => f.Name);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref replaceMechanoids, "replaceMechanoids", true);
            Scribe_Values.Look(ref basicDeathrestCasket, "basicDeathrestCasket", true);
            Scribe_Values.Look(ref renameSanguophage, "renameSanguophage", true);
            Scribe_Values.Look(ref medievalVampireScenario, "medievalVampireScenario", true);
            Scribe_Values.Look(ref medievalSanguophageFaction, "medievalSanguophageFaction", true);
            Scribe_Values.Look(ref medievalGoJuice, "medievalGoJuice", true);

        }

        public void DoWindowContents(Listing_Standard list)
        {
            switch (MenuController.SelectedTab)
            {
                case 0:
                    DoSettingsWindowContents_Constructs(list);
                    break;
                case 1:
                    DoSettingsWindowContents_Genetics(list);
                    break;
                case 2:
                    DoSettingsWindowContents_Vampires(list);
                    break;
            }
        }

        private void DoSettingsWindowContents_Constructs(Listing_Standard list)
        {
            list.Label("FantasyBiotech_Settings_HeaderConstructs".Translate());
            list.Gap();
            list.CheckboxLabeled("FantasyBiotech_Settings_Mech_Replace_Title".Translate(), ref replaceMechanoids, "FantasyBiotech_Settings_Mech_Replace_Desc".Translate());
        }
        private void DoSettingsWindowContents_Genetics(Listing_Standard list)
        {
            list.Label("FantasyBiotech_Settings_HeaderGenetics".Translate());
            list.Gap();
            list.CheckboxLabeled("FantasyBiotech_Settings_MedievalGoJuice_Replace_Title".Translate(), ref medievalGoJuice, "FantasyBiotech_Settings_MedievalGoJuice_Replace_Desc".Translate());
            //list.CheckboxLabeled("FantasyBiotech_Settings_Mech_Replace_Title".Translate(), ref replaceMechanoids, "FantasyBiotech_Settings_Mech_Replace_Desc".Translate());
        }
        private void DoSettingsWindowContents_Vampires(Listing_Standard list)
        {
            list.Label("FantasyBiotech_Settings_HeaderVampires".Translate());
            list.Gap();
            list.CheckboxLabeled("FantasyBiotech_Settings_BasicDeathrestCasket_Replace_Title".Translate(), ref basicDeathrestCasket, "FantasyBiotech_Settings_BasicDeathrestCasket_Replace_Desc".Translate());
            list.CheckboxLabeled("FantasyBiotech_Settings_RenameSanguophage_Replace_Title".Translate(), ref renameSanguophage, "FantasyBiotech_Settings_RenameSanguophage_Replace_Desc".Translate());
            list.CheckboxLabeled("FantasyBiotech_Settings_MedievalVampireScenario_Replace_Title".Translate(), ref medievalVampireScenario, "FantasyBiotech_Settings_MedievalVampireScenario_Replace_Desc".Translate());
            list.CheckboxLabeled("FantasyBiotech_Settings_MedievalSanguophageFaction_Replace_Title".Translate(), ref medievalSanguophageFaction, "FantasyBiotech_Settings_MedievalSanguophageFaction_Replace_Desc".Translate());


        }

        public float GetOrCalculateHeightForTab(int tabIndex, float width)
        {
            if (!_cachedTabHeights.TryGetValue(tabIndex, out float height) || _dirtyTabs.Contains(tabIndex))
            {
                Listing_Standard measuringList = new Listing_Standard();
                Rect dummyRect = new Rect(0f, 0f, width, 99999f);
                measuringList.Begin(dummyRect);
                switch (tabIndex)
                {
                    case 0:
                        DoSettingsWindowContents_Constructs(measuringList);
                        break;
                    case 1:
                        DoSettingsWindowContents_Genetics(measuringList);
                        break;
                    case 2:
                        DoSettingsWindowContents_Vampires(measuringList);
                        break;
                }
                measuringList.End();
                height = measuringList.CurHeight + 10f;
                if (tabIndex == 0)
                {
                    height = measuringList.curY + 10f;
                }
                _cachedTabHeights[tabIndex] = height;
                _dirtyTabs.Remove(tabIndex);
            }
            return height;
        }

        #region ResetToDefault
        public void ResetToDefaults()
        {
            switch (MenuController.SelectedTab)
            {
                case 0:
                    ResetToDefault_Constructs();
                    break;
                case 1:
                    ResetToDefault_Genetics();
                    break;
                case 2:
                    ResetToDefault_Vampires();
                    break;
            }
            _dirtyTabs.Add(MenuController.SelectedTab);
        }

        private void ResetToDefault_Constructs()
        {
            replaceMechanoids = true;
        }

        private void ResetToDefault_Genetics()
        {
            medievalVampireScenario = true;
        }

        private void ResetToDefault_Vampires()
        {
            basicDeathrestCasket = true;
            renameSanguophage = true;
            medievalVampireScenario = true;
            medievalSanguophageFaction = true;
        }

        #endregion
    }
}
