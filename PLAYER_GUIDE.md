# Как работает система игрока

## Общая картина

Когда игрок появляется в игре, создаётся его окружение — **PlayerScope**. Внутри него живут все сервисы игрока. Сервисы — это классы которые отвечают за конкретное поведение: движение, анимацию, взаимодействие с объектами и т.д.

```
PlayerScope
├── PlayerView       — компонент на GameObject игрока (физика, спрайт)
├── PlayerServices   — управляет всеми сервисами (включает/выключает их)
├── PlayerMovementService — движение
└── ... твои новые сервисы
```

---

## Как добавить новый сервис

### Шаг 1 — Создай файл

Создай новую папку и файл в `Assets/_Scripts/Gameplay/Player/Services/`.  
Например: `Services/Animation/PlayerAnimationService.cs`

### Шаг 2 — Напиши класс

```csharp
using Farmway.Gameplay.Player;

namespace Farmway.Gameplay.Player
{
    public class PlayerAnimationService : PlayerService
    {
        // Вызывается один раз при старте
        public override void OnInitialize()
        {
        }

        // Вызывается каждый кадр
        public override void OnUpdate()
        {
        }

        // Вызывается каждый физический кадр (для физики и движения)
        public override void OnFixedUpdate()
        {
        }

        // Вызывается когда сервис уничтожается
        public override void OnDispose()
        {
        }
    }
}
```

> Переопределяй только те методы которые нужны. Если анимация не использует физику — `OnFixedUpdate` не нужен, просто не пиши его.

### Шаг 3 — Зарегистрируй сервис

Открой `Assets/_Scripts/Gameplay/Player/PlayerScope.cs` и добавь одну строчку:

```csharp
private void RegisterPlayerServices(IContainerBuilder builder)
{
    builder.Register<IPlayerServices, PlayerServices>(Lifetime.Scoped).AsImplementedInterfaces();
    builder.Register<PlayerMovementService>(Lifetime.Scoped).As<PlayerService>();
    builder.Register<PlayerAnimationService>(Lifetime.Scoped).As<PlayerService>(); // ← добавь сюда
}
```

Всё. Сервис автоматически подхватится системой.

---

## Доступ к данным игрока

В каждом сервисе уже есть доступ к базовым данным через базовый класс `PlayerService`:

```csharp
PlayerView.Rigidbody2D   // физическое тело
PlayerView.SpriteRenderer // спрайт

PlayerConfig.Speed        // скорость из конфига
```

---

## Если сервису нужны дополнительные зависимости

Например, сервис анимации хочет знать про ввод с клавиатуры:

```csharp
public class PlayerAnimationService : PlayerService
{
    private readonly IInputService _inputService;

    // Зависимости передаются через конструктор
    public PlayerAnimationService(IInputService inputService)
    {
        _inputService = inputService;
    }

    public override void OnUpdate()
    {
        var movement = _inputService.MovementVector;
        // логика анимации
    }
}
```

> Не нужно ничего делать вручную — зависимости подставятся автоматически.

---

## Включение и выключение сервисов

Иногда нужно остановить определённый сервис (например заблокировать движение пока открыт инвентарь). Это делается через `IPlayerServices`:

```csharp
// Выключить конкретный сервис
_playerServices.DisableService<PlayerMovementService>();

// Включить обратно
_playerServices.EnableService<PlayerMovementService>();

// Выключить все сервисы сразу
_playerServices.DisableServices();

// Включить все
_playerServices.EnableServices();
```

---

## Частые ошибки

**Забыл зарегистрировать сервис в PlayerScope** — сервис не будет работать, никакой ошибки не будет.

**Физику написал в `OnUpdate` вместо `OnFixedUpdate`** — движение будет дёргаться, всегда используй `OnFixedUpdate` для всего что связано с `Rigidbody2D`.

**Хочешь получить другой сервис внутри сервиса** — не делай этого напрямую. Лучше вынеси общие данные в отдельный класс и инжектируй его в оба сервиса.
