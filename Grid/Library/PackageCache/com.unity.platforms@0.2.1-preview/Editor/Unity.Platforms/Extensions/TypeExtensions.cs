using System;

namespace Unity.Platforms
{
    internal static class TypeExtensions
    {
        public static string GetFullyQualifedAssemblyTypeName(this Type type)
        {
            return $"{type}, {type.Assembly.GetName().Name}";
        }
    }
}
