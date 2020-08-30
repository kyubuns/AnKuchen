using AnKuchen.UIMapper;
using UnityEngine;

namespace AnKuchen.UILayouter
{
    public static partial class Layouter
    {
        public static LayoutEditor<T> TopToBottom<T>(T original) where T : IMappedObject, new()
        {
            return new LayoutEditor<T>(new TopToBottomLayouter(), original);
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
