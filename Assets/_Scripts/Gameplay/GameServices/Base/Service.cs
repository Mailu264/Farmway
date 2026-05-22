namespace Farmway.Gameplay.Services
{
    public abstract class Service
    {
        public bool IsEnabled { get; private set; }

        public void Enable()
        {
            IsEnabled = true;
            OnEnable();
        }

        public void Disable()
        {
            IsEnabled = false;
            OnDisable();
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnInitialize() { }
        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnDispose() { }

        public virtual void Dispose() => OnDispose();
    }
}
