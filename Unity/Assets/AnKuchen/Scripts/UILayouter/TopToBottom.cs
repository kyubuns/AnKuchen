using AnKuchen.UIMapper;
using UnityEngine;

namespace AnKuchen.UILayouter
{
    public static partial class Layouter
    {
        public static LayoutEditor TopToBottom(IMapper original)
        {
            return new LayoutEditor(new TopToBottomLayouter(), original);
        }
    }

    public class TopToBottomLayouter : ILayouter
    {
        public void Layout(IMapper original, IMapper[] elements)
        {
            var position = original.Get<RectTransform>().anchoredPosition;
            foreach (var e in elements)
            {
                var rectTransform = e.Get<RectTransform>();
                rectTransform.anchoredPosition = position;
                position.y -= rectTransform.sizeDelta.y;
            }
        }
    }
}
