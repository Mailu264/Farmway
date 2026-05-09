# Как устроен проект

## Структура папок

```
Assets/_Scripts/
├── Infrastructure/   — всё что не зависит от геймплея (загрузка, данные, ввод)
└── Gameplay/         — игровая логика
    └── Player/       — всё про игрока
```

Правило простое: если код можно переиспользовать в другой игре — он в `Infrastructure`. Если это про конкретную механику фермы — он в `Gameplay`.

---

## Как запускается игра

```
Bootstrap сцена
    └── RootLifetimeScope стартует
        └── Bootstrapper.StartAsync()
            ├── Загружает все конфиги
            ├── Прогревает ассеты
            └── Загружает Game сцену
                └── GameplayScope стартует
                    └── GameplayBootstrapper.StartAsync()
                        └── Спавнит игрока
```

> **Важно:** всегда запускай игру из Bootstrap сцены, не из Game сцены. Иначе стартует дважды.

---

## Скоупы (Scopes)

Скоуп — это контейнер в который регистрируются сервисы. Дочерний скоуп видит сервисы родителя, но не наоборот.

```
RootLifetimeScope       (живёт всю игру, DontDestroyOnLoad)
    └── GameplayScope   (живёт пока открыта Game сцена)
            └── PlayerScope  (живёт пока жив игрок)
```

В каждом скоупе есть файл `*Scope.cs` где регистрируются его сервисы.  
В каждом скоупе есть `*Bootstrapper.cs` где пишется стартовая логика.

---

## Конфиги

Конфиги — это ScriptableObject файлы с настройками. Лежат в `Assets/Data/`.

**Как создать новый конфиг:**

### Шаг 1 — Создай класс

```csharp
using UnityEngine;

namespace Farmway.Infrastructure
{
    [CreateAssetMenu(fileName = "FarmConfig", menuName = "Configs/FarmConfig")]
    public class FarmConfig : ScriptableObject
    {
        [field: SerializeField] public float GrowthTime { get; private set; }
        [field: SerializeField] public int StartMoney { get; private set; }
    }
}
```

### Шаг 2 — Создай asset

В Unity: правой кнопкой в папке `Assets/Data` → `Create → Configs → FarmConfig`

### Шаг 3 — Пометь лейблом

В инспекторе у созданного файла добавь Addressables лейбл `Config` — тогда система подгрузит его автоматически при старте.

### Шаг 4 — Используй в коде

```csharp
public class SomeService
{
    private readonly FarmConfig _config;

    public SomeService(IConfigProvider configProvider)
    {
        _config = configProvider.GetConfig<FarmConfig>();
    }
}
```

---

## Загрузка ассетов (AssetProvider)

Для загрузки префабов и других ассетов используй `IAssetProvider`. Он кэширует загруженное — один и тот же ассет грузится только раз.

```csharp
// Загрузить GameObject
var prefab = await _assetProvider.LoadAssetAsync(assetReference, cancellationToken);

// Загрузить компонент сразу
var view = await _assetProvider.LoadAssetAsync<PlayerView>(assetReference, cancellationToken);
```

Ассеты должны быть добавлены в Addressables — в Unity выдели файл и нажми `Make Addressable` в инспекторе.

---

## Пул объектов (ObjectPool)

Пул нужен чтобы не создавать и не удалять объекты каждый раз — это дорого. Вместо этого объекты берутся из пула и возвращаются обратно.

```csharp
// Прогрей пул при старте (создаст N объектов заранее)
_pool.Warmup(bulletPrefab, count: 20);

// Взять объект из пула
var bullet = _pool.GetGameObject(bulletPrefab, position, rotation);

// Вернуть объект в пул когда не нужен
_pool.ReturnGameObject(bullet, bulletPrefab);

// Вернуть с задержкой (например через 3 секунды после попадания)
_pool.ReturnGameObject(bullet, bulletPrefab, seconds: 3f);
```

---

## Ввод (InputService)

Ввод читается через `IInputService`. Он уже обрабатывает клавиатуру и геймпад через Unity Input System.

```csharp
public class SomeService : PlayerService
{
    private readonly IInputService _inputService;

    public SomeService(IInputService inputService)
    {
        _inputService = inputService;
    }

    public override void OnUpdate()
    {
        var movement = _inputService.MovementVector; // Vector2 направление движения
        var isSprint = _inputService.IsSprint;       // bool зажат ли спринт
    }
}
```

Если нужно добавить новую кнопку — открой файл `.inputactions` в Unity (лежит в `Assets/_Scripts/Infrastructure/Services/Input/`), добавь новый Action, пересоздай C# класс (галочка Generate C# Class в инспекторе), затем добавь свойство в `IInputService` и реализацию в `InputService`.

---

## Неймспейсы

Неймспейс — это как папка для кода, чтобы классы с одинаковым именем не конфликтовали и было понятно откуда что пришло.

**Правило проекта:**

| Где лежит файл | Неймспейс |
|---|---|
| `Infrastructure/` | `Farmway.Infrastructure` |
| `Gameplay/` | `Farmway.Gameplay` |
| `Gameplay/Player/` | `Farmway.Gameplay.Player` |
| `Gameplay/Farm/` | `Farmway.Gameplay.Farm` |

> Никогда не пиши неймспейс начинающийся с `_Scripts` — это артефакт дефолтных настроек, у нас так не принято.

---

## VContainer — зачем он нужен

VContainer — это инструмент который автоматически создаёт объекты и передаёт им зависимости. Без него пришлось бы вручную создавать каждый сервис и передавать ему всё что нужно.

**Без VContainer:**
```csharp
var assetProvider = new AssetProvider();
var configProvider = new ConfigProvider(assetProvider);
var inputService = new InputService();
var movementService = new PlayerMovementService(inputService); // и так для каждого
```

**С VContainer — регистрируешь один раз в скоупе:**
```csharp
builder.Register<IAssetProvider, AssetProvider>(Lifetime.Singleton);
builder.Register<IConfigProvider, ConfigProvider>(Lifetime.Singleton);
```

И просто просишь нужное в конструкторе — VContainer сам разберётся и передаст:
```csharp
public class MyService
{
    public MyService(IAssetProvider assetProvider) // VContainer передаст сам
    {
    }
}
```

**Lifetime — время жизни объекта:**
- `Singleton` — создаётся один раз на весь скоуп, все получают один и тот же объект
- `Scoped` — то же что Singleton но в рамках конкретного скоупа
- `Transient` — создаётся новый объект каждый раз когда кто-то его запрашивает

В нашем проекте почти везде `Scoped` — это значит один экземпляр на скоуп.

---

## PlayerView — только данные, никакой логики

`PlayerView` — это мост между кодом и Unity объектом. Она живёт на GameObject игрока и хранит ссылки на компоненты.

**Что должно быть во View:**
```csharp
public class PlayerView : MonoBehaviour
{
    [field: SerializeField] public Rigidbody2D Rigidbody2D { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
}
```

**Что НЕ должно быть во View — логика:**
```csharp
// ❌ Так не надо
public class PlayerView : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += Vector3.up; // логика движения — не место View
    }

    public void TakeDamage(int amount)
    {
        // логика урона — не место View
    }
}
```

**Правило простое:** View только хранит ссылки на компоненты Unity. Всё что "происходит" — в сервисах. Если ловишь себя на том что пишешь логику во View — останови и перенеси в нужный сервис.

---

## Частые ошибки

**Запустила из Game сцены** — запускай только из Bootstrap.

**Ассет не грузится** — проверь что он добавлен в Addressables и помечен нужным лейблом.

**Конфиг не находится** — проверь что asset файл имеет лейбл `Config` в Addressables.

**Новый сервис не работает** — проверь что зарегистрировал его в нужном `*Scope.cs`.

**Зависимость не инжектируется** — сервис из дочернего скоупа не может попасть в родительский. Если `PlayerService` нужен в `GameplayScope` — что-то не так с архитектурой, спроси лида.
