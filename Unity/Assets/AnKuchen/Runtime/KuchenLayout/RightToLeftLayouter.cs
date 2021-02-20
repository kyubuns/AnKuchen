using AnKuchen.Map;
using UnityEngine;

namespace AnKuchen.KuchenLayout.Layouter
{
    public class RightToLeftLayouter : ILayouter
    {
        private readonly float margin;

        public RightToLeftLayouter(float margin = 0f)
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