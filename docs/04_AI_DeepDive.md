# Система принятия решений ИИ в RimWorld 1.6

Job · JobDriver · Toil · ThinkTree · JobGiver · Duty · Lord · LordToil.
Собрано из твоих файлов `Data/Core/Defs/` и декомпилированной сборки 1.6 (Chillu1/RimWorldDecompiled, коммит «v1.6.9438»).

✅ проверено по исходнику · ⚠️ логический вывод · ❌ не проверено

---

## 1. Общая картина: два независимых мозга

Самое важное, что нужно уложить в голову перед всем остальным: у рейдера **два мозга, и они не знают друг о друге напрямую**.

| Групповой мозг — `Lord` | Личный мозг — `ThinkTree` |
|---|---|
| Один на весь рейд. Живёт в `map.lordManager`.<br>Решает **«какая сейчас фаза у группы»**: собираемся, штурмуем, ломаем стену, уходим.<br>Тикает раз в тик, но переключает фазу редко — по триггерам. | Свой у каждой пешки. Собирается из XML.<br>Решает **«что я делаю прямо сейчас»**: стрелять, бежать, ломать дверь, стоять.<br>Обходится каждый раз, когда у пешки закончилось текущее задание. |

Связывает их **ровно одна вещь** — `Duty`. Групповой мозг не командует пешкой напрямую; он выдаёт ей **роль**, а роль подключает к её личному дереву соответствующее поддерево.

```
Lord (групповой мозг) — «сейчас фаза: штурм»
  ↓ раздаёт роли методом LordToil.UpdateAllDuties()
Duty у пешки — «твоя роль: AssaultColony»
  ↓ роль подключает своё поддерево в личное дерево
ThinkTree пешки — обход сверху вниз до первого, кто ответил
  ↓ узел-лист вернул задание
Job — «стрелять в цель X с позиции Y»
  ↓ исполняется по шагам
JobDriver → Toil → Toil → … → задание закончилось
```

✅ Аналогия из твоего мира: `Lord` — сага-оркестратор, `LordToil` — состояние саги, `Duty` — какую стратегию-пайплайн подставить исполнителю, `ThinkTree` — chain of responsibility, `Job` — команда, `JobDriver` — конечный автомат её исполнения.

---

## 2. Тайминг: кто кого зовёт и как часто

Это первое, что нужно знать для отладки — потому что «почему пешка не реагирует» почти всегда про то, что дерево **вообще не обходилось**.

### Личный мозг

```
Pawn.Tick()                                    ✅ Verse/Pawn.cs:1560
└─ jobs.JobTrackerTickInterval(delta)          ✅ Pawn.cs:1620  (в 1.6 — с delta!)
   │
   ├─ каждые 30 тиков: DetermineNextConstantThinkTreeJob()
   │     └─ если дал Job → StartJob(JobCondition.InterruptForced)   ← ПЕРЕБИВАЕТ ВСЁ
   │
   ├─ проверка истечения текущего Job (expiryInterval)
   │     ├─ checkOverrideOnExpire == false → EndCurrentJob(Succeeded)
   │     └─ checkOverrideOnExpire == true  → CheckForJobOverride()
   │
   └─ если curJob == null → TryFindAndStartJob()
         └─ DetermineNextJob()
              ├─ 1) сначала constant tree
              └─ 2) потом pawn.thinker.MainThinkNodeRoot.TryIssueJobPackage(...)
```

**Три вывода, которые сэкономят тебе часы отладки:**

1. Главное дерево обходится **не каждый тик**, а только когда текущий `Job` закончился, провалился или истёк. Пешка, залипшая в длинном задании, твоё новое поведение просто не увидит.
2. **Constant tree** — единственное, что опрашивается принудительно (раз в 30 тиков) и перебивает текущую работу через `InterruptForced`. Там живут «граната под ногами» и «реакция на враждебность». Если тебе нужна реакция, которая обязана прервать что угодно — тебе туда.
3. В 1.6 у тика есть `delta`. За кадром пешки тикают реже — счётчик вызовов временем не является.

### Групповой мозг

```
Map.MapPostTick()                              ✅ Verse/Map.cs:1037
└─ lordManager.LordManagerTick()               ✅ LordManager.cs:48
   └─ для каждого lord: lord.LordTick()        ✅ Lord.cs:593
        ├─ curJob.LordJobTick()
        ├─ curLordToil.LordToilTick()
        ├─ CheckTransitionOnSignal(ForTick)    ← проверка триггеров перехода
        └─ ticksInToil++
```

**Ключевой факт про `UpdateAllDuties()`:** ✅ он вызывается **только в момент входа в новый `LordToil`** (последним шагом `Lord.GotoToil()`, `Lord.cs:585`), а не каждый тик.

Поэтому роли у рейдеров меняются «пачкой и разом»: сработал триггер → сага перешла в новое состояние → всем 20 пешкам переписали Duty в одном тике → у всех разом изменилось поведение. Именно этот момент ты и увидишь в своём оверлее.

Исключение: `LordToil_AssaultColonyBreaching` дёргает `UpdateAllDuties()` сам каждые 300 тиков ✅ — потому что цель пролома меняется по ходу.

---

## 3. Как дерево выбирает: два разных механизма приоритета

Это то место, где почти все путаются. В RimWorld **два** разных узла-контейнера, и они работают принципиально по-разному.

#### `ThinkNode_Priority` — статический порядок

```
foreach (child in subNodes)   // ПОРЯДОК ИЗ XML
    result = child.TryIssueJobPackage(...)
    if (result.IsValid) return result;
return NoJob;
```

Приоритет = **позиция в XML-файле**. Никаких чисел. Первый, кто вернул задание, выиграл. Так устроено **всё боевое поведение**.

#### `ThinkNode_PrioritySorter` — вычисляемый

```
каждый ребёнок называет число: GetPriority(pawn)
отбросить <= 0 и < minPriority
взять максимум → попробовать
не дал Job → выкинуть, взять следующий
```

Приоритет = **число, вычисляемое прямо сейчас**. Так устроены нужды и работа колониста (голод растёт → число растёт → еда обгоняет работу).

✅ `ThinkNode_PrioritySorter.cs`. При равных числах порядок рандомизируется псевдо-перемешиванием — тай-брейк недетерминирован по позиции в XML.

#### Шкала чисел для `PrioritySorter` ✅ `Verse.AI/ThinkNodePriority.cs`

| Приоритет | Значение |
|---|---|
| Food / Energy | 9.5 |
| DrugDesire | 9.25 |
| MiscNeed | 9.1 |
| AssignedWork | 9.0 |
| Rest | 8.0 |
| AssignedJoy | 7.0 |
| AnythingJoy | 6.0 |
| Reload | 5.9 |
| AnythingWork | 5.5 |
| AvoidIdle | 2.0 |

#### Как «нет ответа» превращается в «иди дальше»

```csharp
// ThinkNode_JobGiver — базовый класс всех листьев     ✅ Verse.AI/ThinkNode_JobGiver.cs
Job job = TryGiveJob(pawn);
if (job == null) return ThinkResult.NoJob;   // struct с Job == null, IsValid == false
return new ThinkResult(job, this);           // this = SourceNode — это и есть твой jobGiver в оверлее
```

⚠️ Никакого «возврата наверх с ошибкой» нет. `null` — это просто «я пас», и родительский цикл идёт к следующему ребёнку. Отсюда правило: **лист дерева обязан быть дешёвым в отказе**, потому что его спрашивают всегда, даже когда он не при делах.

---

## 4. Что такое Duty на самом деле

Duty — это **не задание и не приказ**. Это указатель на поддерево.

```csharp
// DutyDef — весь класс целиком                        ✅ Verse.AI/DutyDef.cs
ThinkNode thinkNode;            // ← ГЛАВНОЕ: поддерево, подключаемое пешке
ThinkNode constantThinkNode;    // то же, но в constant-дерево (перебивает всё)
bool alwaysShowWeapon;
ThinkTreeDutyHook hook = HighPriority;
bool threatDisabled;
...
```

Механика подключения — самая неочевидная часть всей системы:

```csharp
// ThinkNode_Duty — один узел, внутри которого лежат ВСЕ Duty игры  ✅ RimWorld/ThinkNode_Duty.cs
protected override void ResolveSubnodes() {
    foreach (DutyDef d in DefDatabase<DutyDef>.AllDefs)
        subNodes.Add(d.thinkNode.DeepCopy());       // копия поддерева каждого Duty
}

public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams p) {
    Lord lord = pawn.GetLord();                      // нет лорда → Log.Error + NoJob
    if (pawn.mindState.duty == null) → NoJob;
    var result = subNodes[pawn.mindState.duty.def.index]   // ← ВЫБОР ПО ИНДЕКСУ Duty
                     .TryIssueJobPackage(pawn, p);
    result = lord.Notify_DutyResult(result, pawn, p);      // ← ХУК ДЛЯ МОДА
    if (result.Job != null) {
        result.Job.lord = lord;
        result.Job.dutyTag = pawn.mindState.duty.tag;
    }
    return result;
}
```

**Читай так:** `ThinkNode_Duty` — это один узел в дереве пешки, который держит поддеревья _всех_ Duty игры сразу, а во время обхода спускается ровно в **одно** из них — то, чей `DutyDef` сейчас записан в `pawn.mindState.duty`.

То есть Duty работает как **switch по одному полю**. Меняешь поле — меняется целая ветка поведения. Ничего не пересобирается, дерево статично.

⚠️ И заодно: `lord.Notify_DutyResult(...)` — это **штатный ванильный хук**, через который твой `LordJob` может подменить или отменить любое задание, выданное через Duty, вообще не патча JobGiver'ы.

✅ Ставит `pawn.mindState.duty` только `LordToil.UpdateAllDuties()`. Обнуляет — `Lord.RemovePawn()` / `Cleanup()` (смерть, пленение, уход с карты).

### Где Duty подключается к дереву

В твоём файле `Data/Core/Defs/ThinkTreeDefs/SubTrees_Duty.xml` — дерево `LordDuty`:

```
ThinkNode_Tagger tagToGive=UnspecifiedLordDuty
└─ ThinkNode_ConditionalHasLordDuty
    ├─ ThinkNode_Duty                                          ← вот сюда спускается роль
    └─ ThinkNode_ConditionalHasVoluntarilyJoinableLord invert=true
        ├─ JobGiver_GetFood minCategory=UrgentlyHungry
        ├─ ThinkNode_ConditionalHasFallbackLocation
        │     └─ JobGiver_WanderNearFallbackLocation
        ├─ JobGiver_WanderAnywhere
        └─ JobGiver_IdleError
```

Отсюда же понятно, откуда берётся «рейдер тупо бродит»: если Duty не выдал ни одного задания, пешка проваливается в `WanderAnywhere`. Увидел в логе `IdleError` — значит не ответил вообще никто, и это баг.

---

## 5. Разбор: Duty `AssaultColony` — и почему рейдеры бьют мебель

Вот содержимое `AssaultColony` из твоего `Data/Core/Defs/DutyDefs/Duties_Misc.xml`. Это `ThinkNode_Priority`, то есть **порядок строк = приоритет**:

```
ThinkNode_Priority
 1. JobGiver_TakeCombatEnhancingDrug
 2. ThinkNode_Subtree → Abilities_Aggressive
 3. JobGiver_AIFightEnemies targetAcquireRadius=65, targetKeepRadius=72
 4. JobGiver_AITrashColonyClose
 5. JobGiver_AITrashBuildingsDistant                ← ЛОМАЕТ ДАЛЬНЮЮ ПОСТРОЙКУ
 6. JobGiver_AIGotoNearestHostile                    ← идти к ближайшему врагу
 7. JobGiver_AITrashBuildingsDistant attackAllInert=true
 8. JobGiver_AISapper canMineNonMineables=false
```

**Порядок строк — только половина ответа. Вторая половина в коде `JobGiver_AITrashBuildingsDistant`, и она интереснее.** ✅ `RimWorld/JobGiver_AITrashBuildingsDistant.cs`

```csharp
var all = pawn.Map.listerBuildings.allBuildingsColonist;
if (all.Count == 0) return null;                  // ← пустая карта → пункт 5 пасует
candidates = копия всего списка;
for (int i = 0; i < 75; i++) {                    // ← 75 СЛУЧАЙНЫХ ПОПЫТОК
    var b = candidates.RandomElement();            // ← БЕЗ учёта расстояния,
    if (TrashUtility.ShouldTrashBuilding(pawn, b, attackAllInert)) //   ценности, угрозы
        return TrashUtility.TrashJob(pawn, b);
}
return null;
```

**Настоящий дефект — не приоритет, а `RandomElement()`.** Кандидатами выступают **все** постройки колонии на карте, и выбор среди них — чистый рандом. Рейдер с севера карты может вытянуть стену на юге. Ни расстояния, ни ценности, ни «а не стреляет ли в меня кто-то прямо сейчас».

**Фильтр `ShouldTrashBuilding` — почему мебель редкая, но «прилипчивая»** ✅ `RimWorld/TrashUtility.cs`

```csharp
if ((b.def.building.isInert || b.def.IsFrame) && !attackAllInert || b.def.building.isTrap) {
    int hour3 = GenLocalDate.HourOfDay(pawn) / 3;
    int seed = (b.GetHashCode()*612361) ^ (pawn.GetHashCode()*391) ^ (hour3*73427324);
    if (!Rand.ChanceSeeded(0.008f, seed)) return false;      // 0.8%
}
if (!CanTrash(pawn, b) || !pawn.HostileTo(b)) return false;  // CanReach(Touch, Danger.Some)
```

**Инертные объекты** (мебель, скульптуры, столы) и недостроенные каркасы проходят фильтр с шансом **0.8%**. Всё остальное — стены, двери, турели, охладители, верстаки — проходит **без броска вообще**. Поэтому обычная цель рейдера это стена или дверь, а не столик.

⚠️ Но бросок **сеяный**, и сид собран из хэша постройки, хэша пешки и номера 3-часового блока. То есть внутри трёхчасового окна результат для пары (этот рейдер, этот столик) **стабилен**. Не «случайно разок клюнул» — а «зафиксировался и идёт». Вот откуда у игроков ощущение осмысленной ненависти к конкретной подкове.

**И третья часть, самая недооценённая: задание на снос почти не переспрашивается.**

```csharp
// TrashUtility.FinalizeTrashJob                     ✅ TrashUtility.cs
job.expiryInterval = Rand.Range(450, 500);   // ~7.5–8.3 сек
job.checkOverrideOnExpire = true;
job.expireRequiresEnemiesNearby = true;      // ← ВОТ ОНО

// Pawn_JobTracker.cs:172
if (!curJob.expireRequiresEnemiesNearby || PawnUtility.EnemiesAreNearby(pawn, 25))
    → CheckForJobOverride();
else
    → DebugLogEvent("Job expire skipped because there are no enemies nearby");
```

Задание истекает раз в ~8 секунд, но **переоценка происходит только если рядом есть враги**. А «рядом» здесь — это `regionsToScan: 25` при `passDoors: false` ✅ (сигнатура `PawnUtility.EnemiesAreNearby`): заливка по регионам, **не проходящая через двери**.

⚠️ Следствие: колонисты, сидящие в закрытой базе за дверьми, для этой проверки **не существуют**. Рейдер, начавший ломать дальний сарай, будет ломать его до конца — переоценка не сработает, потому что «врагов рядом нет».

**Что из этого следует для мода — пересмотренная версия:**

1. Простая перестановка строк 5 и 6 — **не решение**. Она сделает `GotoNearestHostile` почти всегда выигрывающим, и рейдеры превратятся в предсказуемую толпу, которая всегда бежит по прямой. Осада и пролом потеряют смысл.
2. Настоящие рычаги — **два**: заменить `RandomElement()` на взвешенный выбор (расстояние + ценность + «мешает ли эта постройка пройти»), и починить условие переоценки, из-за которого рейдер не замечает изменившуюся обстановку.
3. Первое делается подменой класса узла (`PatchOperationAttributeSet` по `@Class`) — свой JobGiver вместо ванильного, без единого Harmony-патча. Второе — правкой полей выданного Job, для чего есть штатный хук `LordJob.Notify_DutyResult`.

Полезно для отладки: включи в dev-режиме подробный лог заданий — строка «Job expire skipped because there are no enemies nearby» печатается ванилью и прямо показывает этот эффект.

### Для сравнения: `Breaching` — как выглядит грамотно написанное Duty

```
ThinkNode_Priority
 1. JobGiver_TakeCombatEnhancingDrug
 2. ThinkNode_HarmedRecently thresholdTicks=600
      └─ JobGiver_AIFightEnemies radius 65/72   ← если ранили за последние 10 сек — драться широко
 3. JobGiver_AIFightEnemies radius 12/15         ← иначе только вплотную
 4. JobGiver_AIBreaching                          ← ломать стену (основная задача)
 5. JobGiver_AIFightEnemies radius 65/72
 6. JobGiver_WanderNearBreacher wanderRadius=5
 7–9. TrashColonyClose → TrashBuildingsDistant → GotoNearestHostile
```

Обрати внимание на приём: **один и тот же JobGiver трижды с разными радиусами**, разделённый условным узлом. Это ванильный способ сделать «контекстное» поведение без единой строки C#: пока не ранили — не отвлекаться от стены (радиус 12), ранили — огрызаться широко (радиус 65).

#### Полное дерево `Humanlike` (главное дерево любого человека)

Из твоего `Data/Core/Defs/ThinkTreeDefs/Humanlike.xml`. Показан скелет; зелёным (в оригинальной HTML-раскраске) — точки, куда моды вставляют свои поддеревья (`JobGiver_*`).

```
ThinkNode_Priority
 ├─ ConditionalLyingDown → ChancePerHour_Lovin → JobGiver_DoLovin
 ├─ ConditionalMustKeepLyingDown → QueuedJob / PrioritySorter(joy, meditate) / KeepLyingDown
 ├─ Subtree Downed
 ├─ Subtree BurningResponse
 ├─ Subtree MentalStateCritical
 ├─ Subtree Abilities_Escape
 ├─ JobGiver_ReactToCloseMeleeThreat
 ├─ Subtree MentalStateNonCritical
 ├─ Subtree RopedPawn
 ├─ SubtreesByTag «Humanlike_PostMentalState»
 ├─ QueuedJob
 ├─ ConditionalColonist → Tagger(DraftedOrder) → MoveToStandable, JobGiver_Orders
 ├─ ConditionalNPCCanSelfTendNow → JobGiver_SelfTend
 ├─ JoinVoluntarilyJoinableLord (HighPriority) → Subtree LordDuty     ← ЗДЕСЬ РЕЙДЕР
 ├─ SubtreesByTag «Humanlike_PostDuty»
 ├─ ConditionalPrisoner → …escape / patient / needs / wander…
 ├─ ConditionalColonist → SeekAllowedArea, SeekSafeTemperature, JobGiver_Work(emergency), …
 ├─ ThinkNode_TraitBehaviors
 ├─ SubtreesByTag «Humanlike_PreMain»
 ├─ ConditionalColonist → Subtree MainColonistBehaviorCore
 ├─ ConditionalPawnKind(WildMan) → Subtree MainWildManBehaviorCore
 ├─ SubtreesByTag «Humanlike_PostMain»
 ├─ ConditionalColonist → Idle: IdleJoy / WanderColony
 ├─ ConditionalGuest → PatientGoToBed / ExitMapBest
 ├─ ConditionalColonist(invert) → JobGiver_ExitMapBest
 ├─ Tagger(Idle) → JobGiver_WanderAnywhere
 └─ JobGiver_IdleError
```

**Ключевое наблюдение:** ветка рейдера (`JoinVoluntarilyJoinableLord → LordDuty`) стоит **выше** всей колонистской части дерева. Пока у пешки есть Lord и Duty, до колонистского поведения дело не доходит вообще.

#### Constant tree — `HumanlikeConstant` (то, что перебивает всё, раз в 30 тиков)

```
ThinkNode_Priority
 ├─ Subtree Despawned
 ├─ ConditionalCanDoConstantThinkTreeJobNow
 │    ├─ JobGiver_FleePotentialExplosion         ← бежать от гранаты
 │    ├─ JobGiver_FindOxygen
 │    ├─ JobGiver_BoardOrLeaveGravship            ← новое в 1.6 (Odyssey)
 │    ├─ Subtree JoinAutoJoinableCaravan
 │    ├─ JobGiver_ConfigurableHostilityResponse
 │    └─ ConditionalIsCrawling → ConditionalCanCrawl(invert) → JobGiver_IdleForever
 └─ ConditionalCanDoLordJobNow → Subtree LordDutyConstant → ThinkNode_DutyConstant
```

Именно сюда `AssaultColony` кладёт свой `constantThinkNode` = «подобрать валяющееся оружие» — поэтому рейдер подбирает ствол, не прерывая штурма.

---

## 6. Lord: граф состояний рейда

`LordJob_AssaultColony` — это описание саги. Его `CreateGraph()` строит `StateGraph` из `LordToil` (состояний) и `Transition` (переходов с триггерами).

```csharp
LordJob_AssaultColony(Faction faction,
    bool canKidnap = true, bool canTimeoutOrFlee = true,
    bool sappers = false, bool useAvoidGridSmart = false,
    bool canSteal = true, bool breachers = false,
    bool canPickUpOpportunisticWeapons = false)          ✅ RimWorld/LordJob_AssaultColony.cs
```

```
LordToil_AssaultColony — стартовое состояние. Duty всем: AssaultColony
  ↓ Trigger_TicksPassed(26000–38000) + фильтр «карта проходима»
LordToil_ExitMap — Duty всем: ExitMapBest. Сообщение «рейдеры сдались и уходят»
```

```
LordToil_AssaultColony
  ↓ Trigger_FractionColonyDamageTaken(0.25–0.35)
LordToil_ExitMap — «рейдеры удовлетворены и уходят»
```

```
LordToil_AssaultColony
  ↓ Trigger_KidnapVictimPresent → подграф LordJob_Kidnap
LordToil_KidnapCover — Duty: Kidnap (схватить и на выход)
```

| Тайминг | Значение | В игровом времени |
|---|---|---|
| `AssaultTimeBeforeGiveUp` | 26000–38000 тиков | ≈ 10–15 игровых часов |
| `SapTimeBeforeGiveUp` | 33000–38000 тиков | ≈ 13–15 часов |
| `BreachTimeBeforeGiveUp` | 33000–38000 тиков | ≈ 13–15 часов |

✅ числа из исходника; ⚠️ перевод в часы мой (2500 тиков = 1 игровой час).

### Кто какой Duty раздаёт

| LordToil | Duty | Детали |
|---|---|---|
| `LordToil_AssaultColony` | `AssaultColony` | + `attackDownedIfStarving`, будит спящих мехов |
| `LordToil_Stage` | `Defend` | `radius = 28` вокруг точки сбора |
| `LordToil_AssaultColonySappers` | `Sapper` / `Escort` / `AssaultColony` | эскорт: радиус 15–19 (стрелки), 23–26 (ближники) |
| `LordToil_AssaultColonyBreaching` | `Breaching` / `Escort` / `AssaultColony` | переназначает роли каждые 300 тиков |
| `LordToil_ExitMap` | `ExitMapBest` | может форсированно оборвать текущий Job |
| `LordToil_Siege` | `Build` + `Defend` | ❌ Duty с именем «Siege» не существует |

**Мост Lord → ThinkTree в обратную сторону.** Когда сага переходит в новое состояние, одного `UpdateAllDuties()` мало: пешки могут сидеть в длинных заданиях и не переспрашивать дерево. Поэтому у переходов есть `TransitionAction_CheckForJobOverride` ✅, который проходит по всем `ownedPawns` и дёргает `jobs.CheckForJobOverride()` — принудительный переобход дерева.

Если твоё будущее поведение «не включается сразу после смены фазы» — причина почти наверняка здесь.

---

## 7. Трассировки: четыре сценария по шагам

#### Сценарий A — рейдер заспавнился, колонисты в 80 клетках за стеной

```
тик 0   Lord создан, LordToil_AssaultColony.Init() → UpdateAllDuties()
        → pawn.mindState.duty = PawnDuty(AssaultColony)

тик 1   curJob == null → TryFindAndStartJob() → DetermineNextJob()
        → constant tree: ничего
        → Humanlike:
            ... Downed? нет. Burning? нет. MentalState? нет ...
            JoinVoluntarilyJoinableLord → LordDuty → ThinkNode_Duty
              → subNodes[AssaultColony.index] (ThinkNode_Priority):
                  1. TakeCombatEnhancingDrug  → null (нет наркотиков)
                  2. Abilities_Aggressive     → null
                  3. AIFightEnemies(r=65)     → null  ← колонист в 80 клетках
                  4. AITrashColonyClose       → null  ← рядом ничего нет
                  5. AITrashBuildingsDistant  → JOB: Attack(пикниковый столик)   ✔ ПОБЕДИЛ

тик 2+  JobDriver_AttackStatic исполняет Toil'ы: дойти → бить → бить → …
        Дерево НЕ опрашивается, пока задание не кончится.
```

Оверлей покажет: `AssaultColony | JobGiver_AITrashBuildingsDistant`. Это ровно та строка, ради которой мы оверлей и делали.

#### Сценарий B — колонист вышел на 50 клеток

```
Текущий Job (ломать столик) ещё идёт → дерево молчит.
Варианты, как поведение переключится:

 (а) столик сломан → EndCurrentJob(Succeeded) → TryFindAndStartJob()
 (б) у Job истёк expiryInterval и checkOverrideOnExpire=true → CheckForJobOverride()
 (в) рейдера ранили → constant tree / реакция на угрозу

затем:  3. AIFightEnemies(r=65) → цель в 50 клетках найдена
        → JOB: AttackStatic(колонист), позиция взята из CastPositionFinder
        → пункты 4–8 даже не спрашиваются
```

⚠️ Вот источник ощущения «рейдеры тупят»: между появлением цели и реакцией лежит время дожития текущего задания. Не логика плохая — **частота переспрашивания** низкая.

#### Сценарий C — рейд потерял треть состава

```
Lord.LordTick() → CheckTransitionOnSignal(ForTick)
  → Transition.triggers: Trigger_FractionColonyDamageTaken(0.30) → ActivateOn == true
  → Transition.Execute(lord):
        preActions:  TransitionAction_Message("рейдеры удовлетворены и уходят")
        lord.GotoToil(LordToil_ExitMap):
            curLordToil.Cleanup()
            curLordToil = LordToil_ExitMap
            ticksInToil = 0
            Init()
            UpdateAllDuties()   ← ВСЕМ 20 ПЕШКАМ РАЗОМ: duty = ExitMapBest
        postActions: TransitionAction_CheckForJobOverride
                     → каждой пешке jobs.CheckForJobOverride()
                     → принудительный переобход дерева ПРЯМО СЕЙЧАС

следующий обход:  ThinkNode_Duty → subNodes[ExitMapBest.index]
                  → JobGiver_ExitMapBest → JOB: Goto(край карты)
```

В твоём оверлее это выглядит как мгновенная смена жёлтых подписей у всей толпы — самый наглядный момент во всей системе.

#### Сценарий D — для контраста: колонист выбирает работу (PrioritySorter)

```
MainColonistBehaviorCore → ThinkNode_PrioritySorter
  каждый ребёнок называет число ПРЯМО СЕЙЧАС:
      JobGiver_GetFood      → 0    (сыт)
      JobGiver_GetRest      → 0    (выспался)
      JobGiver_Work         → 5.5  (AnythingWork)
      JobGiver_GetJoy       → 6.0  (AnythingJoy — но радость выше 90%?)
  → максимум 6.0 → GetJoy → но он вернул null (нет доступных развлечений)
  → выкинуть его, взять следующий максимум: 5.5 → Work → JOB: рубить дерево

через 3 часа голод падает:
      JobGiver_GetFood      → 9.5  (Food)
  → 9.5 > 5.5 → еда обгоняет работу БЕЗ каких-либо изменений в дереве
```

Вот зачем нужны два разных механизма: у нужд приоритет плавающий и должен считаться, у боя — жёсткий и должен быть предсказуемым.

---

## 8. Job → JobDriver → Toil: как задание исполняется

```csharp
// Job — команда с параметрами                          ✅ Verse.AI/Job.cs
JobDef def;  LocalTargetInfo targetA/targetB/targetC;  int count;
int startTick;  int expiryInterval;  bool checkOverrideOnExpire;
bool playerForced;  LocomotionUrgency locomotionUrgency;
Verb verbToUse;  Lord lord;  string dutyTag;
ThinkNode jobGiver;              // ← КТО ВЫДАЛ (это и есть поле для твоего оверлея)
ThinkTreeDef jobGiverThinkTree;  // ← ИЗ КАКОГО ДЕРЕВА
```

```csharp
// JobDriver — конечный автомат исполнения
protected abstract IEnumerable<Toil> MakeNewToils();

// Toil — один шаг
Action initAction;             // выполнить один раз при входе в шаг
Action tickAction;             // выполнять каждый тик, пока шаг активен
ToilCompleteMode defaultCompleteMode;   // Instant | Delay | PatherArrival | FinishedBusy | Never
int defaultDuration;
List<Func<JobCondition>> endConditions;
```

Порядок одного тика ✅ `JobDriver.DriverTick()`:

```csharp
ticksLeftThisToil--
CheckCurrentToilEndOrFail()        // globalFailConditions, потом endConditions текущего Toil
                                   // любое != Ongoing → EndJobWith(condition)
switch (CurToil.defaultCompleteMode) {
    Instant       → сразу следующий Toil
    Delay         → ticksLeftThisToil <= 0 → следующий
    PatherArrival → пешка дошла → следующий
    FinishedBusy  → закончилась стойка/анимация → следующий
    Never         → никогда сам не закончится (только по endCondition)
}
tickAction()

// Toil'ы кончились → EndJobWith(JobCondition.Succeeded)
```

| `JobCondition` | Что значит |
|---|---|
| `Ongoing` | продолжаем |
| `Succeeded` | задание выполнено штатно |
| `Incompletable` | стало невозможно (сюда попадают все `AddFailCondition`) |
| `InterruptOptional` | перебито «мягко» (через `CheckForJobOverride`) |
| `InterruptForced` | перебито жёстко (constant tree, приказ игрока) |
| `Errored` / `ErroredPather` | исключение → пешке принудительно выдаётся `Wait` на 250 тиков |

---

## 9. Куда вклиниваться: шесть точек, по дешевизне

| # | Точка | Чем хороша | Риск конфликта |
|---|---|---|---|
| 1 | **Свой `ThinkTreeDef` с `insertTag`** | Чистый XML. Подхватывается `ThinkNode_SubtreesByTag`, порядок — через `insertPriority`. Чужие деревья не трогаются вообще. | минимальный |
| 2 | **`PatchOperation` в `DutyDef`** | Правка порядка / параметров / подмена класса узла. Именно так чинится баг из §5. | низкий |
| 3 | **`LordJob.Notify_DutyResult`** | Ванильный хук: перехват любого задания, выданного через Duty. Без Harmony. | низкий |
| 4 | **Свой `DutyDef` + свой `LordToil`** | Полностью свои роли и фазы, ничего чужого не переопределяется. | низкий |
| 5 | **`StateGraph.AddTransition`** | Добавить переход в уже построенный граф. Ваниль сама так делает. | средний |
| 6 | **Harmony на `JobGiver.TryGiveJob`** | — | высокий, не делать |

#### Реальные примеры XML-операций (из CE и VEF)

```xml
<!-- 1. Свой поддерево через тег, без правки чужого дерева (VEF) -->
<ThinkTreeDef>
  <defName>MyMod_RaiderTactics</defName>
  <insertTag>Humanlike_PreMain</insertTag>
  <insertPriority>100</insertPriority>
  <thinkRoot Class="ThinkNode_Priority"> ... </thinkRoot>
</ThinkTreeDef>

<!-- 2. Вставить узел ПЕРЕД существующим внутри Duty (CE) -->
<Operation Class="PatchOperationInsert">
  <xpath>Defs/DutyDef[defName="AssaultColony"]/thinkNode/subNodes
         /li[@Class="JobGiver_AITrashBuildingsDistant"]</xpath>
  <value><li Class="JobGiver_AIGotoNearestHostile" /></value>
</Operation>

<!-- 3. Подменить реализацию узла во ВСЕХ Duty сразу, сохранив его XML-поля (CE) -->
<Operation Class="PatchOperationAttributeSet">
  <xpath>Defs/DutyDef/thinkNode/subNodes/li[@Class="JobGiver_AIFightEnemies"]</xpath>
  <attribute>Class</attribute>
  <value>MyMod.JobGiver_AIFightEnemiesSmart</value>
</Operation>

<!-- 4. Поменять параметр существующего узла (CE) -->
<Operation Class="PatchOperationReplace">
  <xpath>Defs/DutyDef[defName="Defend"]/thinkNode/subNodes
         /li[@Class="JobGiver_AIDefendPoint"]/targetAcquireRadius</xpath>
  <value><targetAcquireRadius>86</targetAcquireRadius></value>
</Operation>
```

✅ Подтверждённые ванильные теги вставки: `Humanlike_PostMentalState`, `Humanlike_PostDuty`, `Humanlike_PreMain`, `Humanlike_PostMain`, `Animal_PreMain`, `Animal_PreWander`.

---

## 10. Шпаргалка: где что лежит

| Что ищешь | Где |
|---|---|
| Структура главного дерева человека | `Data/Core/Defs/ThinkTreeDefs/Humanlike.xml` |
| Все поддеревья (Downed, MentalState, MainColonistBehaviorCore…) | `ThinkTreeDefs/SubTrees_Misc.xml` |
| Точка подключения Duty | `ThinkTreeDefs/SubTrees_Duty.xml` |
| Все боевые роли (AssaultColony, Sapper, Breaching, Escort…) | `DutyDefs/Duties_Misc.xml` — 36 штук |
| Код JobGiver'ов, LordToil, Lord | декомпилятор Rider: `Verse.AI`, `Verse.AI.Group`, `RimWorld` |
| Кто выдал текущее задание пешке | `pawn.CurJob.jobGiver` (тип `ThinkNode`) |
| Текущая роль пешки | `pawn.mindState.duty.def.defName` |
| Групповой мозг пешки | `pawn.GetLord()` — это просто поле `p.lord`, не поиск |
| Текущая фаза саги | `lord.CurLordToil.GetType().Name` |
| Граф переходов | `lord.Graph.transitions` — публичный список |

---

Источники: `Data/Core/Defs/` установленной игры (1.6.4871 rev591) · [Chillu1/RimWorldDecompiled](https://github.com/Chillu1/RimWorldDecompiled) (коммит «v1.6.9438») · [Combat Extended](https://github.com/CombatExtended-Continued/CombatExtended) · [VEF](https://github.com/Vanilla-Expanded/VanillaExpandedFramework) · [Smarter Raider AI](https://github.com/pogoman/Smarter-Raider-AI) · [CAI-5000](https://github.com/kbatbouta/CAI-5000)

_Переписано из HTML-артефакта вручную 07.09.2026._
