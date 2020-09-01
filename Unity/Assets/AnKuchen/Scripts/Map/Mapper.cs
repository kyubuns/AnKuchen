using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AnKuchen.Map
{
    public class Mapper : IMapper
    {
        private CachedObject[] elements;

        public Mapper(CachedObject[] elements)
        {
            this.elements = elements;
        }

        public GameObject Get()
        {
            var target = GetInternal(new uint[] { });
            Assert.AreEqual(1, target.Length, $"Root object is not found");
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public T Get<T>() where T : Component
        {
            var target = GetInternal(new uint[] { });
            Assert.AreEqual(1, target.Length, $"Root object is not found");
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public GameObject Get(string objectPath)
        {
            return Get(ToHash(objectPath));
        }

        public GameObject Get(uint[] objectPath)
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length, $"{objectPath} is not found");
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
            return Get<T>(ToHash(objectPath));
        }

        public T Get<T>(uint[] objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length, $"{objectPath} is not found");
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            return GetChild<T>(ToHash(rootObjectPath));
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
            return GetMapper(ToHash(rootObjectPath));
        }

        public IMapper GetMapper(uint[] rootObjectPath)
        {
            var target = GetInternal(rootObjectPath);
            Assert.AreEqual(1, target.Length, $"{rootObjectPath} is not found");

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
            return new Mapper(result.ToArray());
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
            Array.Reverse(path);
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
