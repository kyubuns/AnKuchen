using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AnKuchen.UIMapper
{
    public class Mapper : IMapper
    {
        private UIElement[] elements;

        public Mapper(UIElement[] elements)
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
            var result = new List<UIElement>();
            foreach (var e in elements)
            {
                if (e.Path.Length < pathElements.Length) continue;

                var pass = true;
                for (var i = 0; i < pathElements.Length; ++i)
                {
                    if (e.Path[e.Path.Length - i - 1].Equals(pathElements[i], StringComparison.OrdinalIgnoreCase)) continue;
                    pass = false;
                    break;
                }
                if (pass)
                {
                    result.Add(new UIElement { GameObject = e.GameObject, Path = e.Path.Take(e.Path.Length - pathElements.Length).ToArray() });
                }
            }
            return new Mapper(result.ToArray());
        }

        public UIElement[] GetRawElements()
        {
            return elements;
        }

        public void Copy(IMapper other)
        {
            elements = other.GetRawElements();
        }

        private UIElement[] GetInternal(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                foreach (var e in elements)
                {
                    if (e.Path.Length == 0) return new[] { e };
                }
                return Array.Empty<UIElement>();
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
