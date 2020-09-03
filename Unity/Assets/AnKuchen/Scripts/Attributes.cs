using System;

namespace AnKuchen
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreTestMemberAttribute : Attribute
    {
    }
}
