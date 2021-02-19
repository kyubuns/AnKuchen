using System;
using System.Collections.Generic;
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

        private static bool SequenceEqual(uint[] a, uint[] b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
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
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotFoundException(objectPath, e.Type);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotUniqueException(objectPath, e.Type);
                throw;
            }
        }

        public GameObject Get(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException(objectPath, null);
            if (target.Length > 1) throw new AnKuchenNotUniqueException(objectPath, null);
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public GameObject[] GetAll(string objectPath)
        {
            return GetAll(ToHash(objectPath));
        }

        public GameObject[] GetAll(uint[] objectPath)
        {
            var target = GetInternal(objectPath);

            var a = new GameObject[target.Length];
            for (var i = 0; i < a.Length; ++i) a[i] = target[i].GameObject;
            return a;
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
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotFoundException(objectPath, e.Type);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotUniqueException(objectPath, e.Type);
                throw;
            }
        }

        public T Get<T>(uint[] objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException(objectPath, typeof(T));
            if (target.Length > 1) throw new AnKuchenNotUniqueException(objectPath, typeof(T));

            var component = target[0].GameObject.GetComponent<T>();
            if (component == null) throw new AnKuchenNotFoundException(objectPath, typeof(T));
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
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotFoundException(rootObjectPath, e.Type);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotUniqueException(rootObjectPath, e.Type);
                throw;
            }
        }

        public T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new()
        {
            IMapper newMapper;
            try
            {
                newMapper = GetMapper(rootObjectPath);
            }
            catch (AnKuchenNotFoundException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, rootObjectPath)) throw new AnKuchenNotFoundException(rootObjectPath, typeof(T));
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, rootObjectPath)) throw new AnKuchenNotUniqueException(rootObjectPath, typeof(T));
                throw;
            }

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
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotFoundException(rootObjectPath, e.Type);
                throw;
            }
            catch (AnKuchenNotUniqueException e)
            {
                if (e.PathHash != null && SequenceEqual(e.PathHash, hash)) throw new AnKuchenNotUniqueException(rootObjectPath, e.Type);
                throw;
            }
        }

        public IMapper GetMapper(uint[] rootObjectPath)
        {
            var target = GetInternal(rootObjectPath);
            if (target.Length == 0) throw new AnKuchenNotFoundException(rootObjectPath, null);
            if (target.Length > 1) throw new AnKuchenNotUniqueException(rootObjectPath, null);

            // var pathElements = target[0].Path.Reverse().ToArray();
            var pathElements = new uint[target[0].Path.Length];
            for (var i = 0; i < pathElements.Length; ++i) pathElements[i] = target[0].Path[target[0].Path.Length - i - 1];

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
                    var take = new uint[e.Path.Length - pathElements.Length];
                    for (var i = 0; i < take.Length; ++i) take[i] = e.Path[i];
                    result.Add(new CachedObject { GameObject = e.GameObject, Path = take });
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

            // return stringPath.Split('/').Select(x => FastHash.CalculateHash(x)).ToArray();
            var iMax = 1;
            for (var i = 0; i < stringPath.Length; ++i)
            {
                if (stringPath[i] == '/')
                {
                    iMax++;
                }
            }
            var result = new uint[iMax];
            for (var i = 0; i < result.Length; ++i) result[i] = FastHash.CalculateHash(stringPath, i);
            return result;
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

                // path = path.Skip(1).ToArray();
                var tmp = path;
                path = new uint[path.Length - 1];
                for (var i = 0; i < path.Length; ++i) path[i] = tmp[i + 1];
            }

            // path = path.Reverse().ToArray();
            var tmp2 = path;
            path = new uint[tmp2.Length];
            for (var i = 0; i < path.Length; ++i) path[i] = tmp2[tmp2.Length - i - 1];

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
