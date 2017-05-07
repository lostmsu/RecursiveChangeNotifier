namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    static class ReflectionHelper
    {
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            while(type != null) {
                TypeInfo info = type.GetTypeInfo();
                PropertyInfo property = info.GetDeclaredProperty(name);
                if (property != null)
                    return property;
                type = info.BaseType;
            }
            return null;
        }
    }
}
