using AnKuchen.UIMapper;
using UnityEditor;
using UnityEngine;

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
                Debug.Log("Updated");
            }

            base.OnInspectorGUI();
        }
    }
}
