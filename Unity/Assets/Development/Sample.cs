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
            var num = 3;
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

            yield return new WaitForSeconds(2.0f);

            ui.List.ScrollTo(5, ScrollToType.Center);
            ui.List2.ScrollTo(5, ScrollToType.Center);
            ui.ListH.ScrollTo(5, ScrollToType.Center);
            ui.ListH2.ScrollTo(5, ScrollToType.Center);

            using (var editor = ui.Layout.Edit())
            {
                var a = editor.Create();
                a.Text.text = "d";

                var b = editor.Create();
                b.Text.text = "e";

                var c = editor.Create();
                c.Text.text = "f";
            }

            yield return new WaitForSeconds(2.0f);

            ui.List.ScrollTo(6, ScrollToType.Near);
            ui.List2.ScrollTo(3, ScrollToType.Near);
            ui.ListH.ScrollTo(6, ScrollToType.Near);
            ui.ListH2.ScrollTo(3, ScrollToType.Near);

            using (var editor = ui.Layout.Edit(EditMode.DontClear))
            {
            }

            /*
            yield return new WaitForSeconds(2.0f);

            ui.List.ScrollTo(1, ScrollToType.Top);
            ui.List2.ScrollTo(8, ScrollToType.Top);
            ui.ListH.ScrollTo(1, ScrollToType.Top);
            ui.ListH2.ScrollTo(8, ScrollToType.Top);

            yield return new WaitForSeconds(2.0f);

            ui.List.ScrollTo(3, ScrollToType.Near);
            ui.List2.ScrollTo(6, ScrollToType.Near);
            ui.ListH.ScrollTo(3, ScrollToType.Near);
            ui.ListH2.ScrollTo(6, ScrollToType.Near);
            */

            /*
            ui.List.ScrollTo(1, ScrollToType.Top);
            ui.List2.ScrollTo(4, ScrollToType.Top);
            ui.ListH.ScrollTo(1, ScrollToType.Top);
            ui.ListH2.ScrollTo(4, ScrollToType.Top);
            */

            /*
            ui.List.ScrollTo(4, ScrollToType.Bottom, 0f);
            ui.List2.ScrollTo(1, ScrollToType.Bottom, 0f);
            ui.ListH.ScrollTo(4, ScrollToType.Bottom, 0f);
            ui.ListH2.ScrollTo(1, ScrollToType.Bottom, 0f);
            */

            /*
            yield return new WaitForSeconds(2.0f);

            ui.List.ScrollTo(5, ScrollToType.Top);
            ui.List2.ScrollTo(5, ScrollToType.Top);
            ui.ListH.ScrollTo(5, ScrollToType.Top);
            ui.ListH2.ScrollTo(5, ScrollToType.Top);
            */

            /*
            // ガタッってならないかチェック
            ui.List.ScrollTo(5, ScrollToType.Top);
            ui.List2.ScrollTo(0, ScrollToType.Top);
            ui.ListH.ScrollTo(5, ScrollToType.Top);
            ui.ListH2.ScrollTo(0, ScrollToType.Top);
            */
        }

        private void CreateList(UIElements ui, int num)
        {
            using (var editor = ui.List.Edit())
            {
                Hoge(editor, 2);
            }

            using (var editor = ui.List2.Edit())
            {
                Hoge(editor, 2);
            }

            using (var editor = ui.ListH.Edit())
            {
                Hoge(editor, 2);
            }

            using (var editor = ui.ListH2.Edit())
            {
                Hoge(editor, 2);
            }
        }

        private void Hoge(IListContentEditor<ListElements1, ListElements2> editor, int num)
        {
            for (var i = 0; i < 10; ++i)
            {
                var i1 = i;
                editor.Add(x => x.Text.text = $"No.{i1}");
            }
            for (var i = 0; i < num; ++i)
            {
                editor.Add(x => x.Image.color = Color.blue);
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
        public VerticalList<ListElements1, ListElements2> List { get; private set; }
        public VerticalList<ListElements1, ListElements2> List2 { get; private set; }
        public HorizontalList<ListElements1, ListElements2> ListH { get; private set; }
        public HorizontalList<ListElements1, ListElements2> ListH2 { get; private set; }
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

            var scrollRectH = mapper.Get<ScrollRect>("H_List");
            var contentH = mapper.Get<RectTransform>("H_List/Content");
            scrollRectH.content = contentH;
            ListH = new HorizontalList<ListElements1, ListElements2>(
                scrollRectH,
                mapper.GetChild<ListElements1>("H_List/Element1"),
                mapper.GetChild<ListElements2>("H_List/Element2")
            );

            var scrollRectH2 = mapper.Get<ScrollRect>("H_List2");
            var contentH2 = mapper.Get<RectTransform>("H_List2/Content");
            scrollRectH2.content = contentH2;
            ListH2 = new HorizontalList<ListElements1, ListElements2>(
                scrollRectH2,
                mapper.GetChild<ListElements1>("H_List2/Element1"),
                mapper.GetChild<ListElements2>("H_List2/Element2")
            );

            var scrollRect = mapper.Get<ScrollRect>("List");
            var content = mapper.Get<RectTransform>("List/Content");
            scrollRect.content = content;
            List = new VerticalList<ListElements1, ListElements2>(
                scrollRect,
                mapper.GetChild<ListElements1>("List/Element1"),
                mapper.GetChild<ListElements2>("List/Element2")
            );

            var scrollRect2 = mapper.Get<ScrollRect>("List2");
            var content2 = mapper.Get<RectTransform>("List2/Content");
            scrollRect2.content = content2;
            List2 = new VerticalList<ListElements1, ListElements2>(
                scrollRect2,
                mapper.GetChild<ListElements1>("List2/Element1"),
                mapper.GetChild<ListElements2>("List2/Element2")
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
