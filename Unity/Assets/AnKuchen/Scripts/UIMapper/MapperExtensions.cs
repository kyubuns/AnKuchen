using UnityEngine;

namespace AnKuchen.UIMapper
{
    public static class MapperExtensions
    {
        public static IMapper Clone(this IMapper self)
        {
            var rootObject = self.Get();

            var uiCache = rootObject.GetComponent<UICache>();
            if (uiCache == null)
            {
                uiCache = rootObject.AddComponent<UICache>();
                uiCache.CreateCache();
            }

            var clone = Object.Instantiate(rootObject, rootObject.transform.parent, true);
            return clone.GetComponent<UICache>();
        }
    }
}
