using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnKuchen.UIMapper
{
    public class UICache : MonoBehaviour, IMapper
    {
        [SerializeField]
        public CachedObject[] Elements;

        private IMapper cachedMapper;

        public void CreateCache()
        {
            var elements = new List<CachedObject>();
            CreateCacheInternal(elements, transform, new List<ulong>());
            Elements = elements.ToArray();
        }

        private void CreateCacheInternal(List<CachedObject> elements, Transform t, List<ulong> basePath)
        {
            elements.Add(new CachedObject { GameObject = t.gameObject, Path = basePath.ToArray() });
            foreach (Transform child in t)
            {
                basePath.Insert(0, FastHash.CalculateHash(child.name));
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

        public T GetChild<T>(string rootObjectPath) where T : IDuplicatable, new()
        {
            if (cachedMapper == null) cachedMapper = new Mapper(Elements);
            return cachedMapper.GetChild<T>(rootObjectPath);
        }

        public CachedObject[] GetRawElements()
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
    public class CachedObject
    {
        public GameObject GameObject;
        public ulong[] Path;
    }
}
