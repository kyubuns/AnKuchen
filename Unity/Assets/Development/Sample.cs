using System;
using System.Collections.Generic;
using System.Linq;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;
using AnKuchen.Extensions;
using Object = UnityEngine.Object;

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
        }

        public void Deactivate()
        {
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



namespace AnKuchen
{
    public interface IReusableMappedObject : IMappedObject
    {
        void Activate();
        void Deactivate();
    }

    public class UIFactory<T1, T2> where T1 : IMappedObject where T2 : IMappedObject
    {
        public Action<T1> Callback1 { get; }
        public Action<T2> Callback2 { get; }

        public UIFactory(Action<T1> callback1)
        {
            Callback1 = callback1;
        }

        public UIFactory(Action<T2> callback2)
        {
            Callback2 = callback2;
        }
    }

    public class VerticalList<T1, T2> : IDisposable where T1 : IReusableMappedObject, new() where T2 : IReusableMappedObject, new()
    {
        private readonly ScrollRect scrollRect;
        private readonly T1 original1;
        private readonly T2 original2;
        private List<UIFactory<T1, T2>> contents = new List<UIFactory<T1, T2>>();
        private readonly List<(GameObject, IReusableMappedObject)> items = new List<(GameObject, IReusableMappedObject)>();

        public VerticalList(ScrollRect scrollRect, T1 original1, T2 original2)
        {
            this.scrollRect = scrollRect;
            this.original1 = original1;
            this.original2 = original2;

            this.original1.Mapper.Get().SetActive(false);
            this.original2.Mapper.Get().SetActive(false);
        }

        public T1 Get1(int index)
        {
            return (T1) items[index].Item2;
        }

        public T2 Get2(int index)
        {
            return (T2) items[index].Item2;
        }

        private void ClearAll()
        {
            foreach (var item in items)
            {
                item.Item2.Deactivate();
                Object.Destroy(item.Item1);
            }

            items.Clear();
        }

        private void UpdateListContents()
        {
            ClearAll();

            foreach (var content in contents)
            {
                if (content.Callback1 != null) CreateNewObject(original1, content.Callback1);
                if (content.Callback2 != null) CreateNewObject(original2, content.Callback2);
            }
        }

        private void CreateNewObject<T>(T original, Action<T> contentCallback) where T : IReusableMappedObject, new()
        {
            var newObject = original.Duplicate();
            newObject.Mapper.Get().SetActive(true);
            newObject.Activate();
            contentCallback(newObject);
            items.Add((newObject.Mapper.Get(), newObject));
        }

        public ListContentEditor Edit()
        {
            return new ListContentEditor(this);
        }

        public class ListContentEditor : IDisposable
        {
            private readonly VerticalList<T1, T2> parent;
            public List<UIFactory<T1, T2>> Contents { get; set; }
            public float Spacing { get; set; }
            public Margin Margin { get; set; } = new Margin();

            public ListContentEditor(VerticalList<T1, T2> parent)
            {
                this.parent = parent;
                Contents = parent.contents;
            }

            public void Dispose()
            {
                parent.contents = Contents;
                parent.UpdateListContents();
            }
        }

        public void Dispose()
        {
            ClearAll();
        }
    }

    public class Margin
    {
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
    }
}
