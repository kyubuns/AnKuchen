using System;
using AnKuchen.Map;

namespace AnKuchen.KuchenList
{
    public class UIFactory<T1> where T1 : IMappedObject
    {
        public Action<T1> Callback1 { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }
    }

    public class UIFactory<T1, T2> where T1 : IMappedObject where T2 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Action<T2> Callback2 { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Action<T2> callback2)
        {
            Callback2 = callback2;
        }
    }
}