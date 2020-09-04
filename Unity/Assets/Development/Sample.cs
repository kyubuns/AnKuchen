﻿using System;
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
            using (var editor = Layouter.Edit(ui.HogeButton))
            {
                foreach (var a in new[] { "h1", "h2", "h3" })
                {
                    var button = editor.Create();
                    button.Text.text = a;
                }
            }

            using (var editor = Layouter.Edit(ui.HogeButton))
            {
                foreach (var a in new[] { "h4", "h5", "h6" })
                {
                    var button = editor.Create();
                    button.Text.text = a;
                }
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
