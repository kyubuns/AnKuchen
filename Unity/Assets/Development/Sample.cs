using System;
using System.Collections.Generic;
using AnKuchen.Extensions;
using AnKuchen.Layout;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.Sample
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private UICache root = default;

        public void Start()
        {
            root.SetText(new Dictionary<string, string>
            {
                { "./Text", "Title" },
                { "HogeButton/Text", "Hoge" },
                { "FugaButton/Text", "Fuga" },
                { "PiyoButton/Text", "Piyo" },
            });

            root.Batch(new Dictionary<string, Action<Text>>
            {
                { "./Text", x => x.text = "Title" },
                { "HogeButton/Text", x => x.text = "Hoge" },
                { "FugaButton/Text", x => x.text = "Fuga" },
                { "PiyoButton/Text", x => x.text = "Piyo" },
            });

            var ui = new UIElements(root);
            using (var editor = Layouter.LeftToRight(ui.HogeButton, 10f))
            {
                foreach (var a in new[] { "h1", "h2", "h3" })
                {
                    var button = editor.Create();
                    button.Text.text = a;
                }
                editor.Elements[1].Root.SetActive(false);
            }
        }
    }

    public class UIElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Text Text { get; private set; }
        public ButtonElements HogeButton { get; private set; }

        public UIElements(IMapper mapper)
        {
            Initialize(mapper);
        }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Text = mapper.Get<Text>("./Text");
            HogeButton = mapper.GetChild<ButtonElements>("HogeButton");
        }
    }


    public class ButtonElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Button Button { get; private set; }
        public Text Text { get; private set; }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Button = mapper.Get<Button>();
            Text = mapper.Get<Text>("Text");
        }
    }
}
