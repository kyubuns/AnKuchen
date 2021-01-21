using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AnKuchen.Map;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnKuchen.Editor
{
    [CustomEditor(typeof(UICache))]
    public class UICacheEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Update"))
            {
                var uiCache = (UICache) target;
                CreateCacheAndMarkDirty(uiCache);
                Debug.Log("Updated!");
            }

            if (GUILayout.Button("Copy Template"))
            {
                var uiCache = (UICache) target;
                CreateCacheAndMarkDirty(uiCache);
                var stringElements = CreateStringCache(uiCache.Get<Transform>());
                EditorGUIUtility.systemCopyBuffer = GenerateTemplate(uiCache, stringElements);
                Debug.Log("Copied!");
            }

            base.OnInspectorGUI();
        }

        private const string MenuItemName = "GameObject/Copy AnKuchen Template";
        private const int MenuItemPriority = 48;

        [MenuItem(MenuItemName, true, MenuItemPriority)]
        public static bool CopyAnKuchenTemplateValidate()
        {
            if (Selection.activeGameObject == null) return false;
            return Selection.activeGameObject.GetComponentInParent<UICache>() != null;
        }

        [MenuItem(MenuItemName, false, MenuItemPriority)]
        public static void CopyAnKuchenTemplate()
        {
            var target = Selection.activeGameObject;
            var stringElements = CreateStringCache(target.transform);
            var parentUiCache = target.GetComponentInParent<UICache>();
            CreateCacheAndMarkDirty(parentUiCache);

            var rootObjectPath = parentUiCache.GetRawElements().First(x => x.GameObject == target).Path.Reverse().ToArray();
            var uiCache = parentUiCache.GetMapper(rootObjectPath);

            EditorGUIUtility.systemCopyBuffer = GenerateTemplate(uiCache, stringElements);
            Debug.Log("Copied!");
        }

        private static void CreateCacheAndMarkDirty(UICache uiCache)
        {
            var elements = uiCache.Elements ?? new CachedObject[] { };
            var prev = CalcHash(elements);
            uiCache.CreateCache();
            var now = CalcHash(uiCache.Elements);
            if (prev != now) MarkDirty();
        }

        private static uint CalcHash(CachedObject[] objects)
        {
            var l = new List<uint> { (uint) objects.Length };
            foreach (var a in objects)
            {
                l.Add((uint) a.GameObject.GetInstanceID());
                l.Add((uint) a.Path.Length);
                foreach (var b in a.Path)
                {
                    l.Add(b);
                }
            }
            return FastHash.CalculateHash(l.ToArray());
        }

        private static void MarkDirty()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private static string ToSafeVariableName(string originalName)
        {
            var name = Regex.Replace(originalName, @"[^\w_]", "", RegexOptions.None);
            if (Enumerable.Range(0, 10).Any(x => name.StartsWith(x.ToString()))) name = $"_{name}";
            return name;
        }

        private static string GenerateTemplate(IMapper uiCache, UIStringElement[] stringElements)
        {
            var targetTypes = new[] { typeof(Button), typeof(InputField), typeof(Text), typeof(Image) };

            var elements = new List<(string Name, string[] Path, string Type)>();
            foreach (var e in stringElements)
            {
                if (e.Path.Length == 0) continue;
                foreach (var targetType in targetTypes)
                {
                    if (e.GameObject.GetComponent(targetType) == null) continue;

                    var uniquePath = e.Path.Concat(new[] { "." }).ToArray();
                    while (uniquePath.Length > 1)
                    {
                        var next = uniquePath.Take(uniquePath.Length - 1).ToArray();
                        var findPath = string.Join("/", next.Reverse());
                        var count = uiCache.GetAll(findPath).Length;
                        if (count == 0) throw new Exception($"{findPath} is not found");
                        if (count != 1) break;
                        uniquePath = next;
                    }
                    elements.Add((ToSafeVariableName(string.Join("", uniquePath.Where(x => x != ".").Reverse())), uniquePath.Reverse().ToArray(), targetType.Name));
                    break;
                }
            }

            var className = $"{uiCache.Get().name}UiElements";
            // コード生成
            var text = $"public class {className} : IMappedObject\n";
            text += "{\n";
            {
                text += $"    public IMapper Mapper {{ get; private set; }}\n";
                text += $"    public GameObject Root {{ get; private set; }}\n";
                foreach (var (n, _, t) in elements)
                {
                    text += $"    public {t} {n} {{ get; private set; }}\n";
                }
                text += "\n";
                text += $"    public {className}() {{ }}\n";
                text += $"    public {className}(IMapper mapper) {{ Initialize(mapper); }}\n";
                text += "\n";
                text += "    public void Initialize(IMapper mapper)\n";
                text += "    {\n";
                {
                    text += $"        Mapper = mapper;\n";
                    text += $"        Root = mapper.Get();\n";
                    foreach (var (n, p, t) in elements)
                    {
                        if (t == "GameObject") text += $"        {n} = mapper.Get(\"{string.Join("/", p)}\");\n";
                        else text += $"        {n} = mapper.Get<{t}>(\"{string.Join("/", p)}\");\n";
                    }
                }
                text += "    }\n";
            }
            text += "}\n";
            return text;
        }

        private static UIStringElement[] CreateStringCache(Transform parent)
        {
            var elements = new List<UIStringElement>();
            CreateStringCacheInternal(elements, parent, new List<string>());
            return elements.ToArray();
        }

        private static void CreateStringCacheInternal(List<UIStringElement> elements, Transform t, List<string> basePath)
        {
            elements.Add(new UIStringElement { GameObject = t.gameObject, Path = basePath.ToArray() });
            foreach (Transform child in t)
            {
                basePath.Insert(0, child.name);
                CreateStringCacheInternal(elements, child, basePath);
                basePath.RemoveAt(0);
            }
        }

        private class UIStringElement
        {
            public GameObject GameObject;
            public string[] Path;
        }
    }
}
