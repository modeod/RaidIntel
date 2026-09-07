# RimWorld 1.6 — карта архитектуры

Инженерная база: что вообще существует и куда можно вставиться. Вширь, не вглубь.

Источник: декомпиляция `v1.6.9438.38202` + `Data/Core/Defs` установленной игры.
✅проверено (путь+строка) · ⚠️вывод · ❌не найдено

## Содержание

1. [Иерархия мира](#1-иерархия-мира)
2. [Что живёт на карте — полный инвентарь](#2-что-живёт-на-карте--полный-инвентарь)
3. [Система компонентов](#3-система-компонентов)
4. [Def-система и патчи](#4-def-система-и-патчи)
5. [Тик и время](#5-тик-и-время)
6. [Сохранения](#6-сохранения)
7. [Жизненный цикл карты](#7-жизненный-цикл-карты)
8. [Цепочка рейда от сторителлера до тоила](#8-цепочка-рейда-от-сторителлера-до-тоила)
9. [Полные перечни](#9-полные-перечни)
10. [Расчёт силы рейда](#10-расчёт-силы-рейда)
11. [Боевые подсистемы](#11-боевые-подсистемы)
12. [Корнер-кейсы](#12-корнер-кейсы)
13. [Точки расширения — сводная](#13-точки-расширения--сводная)
14. [Поправки к ранее сказанному](#14-поправки-к-ранее-сказанному)

## 1. Иерархия мира

```
Current.Game
 ├─ World (ровно один)
 │   ├─ WorldComponent[]              ← авто по подклассам
 │   ├─ factionManager, ideoManager, worldPawns, worldObjects
 │   ├─ grid / pathGrid / reachability    (тайлы планеты)
 │   └─ pocketMaps: List<PocketMapParent>
 ├─ Maps: List<Map>                   ← 0..N, обычно 1–3
 │   └─ Map
 │       ├─ MapComponent[]            ← авто по подклассам
 │       ├─ ~90 менеджеров/гридов/listers
 │       └─ spawnedThings → Thing → ThingComp[]   (по XML)
 └─ GameComponent[] + ~30 глобальных менеджеров
```

⚠️ `Map` **не знает про `World` напрямую** — только через `map.Parent` (это `WorldObject`) и `map.Tile`.

### Game — верхний уровень

`storyteller` · `tickManager` · `letterStack` · `researchManager` · `questManager` · `signalManager` (шина сигналов) · `uniqueIDsManager` · `storyWatcher` · `gameEnder` · `history` · `taleManager` · `playLog`/`battleLog` · `autosaver` · базы политик (одежда, наркотики, еда, чтение) · `studyManager`, `analysisManager`, `entityCodex` (Anomaly) · `transportShipManager` · `Gravship` (1.6/Odyssey)

## 2. Что живёт на карте — полный инвентарь

**Это главный раздел карты.** Всё, что ниже — публичные поля `Map`. Если механики нет в этом списке, её на уровне карты не существует.

**Пешки и вещи:**
`spawnedThings` владелец всех Thing · `listerThings` индекс по def/группе, главный «SELECT» по карте · `listerBuildings` здания · `mapPawns` пешки по фракциям/категориям · `listerHaulables` что таскать · `listerMergeables` стаки под слияние · `listerBuildingsRepairable` · `listerFilthInHomeArea` · `listerArtificialBuildingsForMeditation` · `listerBuldingOfDefInProximity` · `listerBuildingWithTagInProximity` · `haulDestinationManager` склады и приоритеты · `storageGroups` · `itemAvailability` кэш «есть ли предмет X» · `resourceCounter` · `gatherSpotLister` · `deferredSpawner` отложенный спавн · `thingListChangedCallbacks` (1.6) · `animalPenManager` · `autoSlaughterManager` · `treeDestructionTracker`

**Пространство и сетки:**
`cellIndices` конверсия IntVec3↔int, индексация всех гридов · `thingGrid` · `coverGrid` лучшее укрытие в клетке · `edificeGrid` главное здание клетки · `blueprintGrid` · `terrainGrid` · `roofGrid` · `fogGrid` · `fertilityGrid` · `snowGrid`/`sandGrid` (1.6) · `deepResourceGrid` · `exitMapGrid` откуда можно уйти · **`avoidGrid`** клетки, избегаемые ИИ · `gasGrid` · `pollutionGrid` · `substructureGrid` (Odyssey) · `linkGrid` · `waterBodyTracker` (Odyssey) · `regionGrid`/`regionMaker`/`regionAndRoomUpdater`/`regionLinkDatabase`/`regionDirtyer` регионы и комнаты · `pathing` наборы PathGrid по режимам движения · **`pathFinder`**/`pawnPathPool` · `reachability` · `floodFiller` · `cellsInRandomOrder` · `roadInfo`/`waterInfo`

**Бой и угрозы:**
**`attackTargetsCache`** кэш целей по фракциям · `attackTargetReservationManager` резервация целей · **`lordManager`** ИИ групп · `lordsStarter` сам заводит лордов для вечеринок/ритуалов · `dangerWatcher` уровень опасности · `damageWatcher` накопленный урон · `strengthWatcher` боевая сила колонии · `fireWatcher` · `passingShipManager` · `mineStrikeManager`

**Строительство, зоны, работа:**
`designationManager` · `zoneManager` · `areaManager` · `planManager` · `reservationManager` · `enrouteManager` · `physicalInteractionReservationManager` · `pawnDestinationReservationManager` · `autoBuildRoofAreaSetter` · `roofCollapseBuffer`+`Resolver` · `powerNetManager`+`powerNetGrid` · `layoutStructureSketches` · `landingBlockers` (1.6)

**Погода и среда:**
`weatherManager` · `weatherDecider` · `windManager` · `mapTemperature` · `TemperatureVacuumCache` (в 1.6 объединена с вакуумом) · `gameConditionManager` · `skyManager` · `glowGrid` · `steadyEnvironmentEffects` · `tempTerrain` · `freezeManager` (1.6) · `wildAnimalSpawner`/`wildPlantSpawner` · `plantGrowthRateCalculator`

**Отрисовка:**
`mapDrawer` секции ландшафта · `dynamicDrawManager` движущиеся Thing · `overlayDrawer` иконки-оверлеи · `flecks` лёгкие частицы · `moteCounter` · `temporaryThingDrawer` · `effecterMaintainer` · `postTickVisuals` (1.6) · `tooltipGiverList` · `debugDrawer` · `rememberedCameraPos`

**Служебное:**
`compressor` сжатие однородных Thing в сейве · `generatorDef` · `uniqueID` · `generationTick` · `components` · `storyState` · `wealthWatcher` · **`events`** (`MapEvents`) · `retainedCaravanData` · `pocketTileInfo`

> **Находка, которая экономит десятки Harmony-патчей: `map.events` — новое в 1.6.**
> 25 обычных C#-событий вместо патчей: `BuildingSpawned/Despawned` · `ThingSpawned/Despawned` · `ThingFactionChanged` · `BuildingHitPointsChanged` · `TerrainChanged` · `PathCostRecalculate` · `LordAdded/Removed` · `FactionAdded/Removed` · `GameConditionAdded/Removed` · `ReservationAdded/Removed` · `HaulEnrouteAdded/Released` · `CellFogChanged` · `MapFogged` · `RegionsRoomsChanged` · `DoorOpened/Closed` · `RoofChanged` · `GlowChanged`

## 3. Система компонентов

| Тип | Регистрация | Тик | В сейве | Применение |
|---|---|---|---|---|
| `GameComponent` | авто по подклассам, ctor `(Game)` | каждый тик, после карт | да | глобальное состояние мода |
| `WorldComponent` | авто, ctor `(World)` | в `World.WorldTick()` | да | планетарное состояние |
| `MapComponent` | авто, ctor `(Map)` | предпоследним в `MapPostTick` | да | покарточное — самый частый хук |
| `ThingComp` | **через XML** в `ThingDef.comps` | `CompTick`/`CompTickInterval(delta)`/`Rare`/`Long` | да | 79 виртуальных хуков на объект |
| `HediffComp` | XML в `HediffDef.comps` | `CompPostTick(Interval)` | да | болезни, импланты, эффекты |
| `WorldObjectComp` | XML в `WorldObjectDef.comps` | `CompTick(Interval)` | да | поселения, сайты, таймауты |
| `AbilityComp` | XML в `AbilityDef.comps` | да | да | эффекты способностей |
| `StorytellerComp` | XML в `StorytellerDef.comps` | не тикает, опрашивается | нет | генерация инцидентов |
| `DefModExtension` | XML `<modExtensions>` на **любом** Def | — | нет | добавить поле к чужому Def |

⚠️ Ключевое различие: `*Component` (Game/World/Map) инстанцируются **рефлексией автоматически** — XML не нужен, но и выключить нельзя. `*Comp` — только по XML-объявлению.

❌ Отдельного `PawnComponent` нет: расширение пешки — через `ThingComp` на `Pawn`, `HediffComp`, `Gene`, либо Harmony на `Pawn_*Tracker` (они зашиты жёстко).

## 4. Def-система и патчи

**Наследование XML:** `Name` (якорь) · `ParentName` (от кого) · `Abstract="True"` (шаблон, Def'ом не станет) · `Inherit="false"` (не тянуть узел). Списки по умолчанию **заменяются целиком**. `GetBestParentFor` предпочитает родителя из того же мода — одинаковые `Name` в разных модах не ломают друг друга ✅

**Доступ:** `DefDatabase<T>` · `[DefOf]` (заполняется по имени поля) · `[MayRequire("packageId")]` и готовые `MayRequireRoyalty/Ideology/Biotech/Anomaly/Odyssey`.

### Все 16 `PatchOperation*` в 1.6

| Операция | Описание | Операция | Описание |
|---|---|---|---|
| `Add` | добавить дочерние узлы (`order` Append/Prepend) | `Insert` | вставить сиблингом до/после |
| `Remove` | удалить по xpath | `Replace` | заменить целиком |
| `SetName` | переименовать тег | `Test` | проверка xpath, ничего не меняет |
| `Conditional` | if/else: `match`/`nomatch` | `Sequence` | цепочка, рвётся при провале |
| **`FindMod`** | **главный инструмент совместимости** | `AddModExtension` | добавить modExtension к Def |
| `AttributeAdd` | атрибут, если нет | `AttributeSet` | добавить или перезаписать |
| `AttributeRemove` | удалить атрибут | `Pathed`/`Attribute` | абстрактные базы |

⚠️ Патчи применяются к сырому XML **после** сборки всех модов, но **до** `XmlInheritance.Resolve()` — значит патчить можно и абстрактные шаблоны.

## 5. Тик и время

```
TickManager.DoSingleTick():
 1. для каждой карты: MapPreTick()
 2. ticksGameInt++                       ← счётчик растёт МЕЖДУ Pre и Post
 3. tickListNormal → tickListRare(250) → tickListLong(2000)
 4. World.WorldTick → Storyteller → QuestManager → ...
 5. для каждой карты: MapPostTick()
 6. History → GameComponentTick → LetterStack
```

**`MapPreTick`** — подготовка мира *до* того, как пешки начнут думать: `itemAvailability` → `listerHaulables` → крыши → `windManager` → `mapTemperature` → **`pathFinder.PathFinderTick()`**.

**`MapPostTick`**: спавнеры → `powerNetManager` → среда → газ → загрязнение → **`lordManager`** → `lordsStarter` → условия → погода → … → **`MapComponentTick`** (предпоследним, видит уже обновлённую карту).

> **VTR — variable tick rate, новое в 1.6.** `Thing.DoTick()` делает две вещи: `Tick()` каждый тик безусловно, и `TickInterval(int delta)` — когда накопится `delta`. Ставка из `GenTicks.GetCameraUpdateRate`: **вне камеры или на неактивной карте → 15**, в кадре → `CameraDriver.CurrentZoom + 1`.
>
> Правило: логику накопления писать в `TickInterval(delta)` и **умножать на `delta`**. Иначе всё замедлится в 15 раз, когда камера смотрит в другую сторону. Хелпер: `GenTicks.IsTickIntervalDelta(period, delta)` вместо `IsHashIntervalTick`. Реализуют 60 классов + 20 через `CompTickInterval`.

**Кадровый цикл** (идёт и на паузе): `Map.MapUpdate()` → `skyManager` → электросети → **`regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms()`** → `glowGrid` → `lordManager.LordManagerUpdate()` → отрисовка → `MapComponentUpdate`.

⚠️ **Регионы, комнаты и электросети пересчитываются в Update, не в тике.** Логика, зависящая от комнат, работает с данными на момент последнего кадра, а не тика.

## 6. Сохранения

**Правило:** объект попадает в сейв, только если кто-то по цепочке от `Game` вызвал на нём `Scribe_Deep.Look` или он лежит в коллекции с `LookMode.Deep`.

Из ~90 полей `Map` сохраняются **39**. Всё остальное — `thingGrid`, `regionGrid`, `pathing`, `listerThings`, `coverGrid`, `glowGrid` — **производные кэши**, перестраиваемые в `FinalizeInit()`. Свой кэш тоже не сохранять, а перестраивать там же.

> **`ExposeData()` вызывается ТРИ раза за загрузку:** `LoadingVars` → `ResolvingCrossRefs` → `PostLoadInit`. Любой код внутри должен проверять `Scribe.mode`, иначе выполнится трижды.

**Грабли:** `Scribe_Deep` на объект, который уже сохраняется в другом месте → **две копии**, одна мёртвая. Пешка, вещь, Lord, фракция — всегда `Scribe_References`, и тип должен реализовать `ILoadReferenceable` со стабильным `GetUniqueLoadID()`. Коллекция с `LookMode.Reference` заполняется только на третьем проходе. Новое поле в старом сейве получает default — миграция не нужна.

## 7. Жизненный цикл карты

**Генерация:** `new Map` → `map.events` → **`ConstructComponents()`** (все менеджеры и гриды создаются здесь, включая MapComponent'ы) → `AddMap` → `GenStep`'ы по `order` → `FinalizeInit()` → `MapGenerated()`.

**`FinalizeInit()`** — вызывается и после генерации, и после загрузки сейва: пути → регионы и комнаты → электросети → температура → `avoidGrid.Regenerate()` → загоны → рост растений → `PostMapInit()` всех вещей → грязь → отрисовка → ресурсы → богатство → **`MapComponentUtility.FinalizeInit()` последним**.

✅ Отсюда: `MapComponent.FinalizeInit()` — правильное место для построения своих кэшей, там всё уже готово.

> **Утечки при выходе в главное меню.** `MemoryUtility.ClearAllMapsAndWorld` обнуляет рефлексией все поля `Map` и `World` — но **не ваши статики**. Любой статический кэш со ссылкой на `Map`/`Thing`/`Pawn` переживёт выход и утечёт. Чистить самому: `MapComponent.MapRemoved()`, `GameComponent.LoadedGame()/StartedNewGame()`, либо постфикс на `ClearAllMapsAndWorld`.

## 8. Цепочка рейда от сторителлера до тоила

```
Storyteller.StorytellerTick
 └ StorytellerComp.MakeIntervalIncidents → FiringIncident{IncidentDef, IncidentParms}
    └ IncidentWorker_Raid.TryExecuteWorker
       └ TryGenerateRaidInfo — СТРОГИЙ ПОРЯДОК:
          1 ResolveRaidPoints          → parms.points
          2 TryResolveRaidFaction      → parms.faction
          3 ResolveRaidStrategy        → parms.raidStrategy
          4 TryResolveRaidArriveMode   → parms.raidArrivalMode
          5 ResolveRaidAgeRestriction
          6 strategy.Worker.TryGenerateThreats
          7 arrivalMode.Worker.TryResolveRaidSpawnCenter
          8 AdjustedRaidPoints         → финальные points
          9 PawnGroupMakerUtility.GeneratePawns
         10 arrivalMode.Worker.Arrive
         11 PostProcessSpawnedPawns · GenerateRaidLoot
       └ strategy.Worker.MakeLords(parms, pawns)
          └ LordMaker.MakeNewLord(faction, LordJob, map, pawns)
             └ LordJob.CreateGraph() → StateGraph{LordToil, Transition{Trigger, TransitionAction}}
                └ LordToil.UpdateAllDuties() → pawn.mindState.duty = PawnDuty(DutyDef, focus)
                   └ ThinkNode_Duty → subNodes[duty.def.index]
                      └ DutyDef.thinkNode → JobGiver_* → Job
                         └ JobDef.driverClass → JobDriver.MakeNewToils() → Toil
```

> ⚠️ **Единственное место, где «рейд» превращается в «поведение» — `LordToil.UpdateAllDuties()`.** Всё, что мод хочет изменить в тактике, дешевле делать новым `LordToil` + новым `DutyDef`, чем патчем `ThinkTreeDef`.

## 9. Полные перечни

### RaidStrategyWorker — 12

| Стратегия | Описание | Стратегия | Описание |
|---|---|---|---|
| `ImmediateAttack` | обычный штурм | `ImmediateAttackSmart` | то же + обход по AvoidGrid |
| `ImmediateAttackSappers` | копают проход, нужны разрушители | `ImmediateAttackBreaching` | ломают стены пробойным |
| `ImmediateAttackBreachingSmart` | пролом + AvoidGrid | `ImmediateAttackFriendly` | союзная помощь |
| `Siege` | строят миномёты | `SiegeMechanoid` | механоидная осада, свои лорды |
| `StageThenAttack` | сбор в точке, затем штурм | `PsychicRitualSiege` | сектанты (Anomaly) |
| `ShamblerAssault` | шамблеры (Anomaly) | `WithRequiredPawnKinds` | абстрактная база |

### PawnsArrivalModeWorker — 13

`EdgeWalkIn` · **`EdgeWalkInGroups`** · `EdgeWalkInDistributed` · **`EdgeWalkInDistributedGroups`** · `EdgeWalkInDarkness` · `EdgeWalkInHateChanters` · `EdgeDrop` · **`EdgeDropGroups`** · `CenterDrop` · `ClusterDrop` · `RandomDrop` · `SpecificLocationDrop` · `EmergeFromWater`

⚠️ Выделенные — **единственные, кто создаёт несколько Lord на один рейд** (через `parms.pawnGroups`).

### LordJob — 71, по группам

**Враждебные рейды:**
`AssaultColony` · `AssaultThings` · `Siege` · `StageThenAttack` · `SleepThenAssaultColony` · `BossgroupAssaultColony` · `Kidnap` · `Steal` · `PrisonBreak` · `SlaveRebellion` · `HateChant` · `PsychicRitualRepeating` · `Metalhorror`

**Защита базы/точки:**
`DefendBase` · `DefendPoint` · `MechanoidDefendBase` · `MechanoidsDefend` · `SleepThenMechanoidsDefend` · `DefendAndExpandHive` · `ManTurrets` · `SitePawns` · `StructureThreatCluster` · `AssistColony`

**Сущности (Anomaly/Odyssey):**
`ShamblerAssault` · `ShamblerSwarm` · `SightstealerAssault/Swarm` · `GorehulkAssault` · `DevourerAssault` · `FleshbeastAssault` · `ChimeraAssault` · `EntitySwarm` · `HiveQueen` · `WanderNest` · …

Плюс группы «караваны/торговцы» (18), «ритуалы/вечеринки» (14), «мапген/спящие».

### Trigger — 47, TransitionAction — 12

Боевые триггеры: `FractionColonyDamageTaken` · `FractionPawnsLost` · `PawnHarmed` · `PawnKilled` · `PawnLost(Violently)` · `TicksPassed(WithoutHarm / AndNoRecentHarm)` · `KidnapVictimPresent` · `HighValueThingsAround` · `NoFightingSappers` · `DormancyWakeup(OrClamor)` · `BecameNonHostileToPlayer` · `Custom` · `TickCondition` · `Memo` · `Signal`

Действия: `CheckForJobOverride` (мост Lord→ThinkTree) · `EndAllJobs` · `EndAttackBuildingJobs` · `WakeAll` · `Message` · `Letter` · `Custom` · `EnsureHaveExitDestination` · `SetDefendLocalGroup/Trader` · `GiveGift`/`CheckGiveGift`

### Боевые DutyDef

`AssaultColony` · `Breaching` · `Sapper` · `Defend` · `DefendBase` · `DefendInvoker` · `HuntEnemiesIndividual` · `HuntDownColonists` · `AssaultThing` · `PrisonerAssaultColony` · `PrisonerEscapeSapper` · `ManClosestTurret` · `DefendHiveAggressively` · `Kidnap` · `Steal` · `ExitMapBestAndDefendSelf` + сущностные

### JobGiver_AI* — 31

**`AIFightEnemies`** главный боевой · `AIFightEnemy` · `AIBreaching` · `AISapper` · `AIDefendPoint/Self/Pawn/Master/Escortee/Overseer` · `AIFollowPawn/Master/Escortee/Overseer` · `AIGotoNearestHostile` · `AIGotoTarget` · `AIWaitAmbush` · `AITrashColonyClose` · `AITrashBuildingsDistant` · `AITrashDutyFocus` · `AICastAbility(OnSelf)` · `AIJump*` · `AISkipToJobTarget` · `AIReleaseMechs` · `AIResurrectTarget`

## 10. Расчёт силы рейда

```
points = clamp(
   ( PointsPerWealthCurve(богатство) + Σ по пешкам )
   × IncidentPointsRandomFactorRange
   × lerp(1, адаптация, difficulty.adaptationEffectFactor)
   × difficulty.threatScale
   × StorytellerDef.pointsFactorFromDaysPassed,
   минимум ~35, 10000 )

затем AdjustedRaidPoints:
   × arrivalMode.pointsFactorCurve × strategy.pointsFactorCurve
   × ageRestriction.threatPointsFactor × Tile.LayerDef.raidPointsFactor
   и не ниже strategy.MinimumPoints × 1.05
```

Богатство до **14 000 → 0 очков**; 400k → 2400; 1M → 4200. Здания считаются с коэффициентом 0.5. Колонист: 15 очков (до 10k богатства) → 200 (1M). Криптосон ×0.3, раб ×0.75, здоровье и возраст учитываются. Караван и гравикорабль ×0.7…0.9.

**Points → пешки:** жадный цикл `ChoosePawnGenOptionsByPoints` тянет `PawnGenOption` по весу, вычитая `kind.combatPower`, с потолком цены одной пешки от `FactionDef.maxPawnCostPerTotalPointsCurve`.

## 11. Боевые подсистемы

| Класс/система | Назначение |
|---|---|
| `Verb` / `VerbTracker` / `VerbProperties` | акт атаки: warmup → cast → cooldown; 37 подклассов |
| **`AttackTargetFinder.BestAttackTarget`** | скоринг целей: `TargetScanFlags`, дистанции, locus, bash |
| `AttackTargetsCache` | кэш `IAttackTarget` по фракциям, событийный |
| **`CastPositionFinder`** | куда встать: укрытие + дальность + LOS |
| `CoverUtility` / `CoverGrid` | `CalculateOverallBlockChance`, `TotalSurroundingCoverScore` |
| `ShootLeanUtility` / `GenSight` | стрельба из-за угла / линия видимости |
| `DamageWorker` / `ArmorUtility` | урон по частям тела / броня vs проникновение |
| `Stance_Warmup/Cooldown/Busy/Mobile` | состояние между Job и выстрелом |
| **`AvoidGrid`** + `AvoidGridTuning` | сетка «опасных» клеток для стоимости пути |
| `Building_Trap` + `ILordAvoidTraps` | ловушки и их обход лордом |

## 12. Корнер-кейсы

| Случай | Чем отличается от обычного рейда |
|---|---|
| **Мех-кластеры** | ❗`IncidentWorker_MechCluster` **не наследует** `IncidentWorker_Raid` — нет ни strategy, ни arrivalMode. Свой генератор и свои лорды |
| **Манхантеры** | ❗**Lord вообще не создаётся.** `Scaria` + `ManhunterPermanent` + `exitMapAfterTick`; поведение через `JobGiver_Manhunter` в дереве животного |
| Осада | `LordJob_Siege`, duty `Build`+`Defend`; выход по «нет артиллерии/строителей», потерям или таймауту |
| Пролом / сапёры | тот же `LordJob_AssaultColony` с флагом; свои LordToil раздают `Breaching`/`Sapper`+`Escort` |
| Инсектоиды | `IncidentWorker_Infestation`, лимит 30 хайвов; `LordJob_DefendAndExpandHive` |
| Квестовые рейды | `QuestNode_Raid` с заранее заданными parms — **обходит сторителлер целиком** |
| Подкрепления | `QuestPart_SurpriseReinforcement` — доп. группа в существующий или новый Lord |
| Спящие из мапгена | IncidentWorker нет вообще: `GenStep_*` / `ComplexThreatWorker_*` → `LordJob_SleepThenAssaultColony` |
| Рейд на караван | цель — `Caravan` (World), генерируется временная карта засады |
| Гравикорабль / космос | `Gravship : IIncidentTarget`; `PlanetLayerDef.raidPointsFactor`; белые/чёрные списки инцидентов по слою |

> ⚠️ **Манхантеры и мех-кластеры не проходят через `RaidStrategyDef` и `PawnsArrivalModeDef`.** Любой мод, который патчит «все рейды» через эти дефы, их пропустит молча.

## 13. Точки расширения — сводная

| Точка | Механизм | Риск |
|---|---|---|
| Новый `IncidentDef` / `RaidStrategyDef` / `PawnsArrivalModeDef` / `DutyDef` / `JobDef` | XML Def + класс | 🟢 очень безопасно |
| Новый `IncidentWorker` / `LordJob` / `LordToil` / `Trigger` / `TransitionAction` / `JobGiver` / `JobDriver` | наследование | 🟢 безопасно |
| `FactionDef.pawnGroupMakers` | PatchOperationAdd | 🟢 аддитивно |
| `map.events.*` | подписка на C#-событие | 🟢 безопасно, новое в 1.6 |
| `IPathFinderDataSource` / `IPathGridCustomizer` | публичная регистрация | 🟢 официальный канал |
| `TriggerFilter` на существующий триггер | `trigger.filters` публичен | 🟢 аддитивно |
| Правка чужого Def (baseChance, minThreatPoints, радиусы) | PatchOperation | 🟡 конфликт, если двое правят один узел |
| Свой `StorytellerComp` в ванильный сторителлер | PatchOperationAdd в comps | 🟡 порядок и дублирование |
| Harmony на `IncidentWorker.CanFireNow`, `TryGenerateRaidInfo` | postfix | 🟡 умеренно |
| Harmony на `LordJob.CreateGraph` чужого класса | postfix | 🟡 хрупко к обновлениям |
| **Вставка узла в ванильный `ThinkTreeDef`** | PatchOperationAdd/Insert | 🔴 главный источник конфликтов AI-модов |
| Harmony на `StorytellerUtility.DefaultThreatPointsNow` | postfix | 🔴 туда лезут все балансные моды |
| Harmony на `ChoosePawnGenOptionsByPoints` | любой | 🔴 CE и VE-ядра |
| Harmony на `AttackTargetFinder` / `CastPositionFinder` | любой | 🔴 конфликтно и дорого по CPU |
| Transpiler на `Verb` / `DamageWorker` | transpiler | 🔴 территория CE |

## 14. Поправки к ранее сказанному

> **`Faction.TacticalMemory` в 1.6 НЕ СУЩЕСТВУЕТ.** ❌ grep по всему репозиторию пуст. Я подал это как находку, вытащив из кода CE 2017 года — то есть из эпохи 1.0. Функционально ближайшее в 1.6: `AvoidGrid` (турели и ловушки), `LordJob_AssaultColony.useAvoidGridSmart`, `StoryState.lastRaidFaction`, `StoryWatcher_Adaptation.adaptDays`.
>
> **`TrapUtility` тоже не существует** — логика в `Building_Trap` + `ILordAvoidTraps.AvoidTrapRatio` + `AvoidGrid`.

_Переписано из HTML-артефакта вручную 07.09.2026._
