using HarmonyLib;
using RimWorld;
using Verse;

namespace Prexis;

public class RaidIntel
{
    // Метод protected, поэтому цель задаём строкой, а не nameof.
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class RaidEnemy_Patch
    {
        // __result — возвращаемое значение оригинала (специмя Harmony).
        // parms — тот же аргумент, что получил оригинальный метод.
        [HarmonyPostfix]
        public static void Postfix(bool __result, IncidentParms parms)
        {
            if (!__result) return;
            if (!PrexisMod.Settings.logRaids) return;

            Log.Message(
                $"[Prexis] RAID: faction={parms.faction?.Name} " +
                $"points={parms.points:F0} " +
                $"strategy={parms.raidStrategy?.defName} " +
                $"arrival={parms.raidArrivalMode?.defName}");
        }
    }
}