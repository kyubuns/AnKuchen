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
            var ui = new UIElements(root);

            using (var editor = Layouter.TopToBottom(ui.HogeButton))
            {
                foreach (var buttonLabel in new[] { "Hoge", "Fuga", "Piyo" })
                {
                    var newObject = editor.Create();
                    newObject.Text.text = buttonLabel;
                }
            }
        }
    }

    public class UIElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public ButtonElements HogeButton { get; private set; }

        public UIElements(IMapper mapper)
        {
            Initialize(mapper);
        }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            HogeButton = mapper.GetChild<ButtonElements>("HogeButton");
        }
    }

    public class ButtonElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public Text Text { get; private set; }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Text = mapper.Get<Text>("Text");
        }
    }
}
