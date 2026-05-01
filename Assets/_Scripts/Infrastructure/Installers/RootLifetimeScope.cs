using VContainer;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IAssetProvider, AssetProvider>(Lifetime.Singleton);
            builder.Register<IStaticDataProvider, StaticDataProvider>(Lifetime.Singleton);
            builder.Register<IObjectPool, ObjectPool>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);

            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
