using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ThumbDriveDuplicator
{
    public static class EnumExtensions
    {
        public static IDictionary<int, string> EnumToDictionary(this Type type)
        {
            if (!type.IsEnum)
                throw new InvalidCastException("Type must be an Enum type");
            var desc = type.GetFields().Where(f => f.IsLiteral)
                .Select((f, i) => new { Index = i, Name = f.Name, Description = (f.GetCustomAttributes(typeof(DescriptionAttribute), false).DefaultIfEmpty(new DescriptionAttribute(f.Name.Replace('_', ' '))).First() as DescriptionAttribute).Description });
            return desc.ToDictionary(a => a.Index, a => a.Description);
        }

        public static string GetDescription(this Enum value)
        {
            return value.GetDescription(false);
        }

        public static string GetDescription(this Enum value, bool throwIfNotFound)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (string.IsNullOrEmpty(name))
            {
                if (throwIfNotFound)
                    throw new Exception(string.Format("Could not find name for type {0}", type.Name));
                return value.ToString();
            }

            var fieldInfo = type.GetField(name);
            if (fieldInfo == null)
            {
                if (throwIfNotFound)
                    throw new Exception(string.Format("Could not find field {0} for type {1}", name, type.Name));
                return value.ToString();
            }

            var attr = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attr == null)
            {
                if (throwIfNotFound)
                    throw new Exception(string.Format("Could not find description attribute for field {0}", name));
                return value.ToString();
            }
            return attr.Description;
        }
    }
}
