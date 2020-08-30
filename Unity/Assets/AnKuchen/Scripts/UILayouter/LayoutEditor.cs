using System;
using System.Collections.Generic;
using AnKuchen.UIMapper;

namespace AnKuchen.UILayouter
{
    public class LayoutEditor : IDisposable
    {
        private readonly ILayouter layouter;
        private readonly IMapper original;
        private readonly List<IMapper> elements;

        public LayoutEditor(ILayouter layouter, IMapper original)
        {
            this.layouter = layouter;

            this.original = original;
            this.original.Get().SetActive(false);

            elements = new List<IMapper>();
        }

        public IMapper Create()
        {
            var newObject = original.Duplicate();
            newObject.Get().SetActive(true);
            elements.Add(newObject);
            return newObject;
        }

        public void Dispose()
        {
            layouter.Layout(original, elements.ToArray());
        }
    }

    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}
