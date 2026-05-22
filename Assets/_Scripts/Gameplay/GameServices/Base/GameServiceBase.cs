using Farmway.Gameplay.Services.Configs;
using Farmway.Infrastructure;
using UniRx;
using VContainer;

namespace Farmway.Gameplay.Services
{
    public abstract class GameServiceBase : Service
    {
        public CompositeDisposable Disposables { get; } = new();

        public IConfigProvider ConfigProvider { get; private set; }
        public TimeConfig TimeConfig { get; private set; }

        [Inject]
        public void Construct(IConfigProvider configProvider)
        {
            ConfigProvider = configProvider;
            TimeConfig = configProvider.GetConfig<TimeConfig>();
        }

        public override void Dispose()
        {
            Disposables?.Dispose();
            OnDispose();
        }
    }
}
