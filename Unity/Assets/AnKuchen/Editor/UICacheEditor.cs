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

            var (body, templateName) = GenerateTemplate(uiCache, stringElements);
            EditorGUIUtility.systemCopyBuffer = body;
            Debug.Log($"Copied! ({templateName})");
        }

        private static void CreateCacheAndMarkDirty(UICache uiCache)
        {
            var elements = uiCache.Elements ?? new CachedObject[] { };
            uiCache.CreateCache();
            if (!CacheEquals(elements, uiCache.Elements))
            {
                MarkDirty();
            }
        }

        private static bool CacheEquals(CachedObject[] previous, CachedObject[] current)
        {
            if (previous.Length != current.Length)
            {
                return false;
            }

            for (var i = 0; i < previous.Length; i++)
            {
                // EntityIdを32bitのハッシュに縮めず、オブジェクトの同一性を比較する。
#if UNITY_6000_7_OR_NEWER
                var sameObject = previous[i].GameObject.GetEntityId() == current[i].GameObject.GetEntityId();
#else
                var sameObject = previous[i].GameObject.GetInstanceID() == current[i].GameObject.GetInstanceID();
#endif
                if (!sameObject || !previous[i].Path.SequenceEqual(current[i].Path))
                {
                    return false;
                }
            }
            return true;
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

        private static (string Body, string TemplateName) GenerateTemplate(IMapper uiCache, UIStringElement[] stringElements)
        {
            var templateName = "DefaultTemplate";
            var classNameTemplate = AnKuchenCopyTemplateSettings.DefaultClassName;
            var targetTypesTemplate = AnKuchenCopyTemplateSettings.DefaultPickupComponentNames;
            var removeText = AnKuchenCopyTemplateSettings.DefaultRemoveText;

            var guids = AssetDatabase.FindAssets($"t:{nameof(AnKuchenCopyTemplateSettings)}");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var settings = AssetDatabase.LoadAssetAtPath<AnKuchenCopyTemplateSettings>(path);

                templateName = path;
                classNameTemplate = settings.ClassName ?? AnKuchenCopyTemplateSettings.DefaultClassName;
                targetTypesTemplate = settings.PickupComponentNames ?? new string[] { };
                removeText = settings.RemoveText ?? new string[] { };
            }

            var className = string.Format(classNameTemplate, ToSafeVariableName(uiCache.Get().name));
            var targetTypes = targetTypesTemplate;

            var elements = new List<(string Name, string[] Path, string Type)>();
            foreach (var e in stringElements)
            {
                if (e.Path.Length == 0) continue;
                foreach (var component in e.GameObject.GetComponents<Component>())
                {
                    var names = GetAllClassNames(component.GetType());
                    if (names.All(name => !targetTypes.Contains(name))) continue;

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

                    var safeVariableName = ToSafeVariableName(string.Join("", uniquePath.Where(x => x != ".").Reverse()));
                    foreach (var r in removeText)
                    {
                        var before = safeVariableName;
                        safeVariableName = safeVariableName.Replace(r, "");
                        if (string.IsNullOrWhiteSpace(safeVariableName)) safeVariableName = before;
                    }
                    elements.Add((safeVariableName, uniquePath.Reverse().ToArray(), component.GetType().Name));
                    break;
                }
            }

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
            return (text, templateName);
        }

        private static readonly Dictionary<Type, string[]> ClassNamesCache = new Dictionary<Type, string[]>();
        private static string[] GetAllClassNames(Type type)
        {
            if (!ClassNamesCache.ContainsKey(type))
            {
                var result = new HashSet<string>();
                result.Add(type.Name);

                void GetInterface(Type t)
                {
                    foreach (var i in t.GetInterfaces())
                    {
                        result.Add(i.Name);
                        GetInterface(i);
                    }
                }

                void GetBase(Type t)
                {
                    if (t.BaseType != null)
                    {
                        result.Add(t.BaseType.Name);
                        GetInterface(t.BaseType);
                    }
                }

                GetInterface(type);
                GetBase(type);
                ClassNamesCache[type] = result.ToArray();
            }

            return ClassNamesCache[type];
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
