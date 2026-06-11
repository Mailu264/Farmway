namespace Farmway.Gameplay.Farm
{
    public abstract class PlantComponent
    {
        protected Plant Plant { get; private set; }

        public void Bind(Plant plant) => Plant = plant;

        public virtual void OnPlanted() { }
        public virtual void OnTick(float hours) { }
        public virtual void Dispose() { }
    }
}
