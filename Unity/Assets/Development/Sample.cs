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
            var hogeButton = root.GetChild("HogeButton");
            hogeButton.Get<Text>("Text").text = "Hoge";

            var fugaButton = hogeButton.Duplicate();
            fugaButton.Get<Text>("Text").text = "Fuga";

            var piyoButton = hogeButton.Duplicate();
            piyoButton.Get<Text>("Text").text = "Piyo";
        }
    }

    public class UIElements
    {
        public GameObject Root { get; }
        public Button HogeButton { get; }
        public Text HogeButtonText { get; }
        public Button FugaButton { get; }
        public Text FugaButtonText { get; }

        public UIElements(IMapper mapper)
        {
            Root = mapper.Get();
            HogeButton = mapper.Get<Button>("HogeButton");
            HogeButtonText = mapper.Get<Text>("HogeButton/Text");
            FugaButton = mapper.Get<Button>("FugaButton");
            FugaButtonText = mapper.Get<Text>("FugaButton/Text");
        }
    }
}

