# Трек Б — голос игроков и карта рынка

Собрано 02.09.2026. Источник: Steam Community (Reddit и ludeon.com отдают 403).
📣 дословная цитата · ⚠️ пересказ · ❌ не найдено

Оценок выполнимости здесь нет намеренно — это сырьё, а не решения.

## 1. Главное: рынок делится на три слоя

Разные моды занимают **разные слои одной цепочки**, поэтому и объявляют себя совместимыми друг с другом.
Это ключ ко всей картине.

**Слой 1 — до высадки**
*Какой рейд придёт, зачем, каким способом*

Цель рейда · выбор тактики захода · способ прибытия · память фракции между рейдами · адаптация к обороне игрока · строка в письме.

**Занят: Raid Tactics (07.08.2026, ~8000 подписчиков за 4 недели).**

**Слой 2 — стратегия группы на карте**
*Фазы рейда, роли, когда уходить*

Кто чем занят внутри рейда · координация групп · **условие победы и ухода** · реакция на потери.

**Занят частично:** ваниль (одно условие ухода на всех) + CAI (деление на группы через `Rand`).

**Слой 3 — тактика в бою**
*Что пешка делает прямо сейчас*

Укрытия · отход и гистерезис · фланги · прикрытие огнём · приоритет целей · не лезть в киллбокс.

**Претенденты сломаны:** CAI мёртв на 1.6 и дёргается, Smarter Raider AI «still just as mindless».

> **Почему Raid Tactics честно пишет «совместим с CAI и SRAI».** Автор дословно
> 📣: «CAI 5000 and Smarter Raider AI change how raiders behave **once they are on your map**.
> This one picks the tactic and arrival mode **before they land**. Different layers, they go well together.»
>
> Он не преувеличивает — его мод действительно не трогает поведение в бою.

> **Но окно закрывается.** Автор Raid Tactics, 10.08.2026 📣:
> «The mod will no longer decide things before an attack **but it will also do this during the attack**».
> То есть он объявил намерение зайти в слой 3. Темп у него: **10 обновлений за 17 дней**,
> отвечает в комментариях в тот же час.

## 2. Raid Tactics — полный разбор

| Параметр | Значение | Параметр | Значение |
|---|---|---|---|
| Опубликован | 07.08.2026 | Обновлён | 24.08.2026 |
| Подписчиков | ~8 000 | Рейтинг | 5★ (226 оценок), 82 комментария |
| Зависимости | только Harmony | Исходники | ❌ GitHub нет, только Ko-fi |

### Цели рейда — дословно от автора

| Тип | Цитата автора |
|---|---|
| **Probe** | «A faction that has not fought you sends people to find out how you fight. They carry nothing away and break off early.» |
| **Plunder** | «The ordinary raid, exactly as the game already makes it.» |
| **Slaving** | «Humanlike factions sometimes come for people instead of goods.» |
| **Revenge** | «Only after a faction has been beaten badly and knows why. Takes nothing, kidnaps nobody, **will not break off**.» |

### Четыре оси захода и чтение базы

**Direct** — идут прямо · **Breach** — через стену там, где вы не смотрели ·
**Standoff** — не подходят и обстреливают · **Bypass** — обходят периметр целиком.

База измеряется по тем же четырём осям, кэш обновляется **не чаще раза в 2500 тиков**:
стена против размера домашней зоны, двери на границе, турели на колониста, толстая крыша.
📣 «Turrets packed behind one entrance raise standoff hardest» ·
«A tight chokepoint raises bypass» · «Thick roof cancels most of that».

### Память фракции

Помнит **фракция**, не рейд. Два разных знания 📣:
«With **no survivor** they know the direct approach kills their people, so the next group is careful but
picks its alternative **fairly blind**. **With survivors** they pick the thing that actually beats your setup.»

Кто доносит: сбежавшие с карты, отпущенные пленные, ушедшие через край карты.
📣 «Machines and hives always know, they were reporting the whole time.»

### Чего Raid Tactics НЕ делает

- 📣 «Right now it only focusses on raids happening at your base» — только рейды на базу
- Не трогает поведение в бою вообще: ни укрытий, ни отхода, ни флангов, ни приоритета целей
- Не трогает **условия победы и ухода** рейда — ванильные триггеры остаются как есть
- Не делает выучку/компетентность — только характер фракции
- 📣 «It never blocks, cancels or delays a raid» — принципиально не подменяет рейд
- Открытый эксплойт: отпущенного пленного можно тут же снова арестовать (MrUglyFace, 29.08, не закрыто)

> **Отзыв, который стоит всех остальных** 📣 JustSomeEggsInAPot, 10.08:
> «This mod actually **made me play differently**. I hate breachers and they just kept sending breachers,
> so I changed my usual base style.»
>
> Это доказательство, что слой 1 работает как ценность: игрок поменял способ строить базу.

## 3. Голос игроков: боли ванильного ИИ

### Самая частая — ломают стены вместо прохода

*Повторяется у разных людей в 2020, 2021, 2024, 2025*

> 📣 «is there a mod that fixes ai so it doesnt spend 90% of its time attacking random pieces of wall **even with a clear line to your colonists**»
> — Ashardalon, 23.08.2020

> 📣 «If they can't see anything worth smashing or stealing they will just start opening walls.»
> — MadArtillery, 01.05.2024

> 📣 «the ai of this game is just **made to annoy, not to be a challenge**»
> — Ashardalon, 23.08.2020

⚠️ Контрточка, важная: часть сообщества считает это поломкой сборки жалующегося, а не багом игры
(liosalpha: «In my 12k hours... i have never seen this happen»). Консенсуса нет.

### Уходят, «удовлетворённые уроном», не нанеся урона

> 📣 «They just leave with message: raiders are happy with dealt damage. **But they didnt dealt any damage at all!**»
> — Michał, 29.08.2021

Это ровно то, что мы воспроизвели в игре с додо-аборигенами. Жалоба существует с 2018 года.

### Выбор целей и самосохранение

> 📣 «Realistic raiders would... **Avoid getting into a direct fight unless absolutely necessary** (self preservation)»
> — The Blind One, 11.08.2023

## 4. Киллбокс: обе стороны

> 📣 «Killboxes are arguably the least interesting thing about Rimworld... turn a raid that would normally require clever positioning, defense and tactics into a **dumbed down version of Bloons TD**»
> — livon, 18.06.2025

> 📣 «If your going to use killboxes you might as well just disable raids.»
> — XelNigma, 30.12.2022

**Отпор — сильный и немедленный:**

> 📣 «I just love it when people try to be the 'games police' and govern how people play their **SINGLE player game**»
> — Ashley, 18.06.2025

> 📣 «They're not unfair nor unfun. **The game is unfair to the player**... building killboxes is merely answering with similar amount of force.»
> — Veylox, 07.04.2025

> 📣 «I personally almost always build killboxes because **I like not having my beloved pawns die**.»
> — Vegetation Bovine

> **Найдена формулировка искомого баланса** 📣 Corvantus, 08.04.2025:
> «Make a good killbox **but not a great one**... always leave a few enemy pawns alive... wipes out about
> 60 to 70% of the raid — this makes it so I can still have some of the raid to actually fight but not so much
> that I am run over by numbers.»
>
> Единичное высказывание, но это точное описание того, чего человек хочет от боя.

## 5. Что считают нечестным

*Повторяется у разных людей, 2022–2025 — это самая эмоциональная тема во всём корпусе*

> 📣 «The fact that raiders can just blow through the roof of your base **like they're helldivers** is the absolute worst thing about Rimworld.»
> — pmemer, май 2025

> 📣 «I do feel like the events are programmed to **maximise tragedy**. Drop pod raid? It will land in a child's room.»
> — Aranador, 28.10.2022

И механическая деталь, после которой «невезение» перестаёт читаться как невезение
📣 Astasia: «Center drop raids look at the current location of all your colonists
and pick a weighted location **roughly in the middle of that** to land.»

## 6. Голос игроков: моды. Числа

| Мод | Подписчиков | Версии | Состояние |
|---|---|---|---|
| **Search and Destroy (Continued)**<br>ИИ для СВОИХ пешек | 251 690 | 1.0–1.6 | жив |
| **CAI 5000 (continued, 6jun)** | 34 067 | 1.6 | «временный порт», 6 мес |
| **CAI 5000 (оригинал)** | 24 606 | 1.4–1.5 | ❌ мёртв с 18.04.2024 |
| **Enemy Self Preservation** | 13 468 | 1.1–1.6 | жив |
| **Smarter Raider AI** | 10 330 | 1.4–1.6 | жив, автор отвечает |
| **Raid Tactics** | ~8 000 | 1.6 | 4 недели, быстрый рост |
| **Ketaros Smart Raiders** | 1 160 | 1.6 | 3★ — единственный низкий |
| **CAI: Stop Kitting Edition** | 896 | 1.5 | форк ради одной механики |

> **Два числа, которые говорят о спросе больше остального.**
>
> Search and Destroy — 251 690 подписчиков. Это авто-управление **своими** пешками:
> людям нужно, чтобы ИИ дрался за них. В 7–10 раз больше, чем у любого мода про ИИ противника.
>
> CAI (continued) набрал 34 067 за полгода — **обогнав оригинал с 24 606**,
> который висит два с половиной года. Спрос на боевой ИИ под 1.6 не удовлетворён.

## 7. За что ругают CAI — и почему это важно

Форк **Stop Kitting Edition** существует ради одной механики. Из его описания
📣: «A new option, "Enable Retreat Rework," has been added, which
**significantly reduces the kiting behavior of enemies**... to a more acceptable level.»

Как игроки описывают киттинг своими словами:

> 📣 «most of the pawns attacking me are now **moving some pixels forward, then backward**, basically in standstill... they seem to be stuck in this kind of loop»
> — BladeofSharpness, 12.11.2024

> 📣 «Raiders dont sap or flank, just **wobble around and kite**. This happens on highest difficulty»
> — Shmuck Bazooker, 06.08.2026 — страница 1.6

> 📣 «This mod makes raiders idle infront of my base in the 'Retreating' state **until they starve**»
> — Lucerne, 15.05.2026

> 📣 «I heard it makes mechanoids very tedious to deal with due to constant kiting»
> — Tam, 20.01.2025 — **человек ещё не ставил мод**, знает по слухам

И перевёртыш, который повторяется независимо у двух людей:

> 📣 «My experience as well unfortunately. **The mod effectively makes the game easier** :(»
> — FlinNice, 21.01.2025, в ответ на разбор Wish Granter

Прочие категории жалоб: поломка Anomaly (повторяется в трёх местах) ·
раздутость — 📣 «Could anyone make a **less bloated version** of this that only has the Combat AI?» ·
зависимость от Prepatcher — 📣 «That mod ♥♥♥♥s itself way too often» ·
баги тумана войны · производительность.

**И при этом хвалят так:**

> 📣 «**Every single raid, from big to small, now requires my full attention.**»
> — RedPine, 10.03.2023 — лучшая формулировка ценности во всём корпусе

> 📣 «CAI5000 is excellent, but be prepared — **standard defences like killboxes won't work with it**»
> — Radiosity, 20.01.2025

> 📣 «CAI 5000 **melted my screen**. Kinda afraid to test it again.» ... «the only thing that resembles anything close to what I'm looking for»
> — Stray, 26.04.2026 — терпят, потому что заменить нечем

## 8. Enemy Self Preservation — обратная ошибка

> 📣 «EVERYONE flees after getting shot once basically, it's really anti-climatic. "Holy crap, a huge raid coming up, everyone get read- Oh nvm, **my three turrets alone made all of them flee**"»
> — Merkurio, 22.11.2025 — двое независимо согласились

> 📣 «Could you make it apply **by faction**? Because I don't think Kriegers are supposed to run away.»
> — RandomEdits

⚠️ Симметрия с CAI: там ИИ отступает **слишком дёргано**, здесь — **слишком охотно**.
Оба раза это главный повод удалить мод. Порог отступления — самая хрупкая настройка в жанре.

## 9. Хотелки: закрытые и незакрытые

| Хотелка | Кем закрыта |
|---|---|
| Обходить зоны обстрела турелей и мобилизованных | Smarter Raider AI |
| Реакция на смерть товарища | Smarter Raider AI |
| Фракции меняют тактику после разгрома | **Raid Tactics** (авг 2026) |
| Не переть колонной в мясорубку | CAI 5000 (частично, ценой киттинга) |
| Рейдеры уходят слишком рано | Mehni's Misc, Raiders Never Retreat |

### Никем не закрыто

| Хотелка | Цитата |
|---|---|
| Осада как настоящая осада | 📣 «If I show up to besiege a settlement, that's what I would like to be able to conduct. **Not cheese. Conduct.**» — Stray, 26.04 |
| Скоординированный пролом одной стены | 📣 «make the raiders **work together to destroy a single piece of wall** instead of all hitting different parts» — Wesdabest, 13.08.2026 |
| Целить двери с малым HP вместо стен | 📣 Wesdabest, 13.08.2026 |
| Середина между «ноль реакции» и «мгновенная атака» | 📣 «There has got to be a **happy medium** between zero reaction to half of the raid being killed and immediately attacking» — Azerbaijan_Technology, 12.08.2026 |
| Рейд за освобождением пленных | 📣 AtheistIII, 11.08.2026 — в Raid Tactics нет |
| Сапёры к складам, а не в спальни | 📣 The Blind One, 20.10.2022 |
| Выкуп, дань, возврат тел | 📣 Fallow, 20.10.2022 |
| Гранулярные тумблеры фич | 📣 повторяется у CAI и у Raid Tactics |

## 10. Производительность — что игроки считают приемлемым

❌ Числовых порогов TPS **никто не называет**. Только качественно:
«слайд-шоу», «5 fps», «подлагивает каждые 3–4 секунды», «melted my screen».

> 📣 «this mod has an incompatibility... forcing me into like **5fps** lol. however **the mod works perfect for the most part though… thanks for keeping CAI alive!**»
> — kill bastard, 02.08.2025

⚠️ Человек с 5 FPS благодарит автора. Пока альтернативы нет,
ценность механики перевешивает измеримую боль.

⚠️ Важный разрез: у CAI лагает **не ИИ, а туман войны** — при выключенном
тумане подтормаживания исчезают (Yrm1e, 19.03.2026).

## 11. Чего не удалось собрать

- ❌ **Reddit** — 403 на все попытки. Это большая дыра: развёрнутые разборы «почему я снёс CAI» живут именно там
- ❌ **ludeon.com/forums** — 403
- ❌ Комментарии YouTube — не извлекаются
- ❌ Счётчики поддержки хотелок — Steam не отдаёт лайки в тексте, частота оценена по повторам
- ❌ Прямых заявлений «сломал сейв» не найдено ни одного
- ⚠️ Steam доступен только через модель-извлекатель, часть длинных цитат обрезана — перед публичным цитированием проверять по оригиналу

---

_Переписано из HTML-артефакта вручную 07.09.2026._
