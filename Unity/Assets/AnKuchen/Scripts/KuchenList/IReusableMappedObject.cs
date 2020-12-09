using AnKuchen.Map;

namespace AnKuchen.KuchenList
{
    public interface IReusableMappedObject : IMappedObject
    {
        void Activate();
        void Deactivate();
    }
}