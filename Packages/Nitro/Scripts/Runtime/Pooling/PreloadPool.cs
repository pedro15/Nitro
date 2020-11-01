using UnityEngine;

namespace Nitro.Pooling
{
    [CreateAssetMenu(menuName = "Nitro/Pre-load Pool")]
    public class PreloadPool : ScriptableObject
    {
        public RecycleBinData[] poolData = default;
    }
}
