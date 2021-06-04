using System;
using System.Collections.Generic;
using AnKuchen.Map;
using UnityEngine;

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

        Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f);
        void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f);
        void DestroyCachedGameObjects();
    }

    public enum ScrollToType
    {
        Top,
        Bottom,
        Near,
        Center,
    }

    public interface IListContentEditor<T1> : IDisposable where T1 : IMappedObject
    {
        List<UIFactory<T1>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
    }

    public interface IListContentEditor<T1, T2> : IDisposable where T1 : IMappedObject where T2 : IMappedObject
    {
        List<UIFactory<T1, T2>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
    }

    public interface IListContentEditor<T1, T2, T3> : IDisposable where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject
    {
        List<UIFactory<T1, T2, T3>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
    }

    public interface IListContentEditor<T1, T2, T3, T4> : IDisposable where T1 : IMappedObject where T2 : IMappedObject where T3 : IMappedObject where T4 : IMappedObject
    {
        List<UIFactory<T1, T2, T3, T4>> Contents { get; set; }
        float Spacing { get; set; }
        Margin Margin { get; set; }
        int SpareElement { get; set; }
    }
}
