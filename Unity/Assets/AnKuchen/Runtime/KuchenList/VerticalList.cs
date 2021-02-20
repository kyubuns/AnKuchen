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
    public class VerticalList<T1>
        where T1 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private List<UIFactory<T1>> contents = new List<UIFactory<T1>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }

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

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());

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
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
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
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i]);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
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
                    elementName = original1.Mapper.Get().name;
                    elementSize = original1.Mapper.Get<RectTransform>().rect.height;
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
                contentPositions.Add(calcPosition);
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);
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

        public ListContentEditor Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IDisposable
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
    }

    public class VerticalList<T1, T2>
        where T1 : IMappedObject, new() where T2 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private List<UIFactory<T1, T2>> contents = new List<UIFactory<T1, T2>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }

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

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());

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
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
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
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i]);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i]);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
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
                    elementName = original1.Mapper.Get().name;
                    elementSize = original1.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback2 != null)
                {
                    elementName = original2.Mapper.Get().name;
                    elementSize = original2.Mapper.Get<RectTransform>().rect.height;
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
                contentPositions.Add(calcPosition);
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);
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

        public ListContentEditor Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IDisposable
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
    }

    public class VerticalList<T1, T2, T3>
        where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private List<UIFactory<T1, T2, T3>> contents = new List<UIFactory<T1, T2, T3>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }

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

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IMappedObject>());

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
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
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
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i]);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i]);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3, contentPositions[i]);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
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
                    elementName = original1.Mapper.Get().name;
                    elementSize = original1.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback2 != null)
                {
                    elementName = original2.Mapper.Get().name;
                    elementSize = original2.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback3 != null)
                {
                    elementName = original3.Mapper.Get().name;
                    elementSize = original3.Mapper.Get<RectTransform>().rect.height;
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
                contentPositions.Add(calcPosition);
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);
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

        public ListContentEditor Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IDisposable
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
    }

    public class VerticalList<T1, T2, T3, T4>
        where T1 : IMappedObject, new() where T2 : IMappedObject, new() where T3 : IMappedObject, new() where T4 : IMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private readonly T3 original3;
        private readonly T4 original4;
        private List<UIFactory<T1, T2, T3, T4>> contents = new List<UIFactory<T1, T2, T3, T4>>();
        private readonly List<float> contentPositions = new List<float>();
        private readonly Dictionary<int, IMappedObject> createdObjects = new Dictionary<int, IMappedObject>();
        private readonly Dictionary<Type, List<IMappedObject>> cachedObjects = new Dictionary<Type, List<IMappedObject>>();
        private readonly RectTransform viewportRectTransformCache;
        private readonly ListAdditionalInfo additionalInfo;
        public float Spacing { get; private set; }
        public int SpareElement { get; private set; }

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

            this.original1 = original1;
            this.original1.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T1), new List<IMappedObject>());

            this.original2 = original2;
            this.original2.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T2), new List<IMappedObject>());

            this.original3 = original3;
            this.original3.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T3), new List<IMappedObject>());

            this.original4 = original4;
            this.original4.Mapper.Get().SetActive(false);
            cachedObjects.Add(typeof(T4), new List<IMappedObject>());

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
                if (start > contentPositions[i]) continue;
                if (contentPositions[i] > end) break;
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
                if (content.Callback1 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original1, content.Callback1, contentPositions[i]);
                if (content.Callback2 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original2, content.Callback2, contentPositions[i]);
                if (content.Callback3 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original3, content.Callback3, contentPositions[i]);
                if (content.Callback4 != null) (newObject, newMappedObject) = GetOrCreateNewObject(original4, content.Callback4, contentPositions[i]);
                if (content.Spacer != null) continue;
                if (newObject == null) throw new Exception($"newObject == null");
                createdObjects[i] = newMappedObject;
            }
        }

        private void UpdateListContents()
        {
            // clear elements
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
                    elementName = original1.Mapper.Get().name;
                    elementSize = original1.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback2 != null)
                {
                    elementName = original2.Mapper.Get().name;
                    elementSize = original2.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback3 != null)
                {
                    elementName = original3.Mapper.Get().name;
                    elementSize = original3.Mapper.Get<RectTransform>().rect.height;
                }
                if (content.Callback4 != null)
                {
                    elementName = original4.Mapper.Get().name;
                    elementSize = original4.Mapper.Get<RectTransform>().rect.height;
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
                contentPositions.Add(calcPosition);
                calcPosition += elementSize;

                prevElementName = elementName;
            }
            calcPosition += Margin.Bottom;

            // calc content size
            var c = scrollRect.content;
            var s = c.sizeDelta;
            c.sizeDelta = new Vector2(s.x, calcPosition);
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

        public ListContentEditor Edit(EditMode editMode = EditMode.Clear)
        {
            return new ListContentEditor(this, editMode);
        }

        public class ListContentEditor : IDisposable
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
    }

}