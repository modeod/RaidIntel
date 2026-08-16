# Raid Intel

Отладочный оверлей для ИИ рейдеров RimWorld 1.6.
Этап 1 учебного трека проекта "Custom raider AI".

## Сборка

```
dotnet build Source/RaidIntel/RaidIntel.csproj
```
или открыть `Source/RaidIntel/RaidIntel.csproj` в Rider и нажать Build.

DLL появится в `1.6/Assemblies/RaidIntel.dll`.

## Подключение к игре

Символьная ссылка из папки модов RimWorld на корень этого репозитория:

```
mklink /D "C:\...\RimWorld\Mods\RaidIntel" "C:\...\RaidIntel"
```
(cmd от администратора)
