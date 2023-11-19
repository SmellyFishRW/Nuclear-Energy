using System.Collections.Generic;
using Verse;
using UnityEngine;
using System.Reflection;

namespace NuclearEnergy
{
    public class NuclearEnergySettings : ModSettings
    {
        public float smallNuclearReactorPowerConsumption = 6750f;
        public float normalNuclearReactorPowerConsumption = 25500f;
        public float giantNuclearReactorPowerConsumption = 50000f;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref smallNuclearReactorPowerConsumption, "smallNuclearReactorPowerConsumption", 6750f);
            Scribe_Values.Look(ref normalNuclearReactorPowerConsumption, "normalNuclearReactorPowerConsumption", 25500f);
            Scribe_Values.Look(ref giantNuclearReactorPowerConsumption, "giantNuclearReactorPowerConsumption", 50000f);
            base.ExposeData();
        }

        public void Reset()
        {
            smallNuclearReactorPowerConsumption = 6750f;
            normalNuclearReactorPowerConsumption = 25500f;
            giantNuclearReactorPowerConsumption = 50000f;
        }
    }

    public class NuclearEnergyMod : Mod
    {
        NuclearEnergySettings settings;
        public NuclearEnergyMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<NuclearEnergySettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Set how much power each reactor generates to your liking. The further to the right, the more power it can generate, up to 200000 W max.");
            listingStandard.Gap(48);
            listingStandard.Label("Small Nuclear Reactor Electricity Production: " + settings.smallNuclearReactorPowerConsumption.ToString("0") + " W");
            settings.smallNuclearReactorPowerConsumption = ((int)(listingStandard.Slider(settings.smallNuclearReactorPowerConsumption, 0f, 200000f) / 50)) * 50;
            listingStandard.Label("Medium Nuclear Reactor Electricity Production: " + settings.normalNuclearReactorPowerConsumption.ToString("0") + " W");
            settings.normalNuclearReactorPowerConsumption = ((int)(listingStandard.Slider(settings.normalNuclearReactorPowerConsumption, 0f, 200000f) / 50)) * 50;
            listingStandard.Label("Giant Nuclear Reactor Electicity Production: " + settings.giantNuclearReactorPowerConsumption.ToString("0") + " W");
            settings.giantNuclearReactorPowerConsumption = ((int)(listingStandard.Slider(settings.giantNuclearReactorPowerConsumption, 0f, 200000f) / 50)) * 50;
            if (listingStandard.ButtonText("Reset to Default"))
            {
                settings.Reset();
            }
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Nuclear Energy"/*.Translate()*/;
        }
    }

    public class NuclearCompPowerPlant : RimWorld.CompPowerPlant
    {
        protected override float DesiredPowerOutput
        {
            get
            {
                var settings = LoadedModManager.GetMod<NuclearEnergyMod>().GetSettings<NuclearEnergySettings>();
                switch (parent.def.defName)
                {
                    case "SmallNuclearReactor":
                        return settings.smallNuclearReactorPowerConsumption;
                    case "NuclearReactor":
                        return settings.normalNuclearReactorPowerConsumption;
                    case "GiantNuclearReactor":
                        return settings.giantNuclearReactorPowerConsumption;
                    default:
                        return base.DesiredPowerOutput;
                }
            }
        }
    }
}
