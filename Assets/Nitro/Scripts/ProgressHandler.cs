using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nitro.Pooling;

namespace Nitro.Demo.ObjectPool
{
    [RequireComponent(typeof(Image))]
    public class ProgressHandler : MonoBehaviour
    {
        private RecycleBin recycleBin = null;

        private Image img = null;

        private void Start()
        {
            img = GetComponent<Image>();
            img.fillAmount = 0;
            Debug.Log("Progress Handler::");
            recycleBin = PoolManager.Instance.GetRecycleBin("Balls");
            Debug.Log("RecycleBin -> " + recycleBin?.GetType().Name);
            
            //if (recycleBin != null)
            //    recycleBin.OnAddressablesLoading += RecycleBin_OnAddressablesLoading;
        }

        private void RecycleBin_OnAddressablesLoading(float progress)
        {
            img.fillAmount = progress * 0.01f;
        }

        private void OnDestroy()
        {
            //if (recycleBin != null)
            //    recycleBin.OnAddressablesLoading -= RecycleBin_OnAddressablesLoading;
        }
    }
}