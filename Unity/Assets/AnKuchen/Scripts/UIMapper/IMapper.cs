using UnityEngine;

namespace AnKuchen.UIMapper
{
    public interface IMapper
    {
        GameObject Get();
        T Get<T>() where T : Component;

        GameObject Get(string objectPath);
        GameObject[] GetAll(string objectPath);
        T Get<T>(string objectPath) where T : Component;

        IMapper GetChild(string rootObjectPath);
        T GetChild<T>(string rootObjectPath) where T : IDuplicatable, new();

        CachedObject[] GetRawElements();
        void Copy(IMapper other);
    }

    public interface IInitializable
    {
        void Initialize(IMapper mapper);
    }

    public interface IDuplicatable : IInitializable
    {
        IMapper Mapper { get; }
    }
}
