namespace Nitro.Pooling
{
    public interface IPoolCallbacks
    {
        void OnSpawn();
        void OnRecycle();
    }
}