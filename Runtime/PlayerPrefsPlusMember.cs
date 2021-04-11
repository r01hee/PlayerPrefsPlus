using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace PlayerPrefsPlus
{
    public abstract class PlayerPrefsPlusMember
    {
        private static readonly Type[] SupportedTypes =
        {
            typeof(int),
            typeof(float),
            typeof(string),
            typeof(bool),
        };

        public string PrefsKey { get; private set; }

        public string Name { get; private set; }

        public Type Type { get; private set; }

        public object DefaultValue { get; private set; }

        private PropertyInfo propertyInfo { get; set; }

        private FieldInfo fieldInfo { get; set; }

        public static bool IsSupportedType(Type type)
        {
            return SupportedTypes.Contains(type);
        }

        protected void Construct<TData>(string prefsKeyPrefix, string prefsKey, string memberName, object defaultValue) where TData : new()
        {
            if (prefsKey == null) { throw new ArgumentNullException(nameof(prefsKey)); }
            if (memberName == null) { throw new ArgumentNullException(nameof(memberName)); }

            fieldInfo = typeof(TData).GetField(memberName);
            propertyInfo = typeof(TData).GetProperty(memberName);

            Type = fieldInfo?.FieldType ?? propertyInfo?.PropertyType;
            if (!IsSupportedType(Type))
            {
                throw new NotSupportedException(Type.ToString() + " is not supported type");
            }

            Name = memberName;
            PrefsKey = (prefsKeyPrefix ?? "") + prefsKey;
            DefaultValue = defaultValue;
        }

        public object Get()
        {
            if (Type == typeof(int))
            {
                return PlayerPrefs.GetInt(PrefsKey, (int)DefaultValue);
            }
            else if (Type == typeof(float))
            {
                return PlayerPrefs.GetFloat(PrefsKey, (float)DefaultValue);
            }
            else if (Type == typeof(string))
            {
                return PlayerPrefs.GetString(PrefsKey, (string)DefaultValue);
            }
            else if (Type == typeof(bool))
            {
                return IntToBool(PlayerPrefs.GetInt(PrefsKey, BoolToInt((bool)DefaultValue)));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public T Get<T>()
        {
            return (T)Get();
        }

        public void Set(object value)
        {
            if (Type == typeof(int))
            {
                PlayerPrefs.SetInt(PrefsKey, (int)value);
            }
            else if (Type == typeof(float))
            {
                PlayerPrefs.SetFloat(PrefsKey, (float)value);
            }
            else if (Type == typeof(string))
            {
                PlayerPrefs.SetString(PrefsKey, (string)value);
            }
            else if (Type == typeof(bool))
            {
                PlayerPrefs.SetInt(PrefsKey, BoolToInt((bool)value));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Load(object data)
        {
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(data, Get());
            }
            else
            {
                propertyInfo.SetValue(data, Get());
            }
        }

        public void Store(object data)
        {
            if (fieldInfo != null)
            {
                Set(fieldInfo.GetValue(data));
            }
            else
            {
                Set(propertyInfo.GetValue(data));
            }
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
        }

        private int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }

        private bool IntToBool(int value)
        {
            return value != 0;
        }
    }

    public class PlayerPrefsPlusMember<TData> : PlayerPrefsPlusMember where TData : new()
    {
        public static MemberExpression GetMemberExpression<T>(Expression<Func<TData, T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                return memberExpression;
            }

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                return unaryExpression.Operand as MemberExpression;
            }

            return null;
        }

        public PlayerPrefsPlusMember(string memberName, object defaultValue) : this(null, memberName, defaultValue) { }

        public PlayerPrefsPlusMember(string prefsKeyPrefix, string memberName, object defaultValue)
        {
            Construct<TData>(prefsKeyPrefix, memberName, memberName, defaultValue);
        }

        public PlayerPrefsPlusMember(Expression<Func<TData, object>> expression, object defaultValue) : this(null, expression, defaultValue) { }

        public PlayerPrefsPlusMember(string prefsKeyPrefix, Expression<Func<TData, object>> expression, object defaultValue) : this(prefsKeyPrefix, null, expression, defaultValue) { }

        public PlayerPrefsPlusMember(string prefsKeyPrefix, string prefsKey, Expression<Func<TData, object>> expression, object defaultValue)
        {
            var memberExpression = GetMemberExpression(expression); 
            Construct<TData>(prefsKeyPrefix, prefsKey ?? memberExpression.Member.Name, memberExpression.Member.Name, defaultValue);
        }
    }
}
