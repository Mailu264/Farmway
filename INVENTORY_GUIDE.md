# Как работает инвентарь и хотбар

## Общая картина

Инвентарь и хотбар работают так:

```
PlayerConfig
  └── сколько слотов создать (InventorySlotCount, HotbarSlotCount)
         ↓
При старте игры слоты создаются автоматически из префаба
         ↓
Предметы кладутся в слоты когда игрок их подбирает
         ↓
InventoryView / HotbarView показывают слоты на экране
```

**Важно:** слоты больше не расставляются руками в иерархии. Ты настраиваешь только **один префаб слота** и **контейнер**, куда они будут спавниться. Количество слотов берётся из конфига.

---

## Что нужно настроить в Unity

### 1. PlayerConfig — количество слотов

Файл лежит тут:
```
Assets/_Data/Player/PlayerConfig.asset
```

| Поле | Что значит |
|---|---|
| `Inventory Slot Count` | сколько ячеек в сумке инвентаря |
| `Hotbar Slot Count` | сколько ячеек в хотбаре (максимум 9, т.к. клавиши 1–9) |

Поставь нужные значения. При нажатии Play создастся ровно столько слотов.

---

### 2. Префаб слота

Слот — это один объект с компонентом `InventorySlotView`. Это **единственный** объект который нужно сделать, один и тот же используется и в инвентаре и в хотбаре (или можно сделать разные если нужен другой вид).

Типичная иерархия объекта внутри префаба:

```
Slot (InventorySlotView)
├── Background      ← Image, Raycast Target = true
├── Icon            ← Image, Raycast Target = false
├── Count           ← TextMeshPro, Raycast Target = false
└── SelectedFrame   ← GameObject (показывается у активного слота хотбара)
```

На объекте `Slot` должен висеть компонент `InventorySlotView`:

| Поле | Что поставить |
|---|---|
| `Icon Placement` | Image для иконки предмета |
| `Count` | TMP текст количества |
| `Selected Frame` | объект-рамка для активного слота хотбара |

> **Raycast Target:** у фона включи, у иконки и текста выключи. Иначе иконка будет перехватывать мышку и drag & drop не будет работать.

---

### 3. InventoryView — окно инвентаря

На объекте окна инвентаря должен висеть компонент `InventoryView`.

| Поле | Что поставить |
|---|---|
| `Drag View` | объект с компонентом `InventoryDragView` |
| `Slot Prefab` | префаб слота (из шага 2) |
| `Slots Container` | пустой `RectTransform` внутри окна, куда будут спавниться слоты |

Типичная иерархия объекта инвентаря:

```
InventoryPanel (InventoryView)
├── Background
├── SlotsContainer    ← сюда спавнятся слоты, повесь Grid Layout Group
└── DragView          ← InventoryDragView
    ├── Icon
    └── Count
```

На `SlotsContainer` поставь **Grid Layout Group** чтобы слоты расставлялись красиво по сетке. Объект не нужно заполнять руками — при старте игры слоты создадутся сами.

---

### 4. HotbarView — хотбар

На объекте хотбара должен висеть компонент `HotbarView`.

| Поле | Что поставить |
|---|---|
| `Drag View` | объект с компонентом `InventoryDragView` |
| `Slot Prefab` | префаб слота |
| `Slots Container` | пустой `RectTransform` куда будут спавниться слоты |

Типичная иерархия:

```
HotbarPanel (HotbarView)
├── Background
├── SlotsContainer    ← сюда спавнятся слоты, повесь Horizontal Layout Group
└── DragView          ← InventoryDragView
    ├── Icon
    └── Count
```

На `SlotsContainer` поставь **Horizontal Layout Group** чтобы слоты встали в ряд.

---

### 5. InventoryDragView — предмет при перетаскивании

Это отдельный объект для иконки, которая летит за мышкой когда игрок тащит предмет. Нужно по одному в `InventoryView` и в `HotbarView`.

| Поле | Что поставить |
|---|---|
| `Icon` | Image для иконки |
| `Count` | TMP текст количества |

У `Icon` и `Count` обязательно выключи **Raycast Target** — иначе drag объект будет перехватывать мышку и drop на слоты не сработает.

---

### 6. GameplaySceneView — связать с кодом

На объекте `GameplaySceneView` на сцене назначь:

| Поле | Что поставить |
|---|---|
| `Inventory View` | объект с компонентом `InventoryView` |
| `Hotbar View` | объект с компонентом `HotbarView` |

Без этого код не найдёт вьюхи и вылетит с ошибкой при старте.

---

## ItemsConfig — добавление предметов

Файл конфига лежит тут:
```
Assets/_Data/Items/ItemsConfig.asset
```

Каждый элемент списка — один вид предмета:

| Поле | Что значит |
|---|---|
| `ItemId` | уникальный идентификатор (выбирается из списка) |
| `ItemType` | тип предмета (Plant, Tool и т.д.) |
| `Icon` | спрайт-иконка, показывается в слоте |
| `Name` | название |
| `MaxStackSize` | сколько штук помещается в один слот |
| `IsStackable` | можно ли складывать в стак |

Пример растения:
```
ItemId:       Item1
ItemType:     Plant
Icon:         [спрайт морковки]
Name:         Carrot
MaxStackSize: 20
IsStackable:  true
```

Пример инструмента:
```
ItemId:       Item2
ItemType:     Tool
Icon:         [спрайт тяпки]
Name:         Hoe
MaxStackSize: 1
IsStackable:  false
```

**Правила:**
- Не используй `ItemId.None` — это зарезервированное значение для пустого слота.
- У каждого предмета должен быть **уникальный** `ItemId`.
- Если `IsStackable = false` — ставь `MaxStackSize = 1`.
- Если `Icon` пустой — предмет не будет видно в инвентаре.

---

## Как работает перетаскивание

1. Игрок нажимает на слот с предметом и начинает тащить.
2. Оригинальный слот прячется, вместо него летит `DragView` с иконкой.
3. Отпустить предмет можно на любой слот инвентаря или хотбара:
   - **пустой слот** — предмет переезжает туда
   - **такой же предмет и есть место в стаке** — стаки объединяются
   - **другой предмет** — слоты меняются местами
4. Если отпустить не на слот — предмет возвращается назад.

Перетаскивать можно **между инвентарём и хотбаром** — это работает.

---

## Управление хотбаром

| Действие | Что делает |
|---|---|
| Клавиши **1–9** | выбирает соответствующий слот хотбара |
| **Скролл мышью вверх** | переключает на предыдущий слот |
| **Скролл мышью вниз** | переключает на следующий слот |
| Активный слот | подсвечивается рамкой `SelectedFrame` |

Количество рабочих клавиш равно `HotbarSlotCount` из `PlayerConfig` (максимум 9).

---

## Частые проблемы

**Слоты не появляются при запуске**

Проверь:
- в `PlayerConfig` выставлен `InventorySlotCount` и `HotbarSlotCount` (не 0)
- в `InventoryView` / `HotbarView` назначены `Slot Prefab` и `Slots Container`

**Предмет не перетаскивается**

Проверь:
- на сцене есть `EventSystem`
- на Canvas есть `GraphicRaycaster`
- у фона слота включен `Raycast Target`
- слот не пустой

**В пустой слот нельзя перетащить предмет**

У пустого слота нет активного элемента с `Raycast Target`. Убедись что у **фоновой картинки** слота `Raycast Target` включен — она всегда видима, даже когда слот пустой.

**Drag иконка перехватывает drop**

У `DragView/Icon` и `DragView/Count` выключи `Raycast Target`.

**Иконка предмета не показывается**

Проверь:
- поле `Icon` у предмета в `ItemsConfig` заполнено
- поле `Icon Placement` в `InventorySlotView` назначено

**Количество не показывается**

Количество показывается только если предметов **больше 1**. Один предмет — текст пустой, это нормально.

**В консоли `Duplicate item id` при старте**

Два предмета в `ItemsConfig` имеют одинаковый `ItemId`. Поменяй один из них.

**В консоли `Slot count mismatch`**

Это предупреждение — количество слотов в модели и во вьюхе не совпало. Проверь что `Slot Prefab` и `Slots Container` правильно назначены в компоненте.

---

## Чеклист перед запуском

- [ ] `PlayerConfig` — выставлены `InventorySlotCount` и `HotbarSlotCount`
- [ ] `ItemsConfig` — у каждого предмета есть уникальный `ItemId` и `Icon`
- [ ] `InventoryView` — назначены `Slot Prefab`, `Slots Container`, `Drag View`
- [ ] `HotbarView` — назначены `Slot Prefab`, `Slots Container`, `Drag View`
- [ ] `GameplaySceneView` — назначены `Inventory View` и `Hotbar View`
- [ ] У каждого `DragView/Icon` и `DragView/Count` выключен `Raycast Target`
- [ ] У фона слота включен `Raycast Target`
- [ ] На `SlotsContainer` инвентаря висит `Grid Layout Group`
- [ ] На `SlotsContainer` хотбара висит `Horizontal Layout Group`
- [ ] На сцене есть `EventSystem`
- [ ] На Canvas есть `GraphicRaycaster`
