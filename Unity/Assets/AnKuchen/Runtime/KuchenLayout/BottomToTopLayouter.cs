using AnKuchen.Map;
using UnityEngine;

namespace AnKuchen.KuchenLayout.Layouter
{
    public class BottomToTopLayouter : ILayouter
    {
        private readonly float margin;

        public BottomToTopLayouter(float margin)
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
                position.y += rectTransform.sizeDelta.y;
                position.y += margin;
            }
        }
    }
}