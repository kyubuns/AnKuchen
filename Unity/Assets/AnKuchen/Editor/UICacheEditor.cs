using System.Collections.Generic;
using System.Linq;
using AnKuchen.UIMapper;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.Editor.UIMapper
{
    [CustomEditor(typeof(UICache))]
    public class UICacheEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Update"))
            {
                var uiCache = (UICache) target;
                uiCache.CreateCache();
                Debug.Log("Updated!");
            }

            if (GUILayout.Button("Copy Template"))
            {
                var uiCache = (UICache) target;
                uiCache.CreateCache();
                EditorGUIUtility.systemCopyBuffer = GenerateTemplate(uiCache);
                Debug.Log("Copied!");
            }

            base.OnInspectorGUI();
        }

        private string GenerateTemplate(UICache uiCache)
        {
            var targetTypes = new[] { typeof(Button), typeof(Text), typeof(Image) };

            var elements = new List<(string[] Path, string Type)>();
            foreach (var e in uiCache.Elements)
            {
                if (e.Path.Length == 0) continue;
                foreach (var targetType in targetTypes)
                {
                    if (e.GameObject.GetComponent(targetType) == null) continue;

                    var uniquePath = e.Path.ToArray();
                    while (uniquePath.Length > 1)
                    {
                        var next = uniquePath.Take(uniquePath.Length - 1).ToArray();
                        if (uiCache.GetAll(string.Join("/", next.Reverse())).Length != 1) break;
                        uniquePath = next;
                    }
                    elements.Add((uniquePath.Reverse().ToArray(), targetType.Name));
                    break;
                }
            }

            // コード生成
            var text = "public class UIElements\n";
            text += "{\n";
            {
                text += $"    public GameObject Root {{ get; }}\n";
                foreach (var (p, t) in elements)
                {
                    text += $"    public {t} {string.Join("", p)} {{ get; }}\n";
                }
                text += "\n";
                text += "    public UIElements(IMapper mapper)\n";
                text += "    {\n";
                {
                    text += $"        Root = mapper.Get();\n";
                    foreach (var (p, t) in elements)
                    {
                        if (t == "GameObject") text += $"        {string.Join("", p)} = mapper.Get(\"{string.Join("/", p)}\");\n";
                        else text += $"        {string.Join("", p)} = mapper.Get<{t}>(\"{string.Join("/", p)}\");\n";
                    }
                }
                text += "    }\n";
            }
            text += "}\n";
            return text;
        }
    }
}
