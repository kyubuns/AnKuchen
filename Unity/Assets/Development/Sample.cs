using System;
using System.Collections.Generic;
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
            var ui = new OriginalUIElements(root);

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
        }
    }

    public class OriginalUIElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Text Text { get; private set; }
        public Button HogeButton { get; private set; }
        public Text HogeButtonText { get; private set; }
        public Button FugaButton { get; private set; }
        public Text FugaButtonText { get; private set; }
        public Button PiyoButton { get; private set; }
        public Text PiyoButtonText { get; private set; }

        public OriginalUIElements(IMapper mapper)
        {
            Initialize(mapper);
        }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Text = mapper.Get<Text>("./Text");
            HogeButton = mapper.Get<Button>(new uint[] { /* HogeButton */ 123, /* Text */ 234 });
            HogeButtonText = mapper.Get<Text>("HogeButton/Text");
            FugaButton = mapper.Get<Button>("FugaButton");
            FugaButtonText = mapper.Get<Text>("FugaButton/Text");
            PiyoButton = mapper.Get<Button>("PiyoButton");
            PiyoButtonText = mapper.Get<Text>("PiyoButton/Text");
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
        public Button Button { get; private set; }
        public Text Text { get; private set; }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Button = mapper.Get<Button>();
            Text = mapper.Get<Text>("Text");
        }
    }
}
