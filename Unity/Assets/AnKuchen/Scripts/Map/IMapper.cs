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
        public string PathString { get; }
        public uint[] PathHash { get; }
        public Type Type { get; }

        public AnKuchenNotFoundException(uint[] pathHash, Type type) : base(
            type == null
                ? $"[{string.Join(", ", pathHash)}] is not found"
                : $"[{string.Join(", ", pathHash)}]<{type}> is not found"
        )
        {
            PathString = null;
            PathHash = pathHash;
            Type = type;
        }

        public AnKuchenNotFoundException(string pathString, Type type) : base(
            type == null
                ? $"{pathString} is not found"
                : $"{pathString}<{type}> is not found"
        )
        {
            PathString = pathString;
            PathHash = null;
            Type = type;
        }
    }

    public class AnKuchenNotUniqueException : AnKuchenException
    {
        public string PathString { get; }
        public uint[] PathHash { get; }
        public Type Type { get; }

        public AnKuchenNotUniqueException(uint[] pathHash, Type type) : base(
            type == null
                ? $"[{string.Join(", ", pathHash)}] is not unique"
                : $"[{string.Join(", ", pathHash)}]<{type}> is not unique"
        )
        {
            PathString = null;
            PathHash = pathHash;
            Type = type;
        }

        public AnKuchenNotUniqueException(string pathString, Type type) : base(
            type == null
                ? $"{pathString} is not unique"
                : $"{pathString}<{type}> is not unique"
        )
        {
            PathString = pathString;
            PathHash = null;
            Type = type;
        }
    }
}
