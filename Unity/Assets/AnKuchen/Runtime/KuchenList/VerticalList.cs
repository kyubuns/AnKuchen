using System;
using System.Linq;
using System.Collections.Generic;
using AnKuchen.AdditionalInfo;
using AnKuchen.Extensions;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AnKuchen.KuchenList
{
    public class VerticalList<T1> : IKuchenList
        where T1 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly (string Name, float Size)[] originalInfoCache;
        private List<UIFactory<T1>> contents = new List<UIFactory<T1>>();
        private readonly List<(float, float)> contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => createdObjects;
        public int ContentsCount => contents.Count;
        public ScrollRect ScrollRect => scrollRect;
        public RectTransform ContentRectTransform => scrollRect.content;

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public float NormalizedPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public VerticalList(ScrollRect scrollRect, T1 original1)
        {
            this.scrollRect = scrollRect;

            originalInfoCache = new (string Name, float Size)[1];

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                margin = new Margin
                {
                    Top = verticalLayoutGroup.padding.top,
                    Bottom = verticalLayoutGroup.padding.bottom
                };
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1> list;

            public ListOperator(VerticalList<T1> list)
            {
                this.list = list;
            }

            public void DeactivateAll()
            {
                list.DeactivateAll();
            }

            public void UpdateView()
            {
                list.UpdateView();
            }
        }

        private void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }
            createdObjects.Clear();
        }

        private void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i].Item1) continue;
                if (contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = contentPositions.Count - 1;
                displayMaxIndex = contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
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
                IMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = createdObjects.Values.Count == 0;
            foreach (var map in createdObjects.Values)
            {
                CollectObject(map);
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (additionalInfo != null && additionalInfo.specialSpacings != null)
                ? additionalInfo.specialSpacings
                : new SpecialSpacing[] { };
            for (var i = 0; i < contents.Count; ++i)
            {
                var content = contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = originalInfoCache[0].Name;
                    elementSize = originalInfoCache[0].Size;
                }
                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            target.Mapper.Get().SetActive(false);

            if (target is T1) cachedObjects[typeof(T1)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
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

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1> Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IListContentEditor<T1>
        {
            private readonly VerticalList<T1> parent;
            public List<UIFactory<T1>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1> parent, EditMode editMode)
            {
                this.parent = parent;
                Contents = parent.contents;
                Spacing = parent.Spacing;
                Margin = parent.margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.Spacing = Spacing;
                parent.margin = Margin;
                parent.SpareElement = SpareElement;
                parent.UpdateListContents();
            }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }
                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
            var content = contentPositions[index];
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = - limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = - viewportHeight / 2f;
                var limitMin = - limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            scrollRect.velocity = Vector2.zero;
        }
    }

    public class VerticalList<T1, T2> : IKuchenList
        where T1 : IMappedObject, new() where T2 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly (string Name, float Size)[] originalInfoCache;
        private List<UIFactory<T1, T2>> contents = new List<UIFactory<T1, T2>>();
        private readonly List<(float, float)> contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => createdObjects;
        public int ContentsCount => contents.Count;
        public ScrollRect ScrollRect => scrollRect;
        public RectTransform ContentRectTransform => scrollRect.content;

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public float NormalizedPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2)
        {
            this.scrollRect = scrollRect;

            originalInfoCache = new (string Name, float Size)[2];

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                margin = new Margin
                {
                    Top = verticalLayoutGroup.padding.top,
                    Bottom = verticalLayoutGroup.padding.bottom
                };
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2> list;

            public ListOperator(VerticalList<T1, T2> list)
            {
                this.list = list;
            }

            public void DeactivateAll()
            {
                list.DeactivateAll();
            }

            public void UpdateView()
            {
                list.UpdateView();
            }
        }

        private void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }
            createdObjects.Clear();
        }

        private void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i].Item1) continue;
                if (contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = contentPositions.Count - 1;
                displayMaxIndex = contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
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
                IMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = createdObjects.Values.Count == 0;
            foreach (var map in createdObjects.Values)
            {
                CollectObject(map);
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (additionalInfo != null && additionalInfo.specialSpacings != null)
                ? additionalInfo.specialSpacings
                : new SpecialSpacing[] { };
            for (var i = 0; i < contents.Count; ++i)
            {
                var content = contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = originalInfoCache[0].Name;
                    elementSize = originalInfoCache[0].Size;
                }
                if (content.Callback2 != null)
                {
                    elementName = originalInfoCache[1].Name;
                    elementSize = originalInfoCache[1].Size;
                }
                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            target.Mapper.Get().SetActive(false);

            if (target is T1) cachedObjects[typeof(T1)].Add(target);
            if (target is T2) cachedObjects[typeof(T2)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
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

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2> Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IListContentEditor<T1, T2>
        {
            private readonly VerticalList<T1, T2> parent;
            public List<UIFactory<T1, T2>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2> parent, EditMode editMode)
            {
                this.parent = parent;
                Contents = parent.contents;
                Spacing = parent.Spacing;
                Margin = parent.margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.Spacing = Spacing;
                parent.margin = Margin;
                parent.SpareElement = SpareElement;
                parent.UpdateListContents();
            }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }
                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
            var content = contentPositions[index];
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = - limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = - viewportHeight / 2f;
                var limitMin = - limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            scrollRect.velocity = Vector2.zero;
        }
    }

    public class VerticalList<T1, T2, T3> : IKuchenList
        where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private readonly (string Name, float Size)[] originalInfoCache;
        private List<UIFactory<T1, T2, T3>> contents = new List<UIFactory<T1, T2, T3>>();
        private readonly List<(float, float)> contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => createdObjects;
        public int ContentsCount => contents.Count;
        public ScrollRect ScrollRect => scrollRect;
        public RectTransform ContentRectTransform => scrollRect.content;

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public float NormalizedPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3)
        {
            this.scrollRect = scrollRect;

            originalInfoCache = new (string Name, float Size)[3];

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IMappedObject>());
            originalInfoCache[2] = (original3.Mapper.Get().name, original3.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                margin = new Margin
                {
                    Top = verticalLayoutGroup.padding.top,
                    Bottom = verticalLayoutGroup.padding.bottom
                };
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2, T3> list;

            public ListOperator(VerticalList<T1, T2, T3> list)
            {
                this.list = list;
            }

            public void DeactivateAll()
            {
                list.DeactivateAll();
            }

            public void UpdateView()
            {
                list.UpdateView();
            }
        }

        private void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }
            createdObjects.Clear();
        }

        private void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i].Item1) continue;
                if (contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = contentPositions.Count - 1;
                displayMaxIndex = contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
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
                IMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i].Item1);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3, contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = createdObjects.Values.Count == 0;
            foreach (var map in createdObjects.Values)
            {
                CollectObject(map);
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (additionalInfo != null && additionalInfo.specialSpacings != null)
                ? additionalInfo.specialSpacings
                : new SpecialSpacing[] { };
            for (var i = 0; i < contents.Count; ++i)
            {
                var content = contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = originalInfoCache[0].Name;
                    elementSize = originalInfoCache[0].Size;
                }
                if (content.Callback2 != null)
                {
                    elementName = originalInfoCache[1].Name;
                    elementSize = originalInfoCache[1].Size;
                }
                if (content.Callback3 != null)
                {
                    elementName = originalInfoCache[2].Name;
                    elementSize = originalInfoCache[2].Size;
                }
                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            target.Mapper.Get().SetActive(false);

            if (target is T1) cachedObjects[typeof(T1)].Add(target);
            if (target is T2) cachedObjects[typeof(T2)].Add(target);
            if (target is T3) cachedObjects[typeof(T3)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
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

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2, T3> Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IListContentEditor<T1, T2, T3>
        {
            private readonly VerticalList<T1, T2, T3> parent;
            public List<UIFactory<T1, T2, T3>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3> parent, EditMode editMode)
            {
                this.parent = parent;
                Contents = parent.contents;
                Spacing = parent.Spacing;
                Margin = parent.margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.Spacing = Spacing;
                parent.margin = Margin;
                parent.SpareElement = SpareElement;
                parent.UpdateListContents();
            }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }
                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
            var content = contentPositions[index];
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = - limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = - viewportHeight / 2f;
                var limitMin = - limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            scrollRect.velocity = Vector2.zero;
        }
    }

    public class VerticalList<T1, T2, T3, T4> : IKuchenList
        where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new() where T4 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private readonly T4 original4;
        private readonly (string Name, float Size)[] originalInfoCache;
        private List<UIFactory<T1, T2, T3, T4>> contents = new List<UIFactory<T1, T2, T3, T4>>();
        private readonly List<(float, float)> contentPositions = new List<(float, float)>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }
        public IReadOnlyDictionary<int, IMappedObject> CreatedObjects => createdObjects;
        public int ContentsCount => contents.Count;
        public ScrollRect ScrollRect => scrollRect;
        public RectTransform ContentRectTransform => scrollRect.content;

        private Margin margin = new Margin();
        public IReadonlyMargin Margin => margin;

        public float NormalizedPosition
        {
            get => scrollRect.verticalNormalizedPosition;
            set => scrollRect.verticalNormalizedPosition = value;
        }

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2, T3 original3, T4 original4)
        {
            this.scrollRect = scrollRect;

            originalInfoCache = new (string Name, float Size)[4];

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());
            originalInfoCache[0] = (original1.Mapper.Get().name, original1.Mapper.Get<RectTransform>().rect.height);

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());
            originalInfoCache[1] = (original2.Mapper.Get().name, original2.Mapper.Get<RectTransform>().rect.height);

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IMappedObject>());
            originalInfoCache[2] = (original3.Mapper.Get().name, original3.Mapper.Get<RectTransform>().rect.height);

            this.original4 = original4;
            this.original4.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T4), new List<IMappedObject>());
            originalInfoCache[3] = (original4.Mapper.Get().name, original4.Mapper.Get<RectTransform>().rect.height);

            var kuchenList = this.scrollRect.gameObject.AddComponent<KuchenList>();
            kuchenList.List = new ListOperator(this);

            var viewport = scrollRect.viewport;
            viewportRectTransformCache = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            additionalInfo = scrollRect.GetComponent<ListAdditionalInfo>();

            var verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                verticalLayoutGroup.enabled = false;
                Spacing = verticalLayoutGroup.spacing;
                margin = new Margin
                {
                    Top = verticalLayoutGroup.padding.top,
                    Bottom = verticalLayoutGroup.padding.bottom
                };
            }

            var contentSizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
            }
        }

        private class ListOperator : IKuchenListMonoBehaviourBridge
        {
            private readonly VerticalList<T1, T2, T3, T4> list;

            public ListOperator(VerticalList<T1, T2, T3, T4> list)
            {
                this.list = list;
            }

            public void DeactivateAll()
            {
                list.DeactivateAll();
            }

            public void UpdateView()
            {
                list.UpdateView();
            }
        }

        private void DeactivateAll()
        {
            foreach (var item in createdObjects.Values)
            {
                if (item is IReusableMappedObject reusable) reusable.Deactivate();
            }
            createdObjects.Clear();
        }

        private void UpdateView()
        {
            var displayRect = viewportRectTransformCache.rect;
            var contentRect = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRectTransformCache, scrollRect.content);
            var start = contentRect.max.y - displayRect.max.y;
            var displayRectHeight = displayRect.height;
            var end = start + displayRectHeight;

            var displayMinIndex = int.MaxValue;
            var displayMaxIndex = int.MinValue;
            for (var i = 0; i < contentPositions.Count; ++i)
            {
                if (start > contentPositions[i].Item1) continue;
                if (contentPositions[i].Item1 > end) break;
                displayMinIndex = Mathf.Min(displayMinIndex, i);
                displayMaxIndex = Mathf.Max(displayMaxIndex, i);
            }

            if (displayMinIndex == int.MaxValue)
            {
                displayMinIndex = contentPositions.Count - 1;
                displayMaxIndex = contentPositions.Count - 1;
            }

            displayMinIndex = Mathf.Max(displayMinIndex - 1 - SpareElement, 0);
            displayMaxIndex = Mathf.Min(displayMaxIndex + SpareElement, contents.Count - 1);

            var removedList = new List<int>();
            foreach (var tmp in createdObjects)
            {
                var index = tmp.Key;
                var map = tmp.Value;
                if (displayMinIndex <= index && index <= displayMaxIndex) continue;

                CollectObject(map);
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
                IMappedObject newMappedObject = null;
                var content = contents[i];
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i].Item1);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i].Item1);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3, contentPositions[i].Item1);
                if (content.Callback4 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original4, content.Callback4, contentPositions[i].Item1);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
            var isFirst = createdObjects.Values.Count == 0;
            foreach (var map in createdObjects.Values)
            {
                CollectObject(map);
            }
            createdObjects.Clear();
            contentPositions.Clear();

            // create elements
            var calcPosition = Margin.Top;
            var prevElementName = "";
            var elementName = "";
            var specialSpacings = (additionalInfo != null && additionalInfo.specialSpacings != null)
                ? additionalInfo.specialSpacings
                : new SpecialSpacing[] { };
            for (var i = 0; i < contents.Count; ++i)
            {
                var content = contents[i];
                var elementSize = 0f;

                if (content.Callback1 != null)
                {
                    elementName = originalInfoCache[0].Name;
                    elementSize = originalInfoCache[0].Size;
                }
                if (content.Callback2 != null)
                {
                    elementName = originalInfoCache[1].Name;
                    elementSize = originalInfoCache[1].Size;
                }
                if (content.Callback3 != null)
                {
                    elementName = originalInfoCache[2].Name;
                    elementSize = originalInfoCache[2].Size;
                }
                if (content.Callback4 != null)
                {
                    elementName = originalInfoCache[3].Name;
                    elementSize = originalInfoCache[3].Size;
                }
                if (content.Spacer != null)
                {
                    elementName = "";
                    elementSize = content.Spacer.Size;
                }

                float? spacing = null;
                var specialSpacing = specialSpacings.FirstOrDefault(x => x.item1 == prevElementName && x.item2 == elementName);
                if (specialSpacing != null) spacing = specialSpacing.spacing;
                if (spacing == null && i != 0) spacing = Spacing;

                calcPosition += spacing ?? 0f;
                contentPositions.Add((calcPosition, elementSize));
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);

            var anchoredPosition = c.anchoredPosition;
            if (isFirst)
            {
                var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
                if (c.pivot.y > 1f - 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, -scrollRectSizeDeltaY);
                if (c.pivot.y < 0f + 0.0001f) c.anchoredPosition = new Vector2(anchoredPosition.x, scrollRectSizeDeltaY);
                scrollRect.velocity = Vector2.zero;
            }
        }

        private void CollectObject(IMappedObject target)
        {
            if (target is IReusableMappedObject reusable) reusable.Deactivate();
            target.Mapper.Get().SetActive(false);

            if (target is T1) cachedObjects[typeof(T1)].Add(target);
            if (target is T2) cachedObjects[typeof(T2)].Add(target);
            if (target is T3) cachedObjects[typeof(T3)].Add(target);
            if (target is T4) cachedObjects[typeof(T4)].Add(target);
        }

        private (RectTransform, IMappedObject) GetOrCreateNewObject<T>(T original, Action<T> contentCallback, float position) where T : IMappedObject, new()
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

            var p = newRectTransform.anchoredPosition;
            var r = newRectTransform.rect;
            newRectTransform.anchoredPosition = new Vector3(p.x, scrollRect.content.sizeDelta.y / 2f - position - r.height / 2f, 0f);

            if (newObject is IReusableMappedObject reusable) reusable.Activate();
            contentCallback(newObject);

            return (newRectTransform, newObject);
        }

        public IListContentEditor<T1, T2, T3, T4> Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IListContentEditor<T1, T2, T3, T4>
        {
            private readonly VerticalList<T1, T2, T3, T4> parent;
            public List<UIFactory<T1, T2, T3, T4>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; }
            public int SpareElement { get; set; }

            public ListContentEditor(VerticalList<T1, T2, T3, T4> parent, EditMode editMode)
            {
                this.parent = parent;
                Contents = parent.contents;
                Spacing = parent.Spacing;
                Margin = parent.margin;
                SpareElement = parent.SpareElement;

                if (editMode == EditMode.Clear) Contents.Clear();
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.Spacing = Spacing;
                parent.margin = Margin;
                parent.SpareElement = SpareElement;
                parent.UpdateListContents();
            }
        }

        public void DestroyCachedGameObjects()
        {
            foreach (var cachedObject in cachedObjects)
            {
                foreach (var go in cachedObject.Value)
                {
                    Object.Destroy(go.Mapper.Get());
                }
                cachedObject.Value.Clear();
            }
        }

        public Vector2? CalcScrollPosition(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var c = scrollRect.content;
            var anchoredPosition = c.anchoredPosition;
            var scrollRectSizeDeltaY = scrollRect.GetComponent<RectTransform>().rect.y;
            var content = contentPositions[index];
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = viewportRectTransformCache.rect.height;
            if (viewportHeight > contentHeight) return null;

            if (c.pivot.y > 1f - 0.0001f)
            {
                var p = -scrollRectSizeDeltaY + content.Item1;
                var limitMin = viewportHeight / 2f;
                var limitMax = - limitMin + contentHeight;
                var top = Mathf.Clamp(p - Spacing - additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p - viewportHeight + content.Item2 + Spacing + additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current > top) return new Vector2(anchoredPosition.x, top);
                    else if (current < bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            if (c.pivot.y < 0f + 0.0001f)
            {
                var p = scrollRectSizeDeltaY - (contentHeight - content.Item1 - content.Item2);
                var limitMax = - viewportHeight / 2f;
                var limitMin = - limitMax - contentHeight;
                var top = Mathf.Clamp(p + Spacing + additionalSpacing, limitMin, limitMax);
                var bottom = Mathf.Clamp(p + viewportHeight - content.Item2 - Spacing - additionalSpacing, limitMin, limitMax);

                if (type == ScrollToType.Top) return new Vector2(anchoredPosition.x, top);
                else if (type == ScrollToType.Bottom) return new Vector2(anchoredPosition.x, bottom);
                else if (type == ScrollToType.Near)
                {
                    var current = c.anchoredPosition.y;
                    if (current < top) return new Vector2(anchoredPosition.x, top);
                    else if (current > bottom) return new Vector2(anchoredPosition.x, bottom);
                    return null;
                }
            }

            return null;
        }

        public void ScrollTo(int index, ScrollToType type = ScrollToType.Top, float additionalSpacing = 0f)
        {
            var scrollPosition = CalcScrollPosition(index, type, additionalSpacing);
            if (scrollPosition != null) ContentRectTransform.anchoredPosition = scrollPosition.Value;
            scrollRect.velocity = Vector2.zero;
        }
    }

}