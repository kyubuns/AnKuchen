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
            var fugaButton = root.GetChild("FugaButton");
            var piyoButton = root.GetChild("PiyoButton");

            SetButtonText(hogeButton, "Hoge");
            SetButtonText(fugaButton, "Fuga");
            SetButtonText(piyoButton, "Piyo");
        }

        private void SetButtonText(IMapper button, string text)
        {
            button.Get<Text>("Text").text = text;
        }
    }
}
