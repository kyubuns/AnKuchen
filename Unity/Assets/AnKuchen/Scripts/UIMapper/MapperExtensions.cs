using UnityEngine;

namespace AnKuchen.UIMapper
{
    public static class MapperExtensions
    {
        public static IMapper Clone(this IMapper self)
        {
            var rootObject = self.Get();
            var clone = Object.Instantiate(rootObject, rootObject.transform.parent, true);
            var uiCache = clone.GetComponent<UICache>();
            if (uiCache == null)
            {
                uiCache = clone.AddComponent<UICache>();
            }
            uiCache.CreateCache();
            return uiCache;
        }
    }
}
