using System;
using System.Collections.Generic;
using System.Linq;
using AnKuchen.Extensions;
using AnKuchen.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnKuchen.Layout
{
    public class LayoutEditor : IDisposable
    {
        private readonly ILayouter layouter;
        private readonly IMapper original;
        private readonly LayoutCache cache;
        public List<IMapper> Elements { get; }

        public LayoutEditor(ILayouter layouter, IMapper original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Get().SetActive(false);

            cache = this.original.Get().GetComponent<LayoutCache>();
            if (cache == null)
            {
                cache = this.original.Get().AddComponent<LayoutCache>();
                cache.Elements = new List<GameObject>();
            }
            foreach(var i in cache.Elements) Object.Destroy(i);
            cache.Elements.Clear();

            Elements = new List<IMapper>();
        }

        public IMapper Create()
        {
            var newObject = original.Duplicate();
            newObject.Get().SetActive(true);
            Elements.Add(newObject);
            cache.Elements.Add(newObject.Get());
            return newObject;
        }

        public void Dispose()
        {
            Layout();
        }

        public void Layout()
        {
            layouter.Layout(original, Elements.ToArray());
        }
    }

    public class LayoutEditor<T> : IDisposable where T : IMappedObject, new()
    {
        private readonly ILayouter layouter;
        private readonly T original;
        private readonly LayoutCache cache;
        public List<T> Elements { get; }

        public LayoutEditor(ILayouter layouter, T original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Mapper.Get().SetActive(false);

            cache = this.original.Mapper.Get().GetComponent<LayoutCache>();
            if (cache == null)
            {
                cache = this.original.Mapper.Get().AddComponent<LayoutCache>();
                cache.Elements = new List<GameObject>();
            }
            foreach(var i in cache.Elements) Object.Destroy(i);
            cache.Elements.Clear();

            Elements = new List<T>();
        }

        public T Create()
        {
            var newObject = original.Duplicate();
            newObject.Mapper.Get().SetActive(true);
            Elements.Add(newObject);
            cache.Elements.Add(newObject.Mapper.Get());
            return newObject;
        }

        public void Dispose()
        {
            Layout();
        }

        public void Layout()
        {
            layouter.Layout(original.Mapper, Elements.Select(x => x.Mapper).ToArray());
        }
    }

    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}
