using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace NuclearEnergy
{
    // --- SETTINGS DATA CLASS ---
    public class NuclearEnergySettings : ModSettings
    {
        // Power Generation
        public float smallNuclearReactorGeneration = 6750f;
        public float mediumNuclearReactorGeneration = 25500f;
        public float giantNuclearReactorGeneration = 50000f;

        // Meltdown & Fallout (Future Features)
        public bool enableMeltdowns = true;
        public float falloutDurationDays = 15f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref smallNuclearReactorGeneration, "smallNuclearReactorGeneration", 6750f);
            Scribe_Values.Look(ref mediumNuclearReactorGeneration, "mediumNuclearReactorGeneration", 25500f);
            Scribe_Values.Look(ref giantNuclearReactorGeneration, "giantNuclearReactorGeneration", 50000f);

            Scribe_Values.Look(ref enableMeltdowns, "enableMeltdowns", true);
            Scribe_Values.Look(ref falloutDurationDays, "falloutDurationDays", 15f);

            base.ExposeData();
        }

        public void Reset()
        {
            smallNuclearReactorGeneration = 6750f;
            mediumNuclearReactorGeneration = 25500f;
            giantNuclearReactorGeneration = 50000f;
            enableMeltdowns = true;
            falloutDurationDays = 15f;
        }
    }

    // --- MAIN MOD CLASS ---
    public class NuclearEnergyMod : Mod
    {
        public static NuclearEnergySettings Settings { get; private set; }

        // --- DATA-DRIVEN UI SETUP ---
        private List<ReactorSetting> _reactorSettings;

        public NuclearEnergyMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<NuclearEnergySettings>();
            InitializeReactorSettings();
        }

        private void InitializeReactorSettings()
        {
            _reactorSettings = new List<ReactorSetting>
            {
                new ReactorSetting("Small Nuclear Reactor",  6750f,  () => Settings.smallNuclearReactorGeneration,  v => Settings.smallNuclearReactorGeneration = v),
                new ReactorSetting("Medium Nuclear Reactor", 25500f, () => Settings.mediumNuclearReactorGeneration, v => Settings.mediumNuclearReactorGeneration = v),
                new ReactorSetting("Giant Nuclear Reactor",  50000f, () => Settings.giantNuclearReactorGeneration,  v => Settings.giantNuclearReactorGeneration = v)
            };
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // --- POWER SETTINGS SECTION ---
            listingStandard.Label("Reactor Power Output");
            listingStandard.Gap(4f);
            listingStandard.Label("Set how much power each reactor generates. The slider will snap to the nearest 50 W, or you can type an exact value.", tooltip: "Fine-tune the balance of your reactors to fit your playthrough.");
            listingStandard.Gap(6f);

            // The UI now builds itself by looping through our list.
            foreach (var setting in _reactorSettings)
            {
                setting.DrawWidget(listingStandard);
            }

            listingStandard.GapLine(); // Separator

            // --- MELTDOWN SETTINGS SECTION ---
            listingStandard.Label("Meltdown & Fallout Settings (Future Feature)");
            listingStandard.Gap(4f);

            listingStandard.CheckboxLabeled("Enable reactor meltdowns and fallout", ref Settings.enableMeltdowns, "If enabled, reactor explosions will cause long-term nuclear fallout unless properly contained. (Feature in development)");

            if (Settings.enableMeltdowns)
            {
                listingStandard.Label($"Nuclear fallout duration: {Settings.falloutDurationDays:F1} days");
                Settings.falloutDurationDays = listingStandard.Slider(Settings.falloutDurationDays, 1f, 60f); // Min 1 day, max 60 days.
            }

            listingStandard.Gap(24f);

            if (listingStandard.ButtonText("Reset to Default"))
            {
                Settings.Reset();
                foreach (var setting in _reactorSettings)
                {
                    setting.ClearBuffer();
                }
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory() => "Nuclear Energy";
    }

    // --- HELPER CLASS FOR DATA-DRIVEN UI ---
    // This class holds all the information for one reactor setting.
    public class ReactorSetting
    {
        private readonly string _label;
        private readonly float _defaultValue;
        private readonly System.Func<float> _getter;
        private readonly System.Action<float> _setter;
        private string _buffer;

        public ReactorSetting(string label, float defaultValue, System.Func<float> getter, System.Action<float> setter)
        {
            _label = label;
            _defaultValue = defaultValue;
            _getter = getter;
            _setter = setter;
        }

        public void DrawWidget(Listing_Standard listing)
        {
            float currentValue = _getter();
            if (_buffer == null) _buffer = currentValue.ToString();

            Rect lineRect = listing.GetRect(30f);
            Rect labelRect = lineRect.LeftPart(0.4f);
            Rect sliderRect = lineRect.RightPart(0.6f);
            Rect fieldRect = sliderRect.RightPartPixels(80f);
            sliderRect.xMax -= fieldRect.width + 10f;

            Widgets.Label(labelRect, _label);
            TooltipHandler.TipRegion(labelRect, $"Current: {currentValue:N0} W\nDefault: {_defaultValue:N0} W\nMax: 200,000 W");

            float sliderValue = Widgets.HorizontalSlider(sliderRect, currentValue, 0f, 200000f);
            if (Mathf.Abs(sliderValue - currentValue) > float.Epsilon)
            {
                float snappedValue = Mathf.Round(sliderValue / 50f) * 50f;
                _setter(snappedValue);
                _buffer = snappedValue.ToString();
            }

            string newBuffer = Widgets.TextField(fieldRect, _buffer);
            if (newBuffer != _buffer)
            {
                if (float.TryParse(newBuffer, out float parsedValue))
                {
                    _setter(Mathf.Clamp(parsedValue, 0f, 200000f));
                }
                _buffer = newBuffer;
            }
            listing.Gap(6f);
        }

        public void ClearBuffer() => _buffer = null;
    }

    // --- REACTOR POWER COMPONENT ---
    public class NuclearCompPowerPlant : RimWorld.CompPowerPlant
    {
        protected override float DesiredPowerOutput
        {
            get
            {
                switch (parent.def.defName)
                {
                    case "SmallNuclearReactor": return NuclearEnergyMod.Settings.smallNuclearReactorGeneration;
                    case "NuclearReactor": return NuclearEnergyMod.Settings.mediumNuclearReactorGeneration;
                    case "GiantNuclearReactor": return NuclearEnergyMod.Settings.giantNuclearReactorGeneration;
                    default: return base.DesiredPowerOutput;
                }
            }
        }
    }


    // --- The Blueprint Class ---
    public class CompProperties_Containment : CompProperties
    {
        public CompProperties_Containment()
        {
            this.compClass = typeof(CompContainment);
        }
    }

    // --- The Live Component Class ---
    public class CompContainment : ThingComp
    {
        // This component is a placeholder for the future meltdown feature.
        // It doesn't need to do anything right now. Its only job is to exist,
    }
}