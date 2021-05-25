using System.Collections.Generic;
using AnKuchen.Map;

namespace AnKuchen.KuchenList
{
    public interface IKuchenList
    {
        float Spacing { get; }
        int SpareElement { get; }
        IReadOnlyDictionary<int, IMappedObject> CreatedObjects { get; }
        int ContentsCount { get; }
        IReadonlyMargin Margin { get; }

        float NormalizedPosition { get; set; }

        void DestroyCachedGameObjects();
    }
}
