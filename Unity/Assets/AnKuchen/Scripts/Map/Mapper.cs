using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnKuchen.Map
{
    public class Mapper : IMapper
    {
        private GameObject root;
        private CachedObject[] elements;

        public Mapper(GameObject root, CachedObject[] elements)
        {
            this.root = root;
            this.elements = elements;
        }

        public GameObject Get()
        {
            return root;
        }

        public T Get<T>() where T : Component
        {
            return root.GetComponent<T>();
        }

        public GameObject Get(string objectPath)
        {
            var hash = ToHash(objectPath);
            try
            {
                return Get(hash);
            }
            catch (AnKuchenNotFoundException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotFoundException($"{objectPath} is not found", null);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotUniqueException($"{objectPath} is not unique", null);
                throw;
            }
        }

        public GameObject Get(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException($"{objectPath} is not found", objectPath);
            if (target.Length > 1) throw new AnKuchenNotUniqueException($"{objectPath} is not unique", objectPath);
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public GameObject[] GetAll(string objectPath)
        {
            return GetAll(ToHash(objectPath));
        }

        public GameObject[] GetAll(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            return target.Select(x => x.GameObject).ToArray();
        }

        public T Get<T>(string objectPath) where T : Component
        {
            var hash = ToHash(objectPath);
            try
            {
                return Get<T>(hash);
            }
            catch (AnKuchenNotFoundException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotFoundException($"{objectPath} is not found", null);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotUniqueException($"{objectPath} is not unique", null);
                throw;
            }
        }

        public T Get<T>(uint[] objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException($"{objectPath} is not found", objectPath);
            if (target.Length > 1) throw new AnKuchenNotUniqueException($"{objectPath} is not unique", objectPath);

            var component = target[0].GameObject.GetComponent<T>();
            if (component == null) throw new AnKuchenNotFoundException($"{objectPath}<{nameof(T)}> is not found", objectPath);
            return component;
        }

        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            var hash = ToHash(rootObjectPath);
            try
            {
                return GetChild<T>(hash);
            }
            catch (AnKuchenNotFoundException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotFoundException($"{rootObjectPath} is not found", null);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotUniqueException($"{rootObjectPath} is not unique", null);
                throw;
            }
        }

        public T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new()
        {
            var newMapper = GetMapper(rootObjectPath);
            var newObject = new T();
            newObject.Initialize(newMapper);
            return newObject;
        }

        public IMapper GetMapper(string rootObjectPath)
        {
            var hash = ToHash(rootObjectPath);
            try
            {
                return GetMapper(hash);
            }
            catch (AnKuchenNotFoundException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotFoundException($"{rootObjectPath} is not found", null);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.Path != null && e.Path.SequenceEqual(hash)) throw new AnKuchenNotUniqueException($"{rootObjectPath} is not unique", null);
                throw;
            }
        }

        public IMapper GetMapper(uint[] rootObjectPath)
        {
            var target = GetInternal(rootObjectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException($"{rootObjectPath} is not found", rootObjectPath);
            if (target.Length > 1) throw new AnKuchenNotUniqueException($"{rootObjectPath} is not unique", rootObjectPath);

            var pathElements = target[0].Path.Reverse().ToArray();
            var result = new List<CachedObject>();
            foreach (var e in elements)
            {
                if (e.Path.Length < pathElements.Length) continue;

                var pass = true;
                for (var i = 0; i < pathElements.Length; ++i)
                {
                    if (e.Path[e.Path.Length - i - 1] == pathElements[i]) continue;
                    pass = false;
                    break;
                }
                if (pass)
                {
                    result.Add(new CachedObject { GameObject = e.GameObject, Path = e.Path.Take(e.Path.Length - pathElements.Length).ToArray() });
                }
            }
            return new Mapper(target[0].GameObject, result.ToArray());
        }

        public CachedObject[] GetRawElements()
        {
            return elements;
        }

        public void Copy(IMapper other)
        {
            elements = other.GetRawElements();
        }

        private uint[] ToHash(string stringPath)
        {
            if (string.IsNullOrEmpty(stringPath)) return new uint[] { };
            return stringPath.Split('/').Select(x => FastHash.CalculateHash(x)).ToArray();
        }

        private static readonly uint CachedHashDot = FastHash.CalculateHash(".");

        private CachedObject[] GetInternal(uint[] path)
        {
            if (path.Length == 0)
            {
                foreach (var e in elements)
                {
                    if (e.Path.Length == 0) return new[] { e };
                }
                return Array.Empty<CachedObject>();
            }

            var result = new List<CachedObject>();
            var start = false;
            if (path[0] == CachedHashDot)
            {
                start = true;
                path = path.Skip(1).ToArray();
            }
            path = path.Reverse().ToArray();
            foreach (var e in elements)
            {
                if (e.Path.Length < path.Length) continue;
                if (start && e.Path.Length != path.Length) continue;

                var pass = true;
                for (var i = 0; i < path.Length; ++i)
                {
                    if (e.Path[i] == path[i]) continue;
                    pass = false;
                    break;
                }
                if (pass)
                {
                    result.Add(e);
                }
            }
            return result.ToArray();
        }
    }
}
