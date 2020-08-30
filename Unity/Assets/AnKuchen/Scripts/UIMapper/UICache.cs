using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnKuchen.UIMapper
{
    public class UICache : MonoBehaviour, IMapper
    {
        [SerializeField]
        public UIElement[] Elements;

        private IMapper cachedMapper;

        public void CreateCache()
        {
            var elements = new List<UIElement>();
            CreateCacheInternal(elements, transform, new List<string>());
            Elements = elements.ToArray();
        }

        private void CreateCacheInternal(List<UIElement> elements, Transform t, List<string> basePath)
        {
            elements.Add(new UIElement { GameObject = t.gameObject, Path = basePath.ToArray() });
            foreach (Transform child in t)
            {
                basePath.Insert(0, child.name);
                CreateCacheInternal(elements, child, basePath);
                basePath.RemoveAt(0);
            }
        }

        public GameObject Get()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.Get();
        }

        public T Get<T>() where T : Component
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.Get<T>();
        }

        public GameObject Get(string objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.Get(objectPath);
        }

        public GameObject[] GetAll(string objectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.GetAll(objectPath);
        }

        public T Get<T>(string objectPath) where T : Component
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.Get<T>(objectPath);
        }

        public IMapper GetChild(string rootObjectPath)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.GetChild(rootObjectPath);
        }

        public UIElement[] GetRawElements()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.GetRawElements();
        }

        public void Copy(IMapper other)
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            cachedMapper.Copy(other);
            Elements = cachedMapper.GetRawElements();
        }
    }

    [Serializable]
    public class UIElement
    {
        public GameObject GameObject;
        public string[] Path;
    }
}
