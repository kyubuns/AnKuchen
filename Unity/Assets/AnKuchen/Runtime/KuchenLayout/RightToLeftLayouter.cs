using AnKuchen.Map;
using UnityEngine;
using UnityEngine.UI;

namespace AnKuchen.KuchenLayout.Layouter
{
    public class RightToLeftLayouter : ILayouter
    {
        private readonly float margin;

        public RightToLeftLayouter(float margin = 0f)
        {
            this.margin = margin;
        }

        public RightToLeftLayouter(HorizontalLayoutGroup layout)
        {
            layout.enabled = false;
            margin = layout.spacing;
        }

        public void Layout(IMapper original, IMapper[] elements)
        {
            var position = original.Get<RectTransform>().anchoredPosition;
            foreach (var e in elements)
            {
                var rectTransform = e.Get<RectTransform>();
                rectTransform.anchoredPosition = position;
                position.x -= rectTransform.sizeDelta.x;
                position.x -= margin;
            }
        }
    }
}