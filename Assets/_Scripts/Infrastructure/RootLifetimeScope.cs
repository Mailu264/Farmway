using VContainer;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IAssetProvider, AssetProvider>(Lifetime.Singleton);
            builder.Register<IConfigProvider, ConfigProvider>(Lifetime.Singleton);
            builder.Register<IObjectPool, ObjectPool>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
            builder.Register<ISaveService, SaveService>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}
