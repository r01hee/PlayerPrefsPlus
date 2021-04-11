using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine;

namespace PlayerPrefsPlus
{
    public abstract class PlayerPrefsPlus : IDisposable
    {
        protected static IReadOnlyDictionary<string, PlayerPrefsPlusMember> Members { get; private set; }

        protected static void StaticConstruct(IEnumerable<PlayerPrefsPlusMember> members)
        {
            Members = members.ToDictionary(x => x.Name, x => x);
        }

        protected virtual string PrefsKeyPrefix { get; }

        protected abstract bool SaveOnDisposed { get; }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && SaveOnDisposed)
            {
                Save();
            }
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void DeleteAll()
        {
            foreach (var member in Members.Select(x => x.Value))
            {
                member.Delete();
            }
        }

        protected static PlayerPrefsPlusMember<TData> GetMember<TData, T>(Expression<Func<TData, T>> expression) where TData : new()
        {
            var name = PlayerPrefsPlusMember<TData>.GetMemberExpression(expression).Member.Name;
            return Members[name] as PlayerPrefsPlusMember<TData>;
        }
    }

    public abstract class PlayerPrefsPlus<TPrefs, TData> : PlayerPrefsPlus where TPrefs : PlayerPrefsPlus<TPrefs, TData> where TData : new()
    {
        private static readonly string _PrefsKeyPrefix = typeof(TData).FullName + ".";

        public static void PreExecuteStaticConstructor() { /* Do Nothing */ }

        static PlayerPrefsPlus()
        {
            var instance = Activator.CreateInstance(typeof(TPrefs)) as TPrefs;
            StaticConstruct(instance.Configure().ToArray());
        }

        public TData Data { get; }

        protected override string PrefsKeyPrefix => _PrefsKeyPrefix;

        protected override bool SaveOnDisposed => saveOnDisposed;

        private readonly bool saveOnDisposed;

        protected PlayerPrefsPlus() : this(default, false) { }

        protected PlayerPrefsPlus(bool saveOnDisposed) : this(default, saveOnDisposed) { }

        protected PlayerPrefsPlus(TData data) : this(data, false) { }

        protected PlayerPrefsPlus(TData data, bool saveOnDisposed)
        {
            this.saveOnDisposed = saveOnDisposed;

            if (data != null)
            {
                Data = data;
            }
            else
            {
                Data = (TData)Activator.CreateInstance(typeof(TData));
            }
        }

        public static T Get<T>(Expression<Func<TData, T>> expression)
        {
            return GetMember(expression).Get<T>();
        }

        public static void Set<T>(Expression<Func<TData, T>> expression, T value)
        {
            GetMember(expression).Set(value);
        }

        public static void Delete<T>(Expression<Func<TData, T>> expression)
        {
            GetMember(expression).Delete();
        }

        public void Load<T>(Expression<Func<TData, T>> expression)
        {
            GetMember(expression).Load(Data);
        }

        public void LoadAll()
        {
            foreach (var member in Members.Select(x => x.Value))
            {
                member.Load(Data);
            }
        }

        public void StoreAll()
        {
            foreach (var member in Members.Select(x => x.Value))
            {
                member.Store(Data);
            }
        }

        public void Store<T>(Expression<Func<TData, T>> expression)
        {
            GetMember(expression).Store(Data);
        }


        protected virtual IEnumerable<PlayerPrefsPlusMember<TData>> Configure()
        {
            var type = typeof(TData);

            var defaultData = Activator.CreateInstance(type);

            foreach (var p in type.GetFields().Where(x => PlayerPrefsPlusMember.IsSupportedType(x.FieldType)))
            {
                var defaultValue = p.GetValue(defaultData);
                yield return new PlayerPrefsPlusMember<TData>(PrefsKeyPrefix, p.Name, defaultValue);
            }

            foreach (var p in type.GetProperties().Where(x => PlayerPrefsPlusMember.IsSupportedType(x.PropertyType)))
            {
                var defaultValue = p.GetValue(defaultData);
                yield return new PlayerPrefsPlusMember<TData>(PrefsKeyPrefix, p.Name, defaultValue);
            }
        }
    }
}
