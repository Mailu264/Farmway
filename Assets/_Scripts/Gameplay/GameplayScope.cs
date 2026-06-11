using Farmway.Gameplay.Farm;
using Farmway.Gameplay.Player;
using Farmway.Gameplay.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayScope : LifetimeScope
    {
        [SerializeField] private GameplaySceneView _gameplaySceneView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplaySceneView);
            RegisterSingletonGameServices(builder);
            RegisterGameServices(builder);
            RegisterFarm(builder);
            builder.RegisterEntryPoint<GameplayBootstrapper>();
        }

        private static void RegisterSingletonGameServices(IContainerBuilder builder)
        {
            builder.Register<IPlayerFactory, PlayerFactory>(Lifetime.Scoped);
        }

        private static void RegisterGameServices(IContainerBuilder builder)
        {
            builder.Register<GameTimeService>(Lifetime.Scoped)
                .As<IGameTimeService>()
                .As<GameServiceBase>();

            // Без AsImplementedInterfaces: жизненный цикл вручную через GameServicesRunner,
            // чтобы тики не зависели от VContainer PlayerLoop и не задвоились
            builder.Register<GameServices>(Lifetime.Scoped).AsSelf().As<IGameServices>();
        }

        private void RegisterFarm(IContainerBuilder builder)
        {
            // Грид строится в рантайме — настройка сцены не нужна
            var gridGo = new GameObject("FarmGrid");
            var grid = gridGo.AddComponent<Grid>();
            var farmGridView = gridGo.AddComponent<FarmGridView>();
            farmGridView.Construct(grid);

            builder.RegisterInstance(farmGridView);
            builder.RegisterInstance(new FarmCellViewFactory(grid, gridGo.transform));
            builder.RegisterInstance(new PlantViewFactory(grid, gridGo.transform));

            builder.Register<FarmGrid>(Lifetime.Scoped);
            builder.Register<PlantFactory>(Lifetime.Scoped);
            builder.Register<FarmService>(Lifetime.Scoped).AsSelf().As<GameServiceBase>();
            builder.Register<PlantSystemService>(Lifetime.Scoped).AsSelf().As<GameServiceBase>();
            builder.Register<DayLightService>(Lifetime.Scoped).As<GameServiceBase>();

            builder.Register<FarmGridPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
