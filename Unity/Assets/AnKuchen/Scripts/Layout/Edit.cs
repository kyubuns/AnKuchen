using AnKuchen.Map;

namespace AnKuchen.Layout
{
    public static partial class Layouter
    {
        public static LayoutEditor Edit(IMapper original)
        {
            return new LayoutEditor(new NoneLayouter(), original);
        }

        public static LayoutEditor<T> Edit<T>(T original) where T : IMappedObject, new()
        {
            return new LayoutEditor<T>(new NoneLayouter(), original);
        }
    }

    public class NoneLayouter : ILayouter
    {
        public void Layout(IMapper original, IMapper[] elements)
        {
        }
    }
}
