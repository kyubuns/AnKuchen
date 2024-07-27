# AnKuchen

Control UI Prefab from Script Library  
You won't have to drag and drop on Inspector when you create your UI.

## Instructions

- [UnityPackage](https://github.com/kyubuns/AnKuchen/releases)
- Package Manager
  - Import AnKuchen `https://github.com/kyubuns/AnKuchen.git?path=Unity/Assets/AnKuchen`

## Sample

Sample GameObject  
<img width="640" alt="Screen Shot 2020-09-03 at 20 49 27" src="https://user-images.githubusercontent.com/961165/92111226-f6806900-ee26-11ea-9718-beb328af5038.png">

### Get Button

You can get a component from a GameObject simply by specifying its name.  
If there is only one `HogeButton` under the UICache, the same code will work regardless of where the HogeButton is located.

```csharp
public class Sample : MonoBehaviour
{
    [SerializeField] private UICache root = default;

    public void Start()
    {
        var hogeButton = root.Get<Button>("HogeButton");
    }
}
```

### Get Text under the HogeButton

Look at the image above. There are four GameObjects named `Text`.  
If you want to get the Text under the HogeButton, you can do this.  
As before, it works whether the `HogeButton` is directly under the `Root` or not.

```csharp
var hogeButtonText = root.Get<Text>("HogeButton/Text");
hogeButtonText.text = "Hoge!";
```

### Get Text directly under the root

```csharp
var text = root.Get<Text>("Text"); // This is an error." There are four names for "Text".
var text = root.Get<Text>("./Text"); // Root/Text
```

### Create children map

When you want to do the same thing with all three buttons in the UI,  
you can treat each of them as a map.

```csharp
public void Start()
{
    var hogeButton = root.GetMapper("HogeButton");
    var fugaButton = root.GetMapper("FugaButton");
    var piyoButton = root.GetMapper("PiyoButton");

    SetButtonText(hogeButton, "Hoge");
    SetButtonText(fugaButton, "Fuga");
    SetButtonText(piyoButton, "Piyo");
}

private void SetButtonText(IMapper button, string labelText)
{
    button.Get<Text>("Text").text = labelText;
}
```

### Duplicate

Want more buttons? You can.

```csharp
var newButton = root.GetMapper("HogeButton").Duplicate();
newButton.Get<Text>("Text").text = "New Button!";
```

### Code Template

Did you notice the "Copy Template" button in UICacheComponent?  
This way you won't have to worry about typo.

```csharp
public void Start()
{
    var ui = new UIElements(root);
    ui.HogeButtonText.text = "Hoge"; // I love having a type!
}

// ↓ The code from here on down is in your clipboard!
public class UIElements : IMappedObject
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
        FugaButton = mapper.Get<Button>("FugaButton");
        FugaButtonText = mapper.Get<Text>("FugaButton/Text");
        PiyoButton = mapper.Get<Button>("PiyoButton");
        PiyoButtonText = mapper.Get<Text>("PiyoButton/Text");
    }
}
```

### Duplicate with type

You can still use `Duplicate` and `Layouter`, even if you have the type. Don't worry.

```csharp
public void Start()
{
    var ui = new UIElements(root);
    using (var editor = ui.HogeButton.Edit())
    {
        foreach (var a in new[] { "h1", "h2", "h3" })
        {
            var button = editor.Create();
            button.Text.text = a;
        }
    }
}

public class UIElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public Layout<ButtonElements> HogeButton { get; private set; }

    public UIElements(IMapper mapper)
    {
        Initialize(mapper);
    }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        HogeButton = new Layout<ButtonElements>(mapper.Map<ButtonElements>("HogeButton"));
    }
}

public class ButtonElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public Text Text { get; private set; }

    public ButtonElements() { }
    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Text = mapper.Get<Text>("Text");
    }
}
```

### Testing

Someone changed the Prefab after I made the type!  
Let's prevent such accidents.

```csharp
var test1Object = Resources.Load<GameObject>("Test1");
Assert.DoesNotThrow(() => UIElementTester.Test<UIElements>(test1Object));
```

### Scroll List

![output](https://user-images.githubusercontent.com/961165/102043233-3805b480-3e17-11eb-8f2a-c54b6121a64a.gif)

```csharp
public class Sample : MonoBehaviour
{
    [SerializeField] private UICache root = default;

    public void Start()
    {
        var ui = new UIElements(root);

        using (var editor = ui.List.Edit())
        {
            editor.Spacing = 10f;
            editor.Margin.TopBottom = 10f;

            for (var i = 0; i < 1000; ++i)
            {
                if (Random.Range(0, 2) == 0)
                {
                    var i1 = i;
                    editor.Add((ListElements1 x) =>
                    {
                        x.LineText.text = $"Test {i1}";
                    });
                }
                else
                {
                    editor.Add((ListElements2 x) =>
                    {
                        x.Background.color = Random.ColorHSV();
                        x.Button.onClick.AddListener(() => Debug.Log("Click Button"));
                    });
                }
            }
        }
    }
}

public class UIElements : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public VerticalList<ListElements1, ListElements2> List { get; private set; }

    public UIElements(IMapper mapper)
    {
        Initialize(mapper);
    }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        List = new VerticalList<ListElements1, ListElements2>(
            mapper.Get<ScrollRect>("List"),
            mapper.Map<ListElements1>("Element1"),
            mapper.Map<ListElements2>("Element2")
        );
    }
}

public class ListElements1 : IMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Text LineText { get; private set; }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        LineText = mapper.Get<Text>("./Text");
    }
}

public class ListElements2 : IReusableMappedObject
{
    public IMapper Mapper { get; private set; }
    public GameObject Root { get; private set; }
    public Image Background { get; private set; }
    public Button Button { get; private set; }

    public void Initialize(IMapper mapper)
    {
        Mapper = mapper;
        Root = mapper.Get();
        Background = mapper.Get<Image>("./Image");
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
```

## With

### With AkyuiUnity

- AkyuiUnity(.Xd) is Adobe XD to Unity(uGUI) Library.
- Please see [AkyuiUnity Manual](https://github.com/kyubuns/AkyuiUnity/blob/main/Manual/Manual_en.md#connecting-to-ankuchen)

## Setup

- Install `AnKuchen`
- Add a UICacheComponent to the Root of the UI and press the Update button.
  - We recommend that you automate the process of pressing the Update button to fit your workflow.
- To update, rewrite the Hash in Packages/packages-lock.json.

## Requirements

- Requires Unity2019.4 or later

## License

MIT License (see [LICENSE](LICENSE))
