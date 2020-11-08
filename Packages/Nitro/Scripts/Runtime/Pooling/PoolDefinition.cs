using UnityEngine;
using System.Collections.Generic;

namespace Nitro.Pooling
{
    [CreateAssetMenu(menuName = "Nitro/Pool Definition" , order = -10)]
    public class PoolDefinition : ScriptableObject
    {
        public List<RecycleBinData> poolData = new List<RecycleBinData>();
    }
}