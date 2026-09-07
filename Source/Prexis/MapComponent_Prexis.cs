using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Prexis
{
    // MapComponent регистрировать нигде не надо: игра сама находит всех наследников
    // в загруженных сборках и создаёт по одному на карту. Живёт и сохраняется с картой.
    public class MapComponent_Prexis : MapComponent
    {
        public MapComponent_Prexis(Map map) : base(map) { }

        // Фаза КАДРА, не симуляции. Здесь можно только ЧИТАТЬ состояние.
        // Считать и кэшировать тут нельзя — момент вызова зависит от камеры.
        public override void MapComponentOnGUI()
        {
            if (!PrexisMod.Settings.showOverlay) return;
            if (Find.CameraDriver.CurrentZoom > CameraZoomRange.Middle) return;

            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.Dead || !pawn.HostileTo(Faction.OfPlayer)) continue;

                string duty = pawn.mindState?.duty?.def?.defName ?? "-";
                string giver = pawn.CurJob?.jobGiver?.GetType().Name ?? "-";
                GenMapUI.DrawThingLabel(pawn, duty + " | " + giver, Color.yellow);
            }
        }

        // Фаза СИМУЛЯЦИИ. Групповой мозг один на весь рейд, поэтому пишем его
        // не над каждой пешкой, а строкой в лог раз в 2 секунды.
        private readonly Dictionary<int, string> lordState = new Dictionary<int, string>();
        
        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 120 != 0) return;

            foreach (var lord in map.lordManager.lords)
            {
                if (lord.faction == null || !lord.faction.HostileTo(Faction.OfPlayer)) continue;

                string toil = lord.CurLordToil?.GetType().Name ?? "-";
                string state = toil + "/" + lord.ownedPawns.Count;
                lordState.TryGetValue(lord.loadID, out string prev);
                if (prev == state) continue;
                
                lordState[lord.loadID] = state;
                Log.Message($"[Prexis] LORD {lord.LordJob?.GetType().Name}" +
                            $" toil={lord.CurLordToil?.GetType().Name}" +
                            $" pawns={lord.ownedPawns.Count}");
                
            }
        }
    }
}