using System;
using System.Collections.Generic;
using AnKuchen.Extensions;
using AnKuchen.Layout;
using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;

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
        }
    }

    /*
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
    */

    public class UIElements : IMappedObject
    {
        public IMapper Mapper { get; private set; }
        public GameObject Root { get; private set; }
        public Text Text { get; private set; }
        public Button HogeButton { get; private set; }
        public Text HogeButtonText { get; private set; }
        public Button SomeButton { get; private set; }
        public Text SomeButtonText { get; private set; }

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
        }
    }

}
