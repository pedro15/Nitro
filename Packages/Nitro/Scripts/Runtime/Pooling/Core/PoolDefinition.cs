using UnityEngine;
using System.Collections.Generic;

namespace Nitro.Pooling.Core
{
    [CreateAssetMenu(menuName = "Nitro/Object Pool Definition" , order = -10)]
    public class PoolDefinition : ScriptableObject
    {
        [SerializeField]
        private List<RecycleBinData> poolData = new List<RecycleBinData>();

        public RecycleBinData[] PoolData => poolData.ToArray();
    }
}