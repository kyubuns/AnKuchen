using UnityEngine;

namespace AnKuchen.KuchenList
{
    public class KuchenList : MonoBehaviour
    {
        public IKuchenList List { get; set; }

        public void LateUpdate()
        {
            List.UpdateView();
        }

        public void OnDestroy()
        {
            List.DeactivateAll();
        }
    }

    public interface IKuchenList
    {
        void DeactivateAll();
        void UpdateView();
    }
}