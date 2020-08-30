using AnKuchen.UIMapper;
using UnityEngine;

namespace AnKuchen.UILayouter
{
    public static partial class Layouter
    {
        public static LayoutEditor LeftToRight(IMapper original)
        {
            return new LayoutEditor(new LeftToRightLayouter(), original);
        }
    }

    public class LeftToRightLayouter : ILayouter
    {
        public void Layout(IMapper original, IMapper[] elements)
        {
            var position = original.Get<RectTransform>().anchoredPosition;
            foreach (var e in elements)
            {
                var rectTransform = e.Get<RectTransform>();
                rectTransform.anchoredPosition = position;
                position.x += rectTransform.sizeDelta.x;
            }
        }
    }
}
