using UnityEngine;

namespace Nitro.Pooling
{
    [CreateAssetMenu(menuName = "Nitro/Pool Definition" , order = -10)]
    public class PoolDefinition : ScriptableObject
    {
        public RecycleBinData[] poolData = default;
    }
}
