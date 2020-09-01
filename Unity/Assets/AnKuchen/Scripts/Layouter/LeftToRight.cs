using AnKuchen.Mapper;
using UnityEngine;

namespace AnKuchen.Layouter
{
    public static partial class Layouter
    {
        public static LayoutEditor LeftToRight(IMapper original, float margin = 0f)
        {
            return new LayoutEditor(new LeftToRightLayouter(margin), original);
        }

        public static LayoutEditor<T> LeftToRight<T>(T original, float margin = 0f) where T : IMappedObject, new()
        {
            return new LayoutEditor<T>(new LeftToRightLayouter(margin), original);
        }
    }

    public class LeftToRightLayouter : ILayouter
    {
        private readonly float margin;

        public LeftToRightLayouter(float margin)
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
                position.x += rectTransform.sizeDelta.x;
                position.x += margin;
            }
        }
    }
}
