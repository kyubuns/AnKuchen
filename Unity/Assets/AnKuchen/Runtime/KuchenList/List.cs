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

        void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f);
        void DestroyCachedGameObjects();
    }

    public enum ScrollToType
    {
        Top,
        Bottom,
        Near,
    }
}
