using UnityEngine;

namespace AnKuchen.UIMapper
{
    public interface IMapper
    {
        GameObject Get();
        T Get<T>() where T : Component;

        GameObject Get(string objectPath);
        T Get<T>(string objectPath) where T : Component;

        IMapper GetChild(string rootObjectPath);
    }
}
