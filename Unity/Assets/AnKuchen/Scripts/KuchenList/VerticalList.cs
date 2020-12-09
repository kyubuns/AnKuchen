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
        private readonly List<(GameObject, IReusableMappedObject)> items = new List<(GameObject, IReusableMappedObject)>();

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

        public void OnDestroy()
        {
            foreach (var item in items)
            {
                item.Item2.Deactivate();
            }
        }

        private void ClearAll()
        {
            foreach (var item in items)
            {
                item.Item2.Deactivate();
                UnityEngine.Object.Destroy(item.Item1);
            }

            items.Clear();
        }

        private void UpdateListContents()
        {
            ClearAll();

            foreach (var content in contents)
            {
                if (content.Callback1 != null) CreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) CreateNewObject(original2, content.Callback2);
            }
        }

        private void CreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var newObject = original.Duplicate();
            newObject.Mapper.Get().SetActive(true);
            newObject.Activate();
            contentCallback(newObject);
            items.Add((newObject.Mapper.Get(), newObject));
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
            public Margin Margin { get; set; } = new Margin();

            public ListContentEditor(VerticalList<T1, T2> parent)
            {
                this.parent = parent;
                Contents = parent.contents;
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.UpdateListContents();
            }
        }
    }
}