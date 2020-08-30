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

public class SampleUIElements
{
    public GameObject Root { get; }
    public Button HogeButton { get; }
    public Text HogeButtonText { get; }
    public Button FugaButton { get; }
    public Text FugaButtonText { get; }

    public SampleUIElements(IMapper mapper)
    {
        Root = mapper.Get();
        HogeButton = mapper.Get<Button>("HogeButton");
        HogeButtonText = mapper.Get<Text>("HogeButton/Text");
        FugaButton = mapper.Get<Button>("FugaButton");
        FugaButtonText = mapper.Get<Text>("FugaButton/Text");
    }
}
}
