using System;
using System.Linq;
using System.Reflection;

namespace HeavenVr.Helpers;

public static class TypeExtensions
{
    private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.Static;

    public static MethodInfo GetAnyMethod(this Type type, string name)
    {
        return type.GetMethod(name, Flags) ??
               type.BaseType?.GetMethod(name, Flags) ??
               type.BaseType?.BaseType?.GetMethod(name, Flags);
    }

    private static MemberInfo GetAnyMember(this Type type, string name)
    {
        return type.GetMember(name, Flags).FirstOrDefault() ??
               type.BaseType?.GetMember(name, Flags).FirstOrDefault() ??
               type.BaseType?.BaseType?.GetMember(name, Flags).FirstOrDefault();
    }

    public static void SetValue(this object obj, string name, object value)
    {
        switch (obj.GetType().GetAnyMember(name))
        {
            case FieldInfo field:
                field.SetValue(obj, value);
                break;
            case PropertyInfo property:
                property.SetValue(obj, value, null);
                break;
        }
    }
}