using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.Editor
{
    [CreateAssetMenu(menuName = "AnKuchen/CopyTemplateSettings", fileName = "AnKuchenCopyTemplateSettings")]
    public class AnKuchenCopyTemplateSettings : ScriptableObject
    {
        public string ClassName => className;
        [SerializeField] private string className = DefaultClassName;
        public static readonly string DefaultClassName = "{0}UiElements";

        public string[] PickupComponentNames => pickupComponentNames;
        [SerializeField] private string[] pickupComponentNames = DefaultPickupComponentNames;
        public static readonly string[] DefaultPickupComponentNames = new[] { nameof(Button), nameof(InputField), nameof(Text) };
    }
}