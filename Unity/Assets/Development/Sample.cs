using System.Collections;
using System.Collections.Generic;
using AnKuchen.KuchenLayout;
using AnKuchen.KuchenLayout.Layouter;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;
using AnKuchen.KuchenList;

namespace AnKuchen.Development
{
    public class Sample : MonoBehaviour
    {
        [SerializeField] private UICache root = default;

        public IEnumerator Start()
        {
            yield return new WaitForSeconds(1.0f);

            var ui = new UIElements(root);
            var num = 10;
            ui.HogeButton.onClick.AddListener(() =>
            {
                num++;
                CreateList(ui, num);
            });
            ui.SomeButton.onClick.AddListener(() =>
            {
                num--;
                if (num < 0) num = 0;
                CreateList(ui, num);
            });
            ui.DeleteAllButton.onClick.AddListener(() =>
            {
                ui.List.DestroyCachedGameObjects();
            });
            CreateList(ui, num);

            using (var editor = ui.Layout.Edit())
            {
                var a = editor.Create();
                a.Text.text = "a";

                var b = editor.Create();
                b.Text.text = "b";

                var c = editor.Create();
                c.Text.text = "c";
            }

            using (var editor = ui.Layout.Edit(EditMode.DontClear))
            {
                var a = editor.Create();
                a.Text.text = "d";

                var b = editor.Create();
                b.Text.text = "e";

                var c = editor.Create();
                c.Text.text = "f";
            }

            ui.Layout.Elements[0].Text.text = "0";
        }

        private void CreateList(UIElements ui, int num)
        {
            using (var editor = ui.List.Edit())
            {
                // editor.Spacing = 10f;
                // editor.Margin.TopBottom = 10f;
                // SpacingはContentのLayoutGroupから自動的に取得される

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
                };
                for (var i = 0; i < num; ++i)
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
        public Button DeleteAllButton { get; private set; }
        public HorizontalList<ListElements1, ListElements2> List { get; private set; }
        public Layout<LayoutItem> Layout { get; private set; }

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
            DeleteAllButton = mapper.Get<Button>("DeleteAllButton");

            var scrollRect = mapper.Get<ScrollRect>("H_List");
            var content = mapper.Get<RectTransform>("H_List/Content");
            scrollRect.content = content;
            List = new HorizontalList<ListElements1, ListElements2>(
                scrollRect,
                mapper.GetChild<ListElements1>("H_List/Element1"),
                mapper.GetChild<ListElements2>("H_List/Element2")
            );
            Layout = new Layout<LayoutItem>(
                mapper.GetChild<LayoutItem>("LayoutGroup/Item"),
                new TopToBottomLayouter(10f)
            );
        }
    }

    public class ListElements1 : IMappedObject
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

    public class LayoutItem : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public Text Text { get; private set; }

        public LayoutItem()
        {
        }

        public void Initialize(IMapper mapper)
        {
            Mapper = mapper;
            Text = mapper.Get<Text>("Text");
        }
    }
}
