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

        private bool SequenceEqual<T>(T[] a, T[] b) where T : IEquatable<T>
        {
            if (a.Length != b.Length) return false;

            for (var i = 0; i < a.Length; ++i)
            {
                if (!a[i].Equals(b[i])) return false;
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

            // var gameObjects = target.Select(x => x.GameObject).ToArray();
            var gameObjects = new GameObject[target.Length];
            for (var i = 0; i < target.Length; ++i)
            {
                gameObjects[i] = target[i].GameObject;
            }

            return gameObjects;
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

        public T Map<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            var hash = ToHash(rootObjectPath);
            try
            {
                return Map<T>(hash);
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

        public T Map<T>(uint[] rootObjectPath) where T : IMappedObject, new()
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

        [Obsolete("Use Map<T> instead")]
        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            return Map<T>(rootObjectPath);
        }

        [Obsolete("Use Map<T> instead")]
        public T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new()
        {
            return Map<T>(rootObjectPath);
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
            Array.Copy(target[0].Path, pathElements, pathElements.Length);
            Array.Reverse(pathElements);

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
                    // var path = e.Path.Take(e.Path.Length - pathElements.Length).ToArray();
                    var pathLength = e.Path.Length - pathElements.Length;
                    if (pathLength > 0)
                    {
                        var path = new uint[pathLength];
                        Array.Copy(e.Path, 0, path, 0, pathLength);
                        result.Add(new CachedObject { GameObject = e.GameObject, Path = path });
                    }
                    else
                    {
                        result.Add(new CachedObject { GameObject = e.GameObject, Path = new uint[] { } });
                    }

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

        private static uint[] ToHash(string stringPath)
        {
            if (string.IsNullOrEmpty(stringPath)) return new uint[] { };

            // var hashArray = stringPath.Split('/').Select(x => FastHash.CalculateHash(x)).ToArray();
            var segments = stringPath.Split('/');
            var hashArray = new uint[segments.Length];
            for (var i = 0; i < segments.Length; i++)
            {
                hashArray[i] = FastHash.CalculateHash(segments[i]);
            }

            return hashArray;
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
                var newPath = new uint[path.Length - 1];
                Array.Copy(path, 1, newPath, 0, path.Length - 1);
                path = newPath;
            }

            {
                // path = path.Reverse().ToArray();
                var reversedArray = new uint[path.Length];
                for (var i = 0; i < path.Length; i++)
                {
                    reversedArray[i] = path[path.Length - 1 - i];
                }
                path = reversedArray;
            }

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
