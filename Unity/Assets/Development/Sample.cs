using AnKuchen.UILayouter;
using AnKuchen.UIMapper;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.Sample
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private UICache root = default;

        public void Start()
        {
            var original = root.GetChild("HogeButton");
            using (var layouter = Layouter.TopToBottom(original))
            {
                foreach (var button in new[] { "Hoge", "Fuga", "Piyo" })
                {
                    var newButton = layouter.Create();
                    newButton.Get<Text>("Text").text = button;
                }
            }
        }
    }
}
