using UnityEngine;

namespace AnKuchen.UIMapper
{
    public static class MapperExtensions
    {
        public static IMapper Duplicate(this IMapper self)
        {
            var rootObject = self.Get();

            var uiCache = rootObject.GetComponent<UICache>();
            if (uiCache == null)
            {
                uiCache = rootObject.AddComponent<UICache>();
                uiCache.Copy(self);
            }

            var clone = Object.Instantiate(rootObject, rootObject.transform.parent, true);
            return clone.GetComponent<UICache>();
        }
    }
}