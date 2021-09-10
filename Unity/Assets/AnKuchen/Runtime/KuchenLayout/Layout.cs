using System;
using System.Collections.Generic;
using System.Linq;
using AnKuchen.Extensions;
using AnKuchen.KuchenLayout.Layouter;
using AnKuchen.KuchenList;
using AnKuchen.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnKuchen.KuchenLayout
{
    public class Layout<T> : IMappedObjectList where T : IMappedObject, new()
    {
        private readonly T original;
        private readonly ILayouter layouter;
        private List<T> elements;
        private readonly List<T> cachedElements;

        public T[] Elements => elements.ToArray();
        public IMappedObject[] MappedObjects => new[] { (IMappedObject) original };

        public Layout(T original, ILayouter layouter = null)
        {
            if (layouter == null) layouter = new NoneLayouter();

            this.original = original;
            this.layouter = layouter;
            this.elements = new List<T>();
            this.cachedElements = new List<T>();

            this.original.Mapper.Get().SetActive(false);
        }

        public LayoutEditor Edit(EditMode editMode = EditMode.Clear)
        {
            if (editMode == EditMode.Clear)
            {
                // Reverseして頭から詰めていくことで、同じ場所に表示される要素は同じGameObjectが割り当てられる可能性を高くする
                foreach (var element in Elements.Reverse())
                {
                    var gameObject = element.Mapper.Get();
                    gameObject.GetComponent<LayoutElement>().Deactivate();
                    gameObject.SetActive(false);
                    cachedElements.Insert(0, element);
                }
                elements.Clear();
            }
            return new LayoutEditor(this);
        }

        public void Clear(bool purgeCache = false)
        {
            UpdateContents(new List<T>());
            if (purgeCache)
            {
                foreach (var element in cachedElements)
                {
                    Object.Destroy(element.Mapper.Get());
                }
                cachedElements.Clear();
            }
        }

        private void UpdateContents(List<T> newElements)
        {
            foreach (var element in Elements)
            {
                if (newElements.Contains(element)) continue;
                var gameObject = element.Mapper.Get();
                gameObject.GetComponent<LayoutElement>().Deactivate();
                gameObject.SetActive(false);
                cachedElements.Add(element);
            }

            elements = newElements;
            layouter.Layout(original.Mapper, Elements.Select(x => x.Mapper).ToArray());
        }

        public class LayoutEditor : IDisposable
        {
            public List<T> Elements { get; }
            private readonly Layout<T> parent;

            public LayoutEditor(Layout<T> parent)
            {
                this.parent = parent;
                Elements = parent.elements.ToList();
            }

            public T Create()
            {
                T newObject;
                LayoutElement layoutElement;
                if (parent.cachedElements.Count > 0)
                {
                    newObject = parent.cachedElements[0];
                    parent.cachedElements.RemoveAt(0);
                    layoutElement = newObject.Mapper.Get().GetComponent<LayoutElement>();
                }
                else
                {
                    newObject = parent.original.Duplicate();
                    layoutElement = newObject.Mapper.Get().AddComponent<LayoutElement>();
                }

                newObject.Mapper.Get().SetActive(true);
                Elements.Add(newObject);
                if (newObject is IReusableMappedObject reusableMappedObject)
                {
                    reusableMappedObject.Activate();
                    layoutElement.ReusableMappedObject = reusableMappedObject;
                }
                return newObject;
            }

            public void Dispose()
            {
                parent.UpdateContents(Elements);
            }
        }
    }

    public class LayoutElement : MonoBehaviour
    {
        public IReusableMappedObject ReusableMappedObject { get; set; }

        public void OnDestroy()
        {
            Deactivate();
        }

        public void Deactivate()
        {
            ReusableMappedObject?.Deactivate();
            ReusableMappedObject = null;
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
