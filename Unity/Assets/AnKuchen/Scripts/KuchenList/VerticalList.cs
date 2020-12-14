using System;
using System.Collections.Generic;
using AnKuchen.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.KuchenList
{
    public class VerticalList<T1, T2> : IKuchenList where T1 : IReusableMappedObject, new() where T2 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private List<UIFactory<T1, T2>> contents = new List<UIFactory<T1, T2>>();
        private readonly List<(RectTransform, IReusableMappedObject, float Position)> items = new List<(RectTransform, IReusableMappedObject, float Position)>();
        public float Spacing { get; private set; }

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2)
        {
            this.scrollRect = scrollRect;
            this.original1 = original1;
            this.original2 = original2;

            this.original1.Mapper.Get().SetActive(false);
            this.original2.Mapper.Get().SetActive(false);

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = this;
        }

        public void DeactivateAll()
        {
            foreach (var item in items)
            {
                item.Item2.Deactivate();
            }
            items.Clear();
        }

        private void UpdateListContents()
        {
            // clear elements
            foreach (var item in items)
            {
                item.Item2.Deactivate();
                UnityEngine.Object.Destroy(item.Item1.gameObject);
            }
            items.Clear();

            // create elements
            var calcHeight = Margin.Top;
            foreach (var content in contents)
            {
                RectTransform newObject = null;
                IReusableMappedObject newMappedObject = null;
                if (content.Callback1 != null) (newObject, newMappedObject) = CreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) (newObject, newMappedObject) = CreateNewObject(original2, content.Callback2);
                if (newObject == null || newMappedObject == null) continue;

                items.Add((newObject, newMappedObject, calcHeight));
                if (newMappedObject is IListRowHeight listRowHeight)
                {
                    calcHeight += listRowHeight.Height;
                }
                else
                {
                    calcHeight += newObject.rect.height;
                }
                calcHeight += Spacing;
            }
            if (contents.Count > 0) calcHeight -= Spacing; // 最後は要らない
            calcHeight += Margin.Bottom;

            // calc content size
            var contentRectTransform = scrollRect.content.GetComponent<RectTransform>();
            var s = contentRectTransform.sizeDelta;
            contentRectTransform.sizeDelta = new Vector2(s.x, calcHeight);

            // move elements position
            var baseY = calcHeight / 2f;
            foreach (var (rectTransform, _, position) in items)
            {
                var p = rectTransform.anchoredPosition;
                var r = rectTransform.rect;
                rectTransform.anchoredPosition = new Vector3(p.x, baseY - position - r.height / 2f, 0f);
            }
        }

        private (RectTransform, IReusableMappedObject) CreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var newObject = original.Duplicate();
            var newRectTransform = newObject.Mapper.Get<RectTransform>();
            newRectTransform.SetParent(scrollRect.content);
            newObject.Mapper.Get().SetActive(true);
            newObject.Activate();
            contentCallback(newObject);
            return (newRectTransform, newObject);
        }

        public ListContentEditor Edit()
        {
            return new ListContentEditor(this);
        }

        public class ListContentEditor : IDisposable
        {
            private readonly VerticalList<T1, T2> parent;
            public List<UIFactory<T1, T2>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }

            public ListContentEditor(VerticalList<T1, T2> parent)
            {
                this.parent = parent;
                Contents = parent.contents;
                Spacing = parent.Spacing;
                Margin = parent.margin;
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.Spacing = Spacing;
                parent.margin = Margin;
                parent.UpdateListContents();
            }
        }
    }
}