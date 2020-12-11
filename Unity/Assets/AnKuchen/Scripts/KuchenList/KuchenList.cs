using UnityEngine;

namespace AnKuchen.KuchenList
{
    public class KuchenList : MonoBehaviour
    {
        public IKuchenList List { get; set; }

        public void OnDestroy()
        {
            List.DeactivateAll();
        }
    }

    public interface IKuchenList
    {
        void DeactivateAll();
    }
}