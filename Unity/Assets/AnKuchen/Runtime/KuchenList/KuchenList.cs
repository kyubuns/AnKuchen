using UnityEngine;

namespace AnKuchen.KuchenList
{
    public class KuchenList : MonoBehaviour
    {
        public IKuchenListMonoBehaviourBridge List { get; set; }

        public void LateUpdate()
        {
            List.UpdateView();
        }

        public void OnDestroy()
        {
            List.DeactivateAll();
        }
    }

    public interface IKuchenListMonoBehaviourBridge
    {
        void DeactivateAll();
        void UpdateView();
    }
}