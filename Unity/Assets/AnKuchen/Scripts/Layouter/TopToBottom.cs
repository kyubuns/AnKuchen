using AnKuchen.Mapper;
using UnityEngine;

namespace AnKuchen.Layouter
{
    public static partial class Layouter
    {
        public static LayoutEditor TopToBottom(IMapper original, float margin = 0f)
        {
            return new LayoutEditor(new TopToBottomLayouter(margin), original);
        }

        public static LayoutEditor<T> TopToBottom<T>(T original, float margin = 0f) where T : IMappedObject, new()
        {
            return new LayoutEditor<T>(new TopToBottomLayouter(margin), original);
        }
    }

    public class TopToBottomLayouter : ILayouter
    {
        private readonly float margin;

        public TopToBottomLayouter(float margin)
        {
            this.margin = margin;
        }

        public void Layout(IMapper original, IMapper[] elements)
        {
            var position = original.Get<RectTransform>().anchoredPosition;
            foreach (var e in elements)
            {
                var rectTransform = e.Get<RectTransform>();
                rectTransform.anchoredPosition = position;
                position.y -= rectTransform.sizeDelta.y;
                position.y -= margin;
            }
        }
    }
}
