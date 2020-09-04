using System;
using AnKuchen.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public static class TestUtils
    {
        public static UICache Instantiate(GameObject prefab)
        {
            var newObject = Object.Instantiate(prefab);
            return newObject.GetComponent<UICache>();
        }
    }
}
