using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ThumbDriveDuplicator
{
    public static class ObjectExtensions
    {
        public static object ParseOrDefault(Type targetType, object dataObject, object defaultValue)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                targetType = Nullable.GetUnderlyingType(targetType);
            if (defaultValue == null && targetType.IsValueType)
                defaultValue = Activator.CreateInstance(targetType);
            var parms = new List<object> { dataObject };
            if (defaultValue != null)
                parms.Add(defaultValue);
            var methods = typeof(ObjectExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name.Equals("ParseOrDefault") && m.IsDefined(typeof(ExtensionAttribute), false));
            var typeArgs = new Type[] { targetType };
            var methodNfo = methods.First(m => m.GetParameters().Length.Equals(parms.Count)).MakeGenericMethod(typeArgs);
            return methodNfo.Invoke(null, parms.ToArray());
        }

        public static T ParseOrDefault<T>(this object dataObject)
        {
            return dataObject.ParseOrDefault<T>(default(T));
        }

        public static T ParseOrDefault<T>(this object dataObject, T defaultValue)
        {
            if (dataObject == null)
                return defaultValue;

            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            if (type.IsAssignableFrom(typeof(bool)) && dataObject.ToString().CanParse<int>())
                return (dataObject.ParseOrDefault<int>() > 0).ParseOrDefault<T>();

            if (type.IsEnum)
            {
                if (!Enum.IsDefined(type, dataObject))
                    return defaultValue;
                return (T)Enum.Parse(type, dataObject.ToString());
            }

            var methodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name.Equals("Parse") && m.GetParameters().Length.Equals(1));
            if (methodInfo != null)
            {
                try
                {
                    return (T)methodInfo.Invoke(null, new object[] { dataObject.ToString() });
                }
                catch
                {
                    return defaultValue;
                }
            }

            if (dataObject.GetType().IsAssignableFrom(type))
                return (T)dataObject;
            else
                return defaultValue;
        }

        public static bool CanParse<T>(this string dataString)
        {
            if (string.IsNullOrEmpty(dataString))
                return false;

            var methodInfo = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(m => m.Name.Equals("Parse") && m.GetParameters().Length.Equals(1));
            if (methodInfo != null)
            {
                try
                {
                    var v = (T)methodInfo.Invoke(null, new object[] { dataString });
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public static string FormatString(this object value, string message)
        {
            return value.FormatString(message, true);
        }

        public static string FormatString(this object value, string message, bool removeEmptyElements)
        {
            message = message.Replace("{Value}", value.ToString());
            var properties = value.GetType().GetProperties().Select(p => new
            {
                FormatString = string.Format("{{{0}}}", p.Name),
                ReplacementValue = p.GetValue(value, null).ToString()
            });
            foreach (var item in properties)
                message = message.Replace(item.FormatString, item.ReplacementValue);
            if (removeEmptyElements)
            {
                var messageRegex = new Regex("{(?!{)[^}]+}(?!})");
                message = messageRegex.Replace(message, string.Empty);
                message = message.Replace("{{", "{");
                message = message.Replace("}}", "}");
            }
            return message;
        }
    }
}
