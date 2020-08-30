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
            var hogeButton = root.GetChild("HogeButton");
            hogeButton.Get<Text>("Text").text = "Hoge";

            var fugaButton = hogeButton.Clone();
            fugaButton.Get<Text>("Text").text = "Fuga";

            var piyoButton = hogeButton.Clone();
            piyoButton.Get<Text>("Text").text = "Piyo";
        }
    }
}
