using System;
using System.Collections.Generic;
using System.Linq;
using AnKuchen.Extensions;
using AnKuchen.KuchenLayout.Layouter;
using AnKuchen.Map;
using Object = UnityEngine.Object;

namespace AnKuchen.KuchenLayout
{
    public class Layout<T> where T : IMappedObject, new()
    {
        private readonly T original;
        private readonly ILayouter layouter;
        private List<T> elements;

        public T[] Elements => elements.ToArray();

        public Layout(T original, ILayouter layouter = null)
        {
            if (layouter == null) layouter = new NoneLayouter();

            this.original = original;
            this.layouter = layouter;
            this.elements = new List<T>();

            this.original.Mapper.Get().SetActive(false);
        }

        public LayoutEditor Edit(EditMode editMode = EditMode.Clear)
        {
            return new LayoutEditor(this, editMode);
        }

        private void UpdateContents(List<T> newElements)
        {
            foreach (var element in Elements)
            {
                if (newElements.Contains(element)) continue;
                Object.Destroy(element.Mapper.Get());
            }

            elements = newElements;
            layouter.Layout(original.Mapper, Elements.Select(x => x.Mapper).ToArray());
        }

        public class LayoutEditor : IDisposable
        {
            public List<T> Elements { get; }
            private readonly Layout<T> parent;

            public LayoutEditor(Layout<T> parent, EditMode editMode)
            {
                this.parent = parent;
                Elements = parent.elements.ToList();

                if (editMode == EditMode.Clear) Elements.Clear();
            }

            public T Create()
            {
                var newObject = parent.original.Duplicate();
                newObject.Mapper.Get().SetActive(true);
                parent.elements.Add(newObject);
                Elements.Add(newObject);
                return newObject;
            }

            public void Dispose()
            {
                parent.UpdateContents(Elements);
            }
        }
    }
}

namespace AnKuchen.KuchenLayout.Layouter
{
    public interface ILayouter
    {
        void Layout(IMapper original, IMapper[] elements);
    }
}