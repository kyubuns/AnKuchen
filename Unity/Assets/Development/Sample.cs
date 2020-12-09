using System.Collections.Generic;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;
using AnKuchen.KuchenList;

namespace AnKuchen.Development
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private UICache root = default;

        public void Start()
        {
            var ui = new UIElements(root);
            ui.HogeButton.onClick.AddListener(() => Debug.Log("Hoge"));
            ui.SomeButton.onClick.AddListener(() => Debug.Log("Some"));

            // List
            using (var editor = ui.List.Edit())
            {
                editor.Spacing = 10f;
                editor.Margin.Top = 10f;

                editor.Contents = new List<UIFactory<ListElements1, ListElements2>>
                {
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Text.text = "No.1";
                    }),
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Text.text = "No.2";
                    }),
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Text.text = "No.3";
                    }),
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Image.color = Color.red;
                        x.Button.onClick.AddListener(() => Debug.Log("Click Red"));
                    }),
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Image.color = Color.green;
                        x.Button.onClick.AddListener(() => Debug.Log("Click Green"));
                    }),
                    new UIFactory<ListElements1, ListElements2>(x =>
                    {
                        x.Image.color = Color.blue;
                        x.Button.onClick.AddListener(() => Debug.Log("Click Blue"));
                    }),
                };

                for (var i = 0; i < 10; ++i)
                {
                    var i1 = i;
                    editor.Contents.Add(new UIFactory<ListElements1, ListElements2>(x => x.Text.text = $"Test {i1}"));
                }
            }
        }
    }

    public class UIElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Text Text { get; private set; }
        public Button HogeButton { get; private set; }
        public Text HogeButtonText { get; private set; }
        public Button SomeButton { get; private set; }
        public Text SomeButtonText { get; private set; }
        public VerticalList<ListElements1, ListElements2> List { get; private set; }

        public UIElements(IMapper mapper)
        {
            Initialize(mapper);
        }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Text = mapper.Get<Text>("./Text");
            HogeButton = mapper.Get<Button>("HogeButton");
            HogeButtonText = mapper.Get<Text>("HogeButton/Text");
            SomeButton = mapper.Get<Button>("Some Button");
            SomeButtonText = mapper.Get<Text>("Some Button/Text");
            List = new VerticalList<ListElements1, ListElements2>(
                mapper.Get<ScrollRect>("List"),
                mapper.GetChild<ListElements1>("Element1"),
                mapper.GetChild<ListElements2>("Element2")
            );
        }
    }

    public class ListElements1 : IReusableMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Text Text { get; private set; }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Text = mapper.Get<Text>("./Text");
        }

        public void Activate()
        {
            Debug.Log("Activate");
        }

        public void Deactivate()
        {
            Debug.Log("Deactivate");
        }
    }

    public class ListElements2 : IReusableMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Image Image { get; private set; }
        public Button Button { get; private set; }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Root = mapper.Get();
            Image = mapper.Get<Image>("./Image");
            Button = mapper.Get<Button>("./Button");
        }

        public void Activate()
        {
        }

        public void Deactivate()
        {
            Button.onClick.RemoveAllListeners();
        }
    }
}
