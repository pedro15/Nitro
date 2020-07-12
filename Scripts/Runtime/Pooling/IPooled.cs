namespace Nitro.Pooling
{
    public interface IPooled
    {
        void OnSpawn();
        void OnRecycle();
    }
}