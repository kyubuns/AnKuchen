using System;
using System.Collections.Generic;
using AnKuchen.UIMapper;

namespace AnKuchen.UILayouter
{
    public class LayoutEditor<T> : IDisposable where T : IMappedObject, new()
    {
        private readonly ILayouter layouter;
        private readonly T original;
        private readonly List<IMapper> elements;

        public LayoutEditor(ILayouter layouter, T original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Mapper.Get().SetActive(false);

            elements = new List<IMapper>();
        }

        public T Create()
        {
            var newObject = original.Duplicate();
            newObject.Mapper.Get().SetActive(true);
            elements.Add(newObject.Mapper);
            return newObject;
        }

        public void Dispose()
        {
            layouter.Layout(original.Mapper, elements.ToArray());
        }
    }

    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}
