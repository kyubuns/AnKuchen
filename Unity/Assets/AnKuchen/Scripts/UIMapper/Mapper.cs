using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AnKuchen.UIMapper
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
            var target = GetInternal(null);
            Assert.AreEqual(1, target.Length, $"Root object is not found");
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public T Get<T>() where T : Component
        {
            var target = GetInternal(null);
            Assert.AreEqual(1, target.Length, $"Root object is not found");
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public GameObject Get(string objectPath)
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length, $"{objectPath} is not found");
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public GameObject[] GetAll(string objectPath)
        {
            var target = GetInternal(objectPath);
            return target.Select(x => x.GameObject).ToArray();
        }

        public T Get<T>(string objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length, $"{objectPath} is not found");
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public IMapper GetChild(string rootObjectPath)
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

        public T GetChild<T>(string rootObjectPath) where T : IMappedObject, new()
        {
            var newMapper = GetChild(rootObjectPath);
            var newObject = new T();
            newObject.Initialize(newMapper);
            return newObject;
        }

        public CachedObject[] GetRawElements()
        {
            return elements;
        }

        public void Copy(IMapper other)
        {
            elements = other.GetRawElements();
        }

        private CachedObject[] GetInternal(string stringPath)
        {
            if (string.IsNullOrWhiteSpace(stringPath))
            {
                foreach (var e in elements)
                {
                    if (e.Path.Length == 0) return new[] { e };
                }
                return Array.Empty<CachedObject>();
            }

            var start = false;
            if (stringPath.StartsWith("./", StringComparison.OrdinalIgnoreCase))
            {
                stringPath = stringPath.Remove(0, 2);
                start = true;
            }
            var pathElements = stringPath.Split('/').Select(x => FastHash.CalculateHash(x)).Reverse().ToArray();

            var result = new List<CachedObject>();
            foreach (var e in elements)
            {
                if (e.Path.Length < pathElements.Length) continue;
                if (start && e.Path.Length != pathElements.Length) continue;

                var pass = true;
                for (var i = 0; i < pathElements.Length; ++i)
                {
                    if (e.Path[i] == pathElements[i]) continue;
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
