using System;
using System.Collections.Generic;
using AnKuchen.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.KuchenList
{
    public class VerticalList<T1> : IKuchenList
        where T1 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private List<UIFactory<T1>> contents = new List<UIFactory<T1>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IReusableMappedObject> createdObjects = new Dictionary<int, IReusableMappedObject>();
        private readonly Dictionary<Type, List<IReusableMappedObject>> cachedObjects = new Dictionary<Type, List<IReusableMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        public float Spacing { get; private set; }

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public VerticalList(ScrollRect scrollRect, T1 original1)
        {
            this.scrollRect = scrollRect;

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IReusableMappedObject>());

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = this;

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        }

        public void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
            }
            createdObjects.Clear();
        }

        public void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var end = start + displayRect.height;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(index, map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IReusableMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1);
                if (newObject == null) throw new Exception($"newObject == null");
                var p = newObject.anchoredPosition;
                var r = newObject.rect;
                newObject.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - contentPositions[i] - r.height / 2f, 0f);
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
                UnityEngine.Object.Destroy(item.Mapper.Get());
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcHeight = Margin.Top;
            foreach (var content in contents)
            {
                contentPositions.Add(calcHeight);
                if (content.Callback1 != null) calcHeight += original1.Mapper.Get<RectTransform>().rect.height;
                calcHeight += Spacing;
            }
            if (contents.Count > 0) calcHeight -= Spacing; // 最後は要らない
            calcHeight += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcHeight);
        }

        private void CollectObject(int index, IReusableMappedObject target)
        {
            target.Deactivate();
            target.Mapper.Get().SetActive(false);

            var content = contents[index];
            if (content.Callback1 != null) cachedObjects[typeof(T1)].Add(target);
        }

        private (RectTransform, IReusableMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var cache = cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

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
            private readonly VerticalList<T1> parent;
            public List<UIFactory<T1>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }

            public ListContentEditor(VerticalList<T1> parent)
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

    public class VerticalList<T1, T2> : IKuchenList
        where T1 : IReusableMappedObject, new() where T2 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private List<UIFactory<T1, T2>> contents = new List<UIFactory<T1, T2>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IReusableMappedObject> createdObjects = new Dictionary<int, IReusableMappedObject>();
        private readonly Dictionary<Type, List<IReusableMappedObject>> cachedObjects = new Dictionary<Type, List<IReusableMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        public float Spacing { get; private set; }

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2)
        {
            this.scrollRect = scrollRect;

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IReusableMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IReusableMappedObject>());

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = this;

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        }

        public void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
            }
            createdObjects.Clear();
        }

        public void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var end = start + displayRect.height;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(index, map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IReusableMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2);
                if (newObject == null) throw new Exception($"newObject == null");
                var p = newObject.anchoredPosition;
                var r = newObject.rect;
                newObject.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - contentPositions[i] - r.height / 2f, 0f);
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
                UnityEngine.Object.Destroy(item.Mapper.Get());
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcHeight = Margin.Top;
            foreach (var content in contents)
            {
                contentPositions.Add(calcHeight);
                if (content.Callback1 != null) calcHeight += original1.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback2 != null) calcHeight += original2.Mapper.Get<RectTransform>().rect.height;
                calcHeight += Spacing;
            }
            if (contents.Count > 0) calcHeight -= Spacing; // 最後は要らない
            calcHeight += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcHeight);
        }

        private void CollectObject(int index, IReusableMappedObject target)
        {
            target.Deactivate();
            target.Mapper.Get().SetActive(false);

            var content = contents[index];
            if (content.Callback1 != null) cachedObjects[typeof(T1)].Add(target);
            if (content.Callback2 != null) cachedObjects[typeof(T2)].Add(target);
        }

        private (RectTransform, IReusableMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var cache = cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

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

    public class VerticalList<T1, T2, T3> : IKuchenList
        where T1 : IReusableMappedObject, new() where T2 : IReusableMappedObject, new() where T3 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private List<UIFactory<T1, T2, T3>> contents = new List<UIFactory<T1, T2, T3>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IReusableMappedObject> createdObjects = new Dictionary<int, IReusableMappedObject>();
        private readonly Dictionary<Type, List<IReusableMappedObject>> cachedObjects = new Dictionary<Type, List<IReusableMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        public float Spacing { get; private set; }

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3)
        {
            this.scrollRect = scrollRect;

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IReusableMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IReusableMappedObject>());

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IReusableMappedObject>());

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = this;

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        }

        public void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
            }
            createdObjects.Clear();
        }

        public void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var end = start + displayRect.height;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(index, map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IReusableMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3);
                if (newObject == null) throw new Exception($"newObject == null");
                var p = newObject.anchoredPosition;
                var r = newObject.rect;
                newObject.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - contentPositions[i] - r.height / 2f, 0f);
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
                UnityEngine.Object.Destroy(item.Mapper.Get());
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcHeight = Margin.Top;
            foreach (var content in contents)
            {
                contentPositions.Add(calcHeight);
                if (content.Callback1 != null) calcHeight += original1.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback2 != null) calcHeight += original2.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback3 != null) calcHeight += original3.Mapper.Get<RectTransform>().rect.height;
                calcHeight += Spacing;
            }
            if (contents.Count > 0) calcHeight -= Spacing; // 最後は要らない
            calcHeight += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcHeight);
        }

        private void CollectObject(int index, IReusableMappedObject target)
        {
            target.Deactivate();
            target.Mapper.Get().SetActive(false);

            var content = contents[index];
            if (content.Callback1 != null) cachedObjects[typeof(T1)].Add(target);
            if (content.Callback2 != null) cachedObjects[typeof(T2)].Add(target);
            if (content.Callback3 != null) cachedObjects[typeof(T3)].Add(target);
        }

        private (RectTransform, IReusableMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var cache = cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

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
            private readonly VerticalList<T1, T2, T3> parent;
            public List<UIFactory<T1, T2, T3>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3> parent)
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

    public class VerticalList<T1, T2, T3, T4> : IKuchenList
        where T1 : IReusableMappedObject, new() where T2 : IReusableMappedObject, new() where T3 : IReusableMappedObject, new() where T4 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private readonly T4 original4;
        private List<UIFactory<T1, T2, T3, T4>> contents = new List<UIFactory<T1, T2, T3, T4>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IReusableMappedObject> createdObjects = new Dictionary<int, IReusableMappedObject>();
        private readonly Dictionary<Type, List<IReusableMappedObject>> cachedObjects = new Dictionary<Type, List<IReusableMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        public float Spacing { get; private set; }

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3, T4 original4)
        {
            this.scrollRect = scrollRect;

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IReusableMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IReusableMappedObject>());

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IReusableMappedObject>());

            this.original4 = original4;
            this.original4.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T4), new List<IReusableMappedObject>());

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = this;

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
        }

        public void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
            }
            createdObjects.Clear();
        }

        public void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var end = start + displayRect.height;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(index, map);
                removedList.Add(index);
            }

            foreach (var removed in removedList)
            {
                createdObjects.Remove(removed);
            }

            for (var i = displayMinIndex; i <= displayMaxIndex; ++i)
            {
                if (createdObjects.ContainsKey(i)) continue;

                RectTransform newObject = null;
                IReusableMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3);
                if (content.Callback4 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original4, content.Callback4);
                if (newObject == null) throw new Exception($"newObject == null");
                var p = newObject.anchoredPosition;
                var r = newObject.rect;
                newObject.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - contentPositions[i] - r.height / 2f, 0f);
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            foreach (var item in createdObjects.Values)
            {
                item.Deactivate();
                UnityEngine.Object.Destroy(item.Mapper.Get());
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcHeight = Margin.Top;
            foreach (var content in contents)
            {
                contentPositions.Add(calcHeight);
                if (content.Callback1 != null) calcHeight += original1.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback2 != null) calcHeight += original2.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback3 != null) calcHeight += original3.Mapper.Get<RectTransform>().rect.height;
                if (content.Callback4 != null) calcHeight += original4.Mapper.Get<RectTransform>().rect.height;
                calcHeight += Spacing;
            }
            if (contents.Count > 0) calcHeight -= Spacing; // 最後は要らない
            calcHeight += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcHeight);
        }

        private void CollectObject(int index, IReusableMappedObject target)
        {
            target.Deactivate();
            target.Mapper.Get().SetActive(false);

            var content = contents[index];
            if (content.Callback1 != null) cachedObjects[typeof(T1)].Add(target);
            if (content.Callback2 != null) cachedObjects[typeof(T2)].Add(target);
            if (content.Callback3 != null) cachedObjects[typeof(T3)].Add(target);
            if (content.Callback4 != null) cachedObjects[typeof(T4)].Add(target);
        }

        private (RectTransform, IReusableMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var cache = cachedObjects[typeof(T)];
            T newObject;
            if (cache.Count > 0)
            {
                newObject = (T) cache[0];
                cache.RemoveAt(0);
            }
            else
            {
                newObject = original.Duplicate();
            }

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
            private readonly VerticalList<T1, T2, T3, T4> parent;
            public List<UIFactory<T1, T2, T3, T4>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3, T4> parent)
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