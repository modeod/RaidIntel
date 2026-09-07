using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Prexis
{
    /// <summary>
    /// Настройки мода. Аналог твоего appsettings + IOptions, только сериализуется
    /// самой игрой в файл конфига через систему Scribe.
    /// </summary>
    public class PrexisSettings : ModSettings
    {
        public bool logRaids = true;
        public bool showOverlay = true;
        
        /// <summary>
        /// Scribe — это и запись, и чтение одновременно (одна функция на оба направления).
        /// Режим определяется глобальным состоянием Scribe.mode. Непривычно после
        /// бекенда, где сериализатор и десериализатор — разные вызовы.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref logRaids, "logRaids", true);
            Scribe_Values.Look(ref showOverlay, "showOverlay", true);
            base.ExposeData();
        }
    }

    /// <summary>
    /// Точка входа мода. Игра сама находит наследника Verse.Mod в нашей сборке
    /// и создаёт его — это шаг 3 из 14 в порядке старта, ДО загрузки любого XML.
    /// Значит: Harmony тут ставить можно, а лезть в DefDatabase — ещё нельзя.
    /// </summary>
    public class PrexisMod : Mod
    {
        public static PrexisSettings Settings;

        public PrexisMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<PrexisSettings>();

            // Строка-идентификатор должна быть уникальной: по ней потом видно,
            // чей патч висит на методе (в т.ч. в отчёте HugsLib по Ctrl+F12).
            var harmony = new Harmony("modeod.Prexis");

            // Находит в нашей сборке все классы с атрибутом [HarmonyPatch] и применяет их.
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("[Prexis] Mod ctor done, Harmony patches applied.");
        }

        public override string SettingsCategory() => "Prexis";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            // RimWorld рисует UI в immediate mode (IMGUI): нет дерева контролов,
            // нет состояния — код перерисовки выполняется каждый кадр заново.
            var list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("Log raid events to dev console", ref Settings.logRaids);
            list.CheckboxLabeled("Log raid show overlay", ref Settings.showOverlay);
            list.End();
            base.DoSettingsWindowContents(inRect);
        }
    }

    /// <summary>
    /// Первый Harmony-патч. Задача одна: доказать, что вся цепочка работает —
    /// сборка -> папка мода -> загрузка -> патч сработал.
    /// Postfix = наш код выполняется ПОСЛЕ оригинального метода, не заменяя его.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
    public static class Game_FinalizeInit_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Log.Message("[Prexis] Game.FinalizeInit finished - map is live.");
        }
    }
}
