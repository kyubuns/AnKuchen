using AnKuchen.Editor;
using AnKuchen.Map;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace Tests
{
    public class PrefabTests
    {
        [Test]
        public void UI要素が全てあるかのテスト()
        {
            var test1Object = Resources.Load<GameObject>("Test1");
            Assert.DoesNotThrow(() => UIElementTester.Test<UIElements>(test1Object));
        }

        [Test]
        public void UI要素が全てあるかのテスト無ければコケる()
        {
            var test1Object = Resources.Load<GameObject>("Test1");
            Assert.Throws<AssertionException>(() => UIElementTester.Test<UIElementsDummy>(test1Object));
        }

        public class UIElements : IMappedObject
        {
            public IMapper Mapper { get; private set; }
            public GameObject Root { get; private set; }
            public Text Text { get; private set; }
            public ButtonElements HogeButton { get; private set; }

            public UIElements()
            {
            }

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

        public class UIElementsDummy : IMappedObject
        {
            public IMapper Mapper { get; private set; }
            public GameObject Root { get; private set; }
            public Text Text { get; private set; }
            public ButtonElementsDummy HogeButton { get; private set; }

            public UIElementsDummy()
            {
            }

            public UIElementsDummy(IMapper mapper)
            {
                Initialize(mapper);
            }

            public void Initialize(IMapper mapper)
            {
                Mapper = mapper;
                Root = mapper.Get();
                Text = mapper.Get<Text>("./Text");
                HogeButton = mapper.GetChild<ButtonElementsDummy>("HogeButton");
            }
        }

        public class ButtonElementsDummy : IMappedObject
        {
            public IMapper Mapper { get; private set; }
            public GameObject Root { get; private set; }
            public Button Button { get; private set; }
            public Text Text { get; private set; }
            public Text Text2 { get; private set; }

            public void Initialize(IMapper mapper)
            {
                Mapper = mapper;
                Root = mapper.Get();
                Button = mapper.Get<Button>();
                Text = mapper.Get<Text>("Text");
                Text2 = mapper.Get<Text>("Text2");
            }
        }
    }
}
