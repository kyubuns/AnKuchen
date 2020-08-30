using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace AnKuchen.UIMapper
{
    public class Mapper : IMapper
    {
        private readonly UIElement[] elements;

        public Mapper(UIElement[] elements)
        {
            this.elements = elements;
        }

        public GameObject Get()
        {
            var target = GetInternal(null);
            Assert.AreEqual(1, target.Length);
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public T Get<T>() where T : Component
        {
            var target = GetInternal(null);
            Assert.AreEqual(1, target.Length);
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public GameObject Get(string objectPath)
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length);
            return target.Length > 0 ? target[0].GameObject : null;
        }

        public T Get<T>(string objectPath) where T : Component
        {
            var target = GetInternal(objectPath);
            Assert.AreEqual(1, target.Length);
            return target.Length > 0 ? target[0].GameObject.GetComponent<T>() : null;
        }

        public IMapper GetChild(string rootObjectPath)
        {
            throw new NotImplementedException();
        }

        private UIElement[] GetInternal(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                foreach (var e in elements)
                {
                    if (e.Path.Length == 0) return new[] { e };
                }
                return null;
            }

            var pathElements = path.Split('/');
            Array.Reverse(pathElements);

            var result = new List<UIElement>();
            foreach (var e in elements)
            {
                if (e.Path.Length < pathElements.Length) continue;

                var pass = true;
                for (var i = 0; i < pathElements.Length; ++i)
                {
                    if (e.Path[i].Equals(pathElements[i], StringComparison.OrdinalIgnoreCase)) continue;
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
