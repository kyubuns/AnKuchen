using UnityEngine;

namespace AnKuchen.UIMapper
{
    public interface IMapper
    {
        GameObject Get();
        T Get<T>() where T : Component;

        GameObject Get(string objectPath);
        GameObject Get(uint[] objectPath);
        GameObject[] GetAll(string objectPath);
        GameObject[] GetAll(uint[] objectPath);
        T Get<T>(string objectPath) where T : Component;
        T Get<T>(uint[] objectPath) where T : Component;
        T GetChild<T>(string rootObjectPath) where T : IMappedObject, new();
        T GetChild<T>(uint[] rootObjectPath) where T : IMappedObject, new();

        IMapper GetMapper(string rootObjectPath);
        IMapper GetMapper(uint[] rootObjectPath);

        CachedObject[] GetRawElements();
        void Copy(IMapper other);
    }

    public interface IMappedObject
    {
        IMapper Mapper { get; }
        void Initialize(IMapper mapper);
    }
}
