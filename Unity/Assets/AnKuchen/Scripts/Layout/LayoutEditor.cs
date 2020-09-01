using System;
using System.Collections.Generic;
using System.Linq;
using AnKuchen.Map;

namespace AnKuchen.Layout
{
    public class LayoutEditor : IDisposable
    {
        private readonly ILayouter layouter;
        private readonly IMapper original;
        public List<IMapper> Elements { get; }

        public LayoutEditor(ILayouter layouter, IMapper original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Get().SetActive(false);

            Elements = new List<IMapper>();
        }

        public IMapper Create()
        {
            var newObject = original.Duplicate();
            newObject.Get().SetActive(true);
            Elements.Add(newObject);
            return newObject;
        }

        public void Dispose()
        {
            layouter.Layout(original, Elements.ToArray());
        }
    }

    public class LayoutEditor<T> : IDisposable where T : IMappedObject, new()
    {
        private readonly ILayouter layouter;
        private readonly T original;
        public List<T> Elements { get; }

        public LayoutEditor(ILayouter layouter, T original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Mapper.Get().SetActive(false);

            Elements = new List<T>();
        }

        public T Create()
        {
            var newObject = original.Duplicate();
            newObject.Mapper.Get().SetActive(true);
            Elements.Add(newObject);
            return newObject;
        }

        public void Dispose()
        {
            layouter.Layout(original.Mapper, Elements.Select(x => x.Mapper).ToArray());
        }
    }

    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}
