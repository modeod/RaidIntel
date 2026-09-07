---
paths:
  - "**/*.cs"
---

# Правила для C# в этом моде

Целевая платформа — **net472 на Mono**. Синтаксис C# 11/12 компилируется, но BCL старый:
`record`, `init`, `Span<T>`, `IsExternalInit` требуют полифилов. По умолчанию не использовать.

## Порядок инициализации — определяет, что где можно

```
шаг 3  из 14: конструкторы Mod-классов   ← Harmony можно, DefDatabase ещё НЕТ
шаг 6  из 14: PatchOperation'ы
шаг 14 из 14: [StaticConstructorOnStartup] ← Def'ы готовы
```

`MapComponent.FinalizeInit()` вызывается последним при генерации и при загрузке сейва —
правильное место для построения своих кэшей: регионы, комнаты, сети уже готовы.

## Сохранения

`ExposeData()` вызывается **три раза** за загрузку: `LoadingVars` → `ResolvingCrossRefs`
→ `PostLoadInit`. Любой код внутри проверяет `Scribe.mode`, иначе выполнится трижды.

`Scribe_Deep` на объект, который уже сохраняется в другом месте, даёт **две копии**.
Пешка, вещь, Lord, фракция — всегда `Scribe_References`.

Производные кэши не сохранять — перестраивать в `FinalizeInit()`.

## Harmony

Postfix по умолчанию. `Prefix` с `return false` выключает чужой метод для всех модов сразу —
только с обоснованием «почему нельзя иначе». Идентификатор патча уникальный,
по нему видно авторство в отчёте HugsLib (Ctrl+F12).

Перед патчем проверить, нет ли официальной точки расширения: `map.events` (25 событий в 1.6),
`IPathFinderDataSource`, `IPathGridCustomizer`, `TriggerFilter`, `ICompTactics` (при CE),
свой `ThinkNode` / `DutyDef` / `LordToil`.

## Производительность

Тяжёлое — через `Gilzoide.ManagedJobs.ManagedJobParallelFor` (встроен в `Assembly-CSharp`),
не через свои `Thread`. Троттлинг — `IsHashIntervalTick`, он же размазывает нагрузку
по разным тикам через hash от `thingIDNumber`.
