using System;
using UnityEngine;

namespace AnKuchen.Map
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

    public abstract class AnKuchenException : Exception
    {
        protected AnKuchenException(string message) : base(message)
        {
        }
    }

    public class AnKuchenNotFoundException : AnKuchenException
    {
        public uint[] Path { get; }

        public AnKuchenNotFoundException(string message, uint[] path) : base(message)
        {
            Path = path;
        }
    }

    public class AnKuchenNotUniqueException : AnKuchenException
    {
        public uint[] Path { get; }

        public AnKuchenNotUniqueException(string message, uint[] path) : base(message)
        {
            Path = path;
        }
    }
}
