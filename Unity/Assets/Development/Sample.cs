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
            root.Get("Text").SetActive(false);
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

