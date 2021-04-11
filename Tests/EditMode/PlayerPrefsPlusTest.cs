using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

using Random = UnityEngine.Random;

namespace PlayerPrefsPlus.Tests.EditMode
{
    public class PlayerPrefsPlusTest
    {
        private class TestDataClass
        {
            public int IntProperty { get; set; } = 999;

            public float FloatProperty { get; set; } = 999.0f;

            public string StringProperty { get; set; } = nameof(StringProperty);

            public bool BoolProperty { get; set; } = true;

            public int IntField = -999;

            public float FloatField = -999.0f;

            public string StringField = nameof(StringField);

            public bool BoolField = true;
        }

        private enum TestDataClassMember
        {
            IntProperty,
            FloatProperty,
            StringProperty,
            BoolProperty,
            IntField,
            FloatField,
            StringField,
            BoolField,
        }

        private class TestDataClassPlayerPrefs : PlayerPrefsPlus<TestDataClassPlayerPrefs, TestDataClass>
        {
            public TestDataClassPlayerPrefs() : base() { }
        }

        private abstract class TestDataInfo
        {
            public string Key { get; protected set; }

            public object Value { get; protected set; }

            public abstract object DifferentValue { get; }

            public abstract object Get();

            protected abstract void Set(object value);

            public void Set()
            {
                Set(this.Value);
            }

            public void SetDifferentValue()
            {
                Set(this.DifferentValue);
            }

            public void DeleteKey()
            {
                PlayerPrefs.DeleteKey(Key);
            }
        }

        private class TestDataInfo<T> : TestDataInfo
        {
            public override object DifferentValue => differentValue;

            private Func<string, T> get { get; }

            private Action<string, T> set { get; }

            private T differentValue { get; }

            public TestDataInfo(string key, Func<string, T> get, Action<string, T> set, Func<T> generateValue, Func<T, T> generateDifferentValue)
            {
                this.Key = key;
                this.get = get;
                this.set = set;

                var value = generateValue();
                this.Value = value;
                this.differentValue = generateDifferentValue(value);
            }

            public override object Get()
            {
                return get(Key);
            }

            protected override void Set(object value)
            {
                set(Key, (T)value);
            }
        }

        private readonly IReadOnlyDictionary<TestDataClassMember, TestDataInfo> TestData = new Dictionary<TestDataClassMember, TestDataInfo>
        {
            {
                TestDataClassMember.IntProperty,
                new TestDataInfo<int>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.IntProperty)}", PlayerPrefs.GetInt, PlayerPrefs.SetInt, () => Random.Range(1, 100), _ => Random.Range(101, 200))
            },
            {
                TestDataClassMember.FloatProperty,
                new TestDataInfo<float>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.FloatProperty)}", PlayerPrefs.GetFloat, PlayerPrefs.SetFloat, () => Random.Range(1.0f, 100.0f), _ => Random.Range(101.0f, 200.0f))
            },
            {
                TestDataClassMember.StringProperty,
                new TestDataInfo<string>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.StringProperty)}", PlayerPrefs.GetString, PlayerPrefs.SetString, () => Guid.NewGuid().ToString(), _ => Guid.NewGuid().ToString())
            },
            {
                TestDataClassMember.BoolProperty,
                new TestDataInfo<int>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.BoolProperty)}", PlayerPrefs.GetInt, PlayerPrefs.SetInt, () => 1, _ => 0)
            },
            {
                TestDataClassMember.IntField,
                new TestDataInfo<int>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.IntField)}", PlayerPrefs.GetInt, PlayerPrefs.SetInt, () => Random.Range(1, 100), _ => Random.Range(101, 200))
            },
            {
                TestDataClassMember.FloatField,
                new TestDataInfo<float>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.FloatField)}", PlayerPrefs.GetFloat, PlayerPrefs.SetFloat, () => Random.Range(1.0f, 100.0f), _ => Random.Range(101.0f, 200.0f))
            },
            {
                TestDataClassMember.StringField,
                new TestDataInfo<string>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.StringField)}", PlayerPrefs.GetString, PlayerPrefs.SetString, () => Guid.NewGuid().ToString(), _ => Guid.NewGuid().ToString())
            },
            {
                TestDataClassMember.BoolField,
                new TestDataInfo<int>($"{typeof(TestDataClass).FullName}.{nameof(TestDataClass.BoolField)}", PlayerPrefs.GetInt, PlayerPrefs.SetInt, () => 1, _ => 0)
            },
        };

        private void CleanUp()
        {
            foreach (var i in TestData)
            {
                i.Value.DeleteKey();
            }
        }

        private void SetAllTestDataValues()
        {
            foreach (var i in TestData)
            {
                i.Value.Set();
            }
        }

        private void SetAllTestDataDifferentValues()
        {
            foreach (var i in TestData)
            {
                i.Value.SetDifferentValue();
            }
        }

        [SetUp]
        public void SetUp()
        {
            CleanUp();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp();
        }

        [Test]
        public void DefaultValue_Test()
        {
            Assert.AreEqual(999, TestDataClassPlayerPrefs.Get(x => x.IntProperty));
            Assert.AreEqual(999.0f, TestDataClassPlayerPrefs.Get(x => x.FloatProperty));
            Assert.AreEqual(nameof(TestDataClass.StringProperty), TestDataClassPlayerPrefs.Get(x => x.StringProperty));
            Assert.AreEqual(true, TestDataClassPlayerPrefs.Get(x => x.BoolProperty));
            Assert.AreEqual(-999, TestDataClassPlayerPrefs.Get(x => x.IntField));
            Assert.AreEqual(-999.0f, TestDataClassPlayerPrefs.Get(x => x.FloatField));
            Assert.AreEqual(true, TestDataClassPlayerPrefs.Get(x => x.BoolField));
        }

        [Test]
        public void Get_Test()
        {
            SetAllTestDataValues();

            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestDataClassPlayerPrefs.Get(x => x.IntProperty));
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestDataClassPlayerPrefs.Get(x => x.FloatProperty));
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestDataClassPlayerPrefs.Get(x => x.StringProperty));
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestDataClassPlayerPrefs.Get(x => x.BoolProperty) ? 1 : 0);
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestDataClassPlayerPrefs.Get(x => x.IntField));
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestDataClassPlayerPrefs.Get(x => x.FloatField));
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestDataClassPlayerPrefs.Get(x => x.StringField));
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestDataClassPlayerPrefs.Get(x => x.BoolField) ? 1 : 0);
        }

        [Test]
        public void Set_Test()
        {
            TestDataClassPlayerPrefs.Set(x => x.IntProperty, TestData[TestDataClassMember.IntProperty].Value);
            TestDataClassPlayerPrefs.Set(x => x.FloatProperty, TestData[TestDataClassMember.FloatProperty].Value);
            TestDataClassPlayerPrefs.Set(x => x.StringProperty, TestData[TestDataClassMember.StringProperty].Value);
            TestDataClassPlayerPrefs.Set(x => x.BoolProperty, (int)TestData[TestDataClassMember.BoolProperty].Value != 0);
            TestDataClassPlayerPrefs.Set(x => x.IntField, TestData[TestDataClassMember.IntField].Value);
            TestDataClassPlayerPrefs.Set(x => x.FloatField, TestData[TestDataClassMember.FloatField].Value);
            TestDataClassPlayerPrefs.Set(x => x.StringField, TestData[TestDataClassMember.StringField].Value);
            TestDataClassPlayerPrefs.Set(x => x.BoolField, (int)TestData[TestDataClassMember.BoolProperty].Value != 0);

            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());
        }

        private void SetPrefsDataDifferentValues(TestDataClassPlayerPrefs prefs)
        {
            prefs.Data.IntProperty = (int)TestData[TestDataClassMember.IntProperty].DifferentValue;
            prefs.Data.FloatProperty = (float)TestData[TestDataClassMember.FloatProperty].DifferentValue;
            prefs.Data.StringProperty = (string)TestData[TestDataClassMember.StringProperty].DifferentValue;
            prefs.Data.BoolProperty = (int)TestData[TestDataClassMember.BoolProperty].DifferentValue != 0;
            prefs.Data.IntField = (int)TestData[TestDataClassMember.IntField].DifferentValue;
            prefs.Data.FloatField = (float)TestData[TestDataClassMember.FloatField].DifferentValue;
            prefs.Data.StringField = (string)TestData[TestDataClassMember.StringField].DifferentValue;
            prefs.Data.BoolField = (int)TestData[TestDataClassMember.BoolField].DifferentValue != 0;
        }

        [Test]
        public void Load_Test()
        {
            var prefs = new TestDataClassPlayerPrefs();

            SetAllTestDataValues();

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.IntProperty);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.BoolProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            SetPrefsDataDifferentValues(prefs);
            prefs.Load(x => x.BoolField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);
        }

        [Test]
        public void LoadAll_Test()
        {
            var prefs = new TestDataClassPlayerPrefs();

            SetAllTestDataValues();
            SetPrefsDataDifferentValues(prefs);

            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);

            prefs.LoadAll();

            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, prefs.Data.IntProperty);
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, prefs.Data.FloatProperty);
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, prefs.Data.StringProperty);
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, prefs.Data.BoolProperty ? 1 : 0);
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, prefs.Data.IntField);
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, prefs.Data.FloatField);
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, prefs.Data.StringField);
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, prefs.Data.BoolField ? 1 : 0);
        }

        private void SetPrefsDataValues(TestDataClassPlayerPrefs prefs)
        {
            prefs.Data.IntProperty = (int)TestData[TestDataClassMember.IntProperty].Value;
            prefs.Data.FloatProperty = (float)TestData[TestDataClassMember.FloatProperty].Value;
            prefs.Data.StringProperty = (string)TestData[TestDataClassMember.StringProperty].Value;
            prefs.Data.BoolProperty = (int)TestData[TestDataClassMember.BoolProperty].Value != 0;
            prefs.Data.IntField = (int)TestData[TestDataClassMember.IntField].Value;
            prefs.Data.FloatField = (float)TestData[TestDataClassMember.FloatField].Value;
            prefs.Data.StringField = (string)TestData[TestDataClassMember.StringField].Value;
            prefs.Data.BoolField = (int)TestData[TestDataClassMember.BoolField].Value != 0;
        }

        [Test]
        public void Store_Test()
        {
            var prefs = new TestDataClassPlayerPrefs();

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.IntProperty);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.FloatProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.StringProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.BoolProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.IntField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.FloatField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.StringField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);
            prefs.Store(x => x.BoolField);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());
        }

        [Test]
        public void StoreAll_Test()
        {
            var prefs = new TestDataClassPlayerPrefs();

            SetAllTestDataDifferentValues();
            SetPrefsDataValues(prefs);

            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            prefs.StoreAll();

            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());
        }

        [Test]
        public void Delete_Test()
        {
            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.IntProperty);
            Assert.AreNotEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.FloatProperty);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.StringProperty);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.BoolProperty);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.IntField);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.FloatField);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.StringField);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());

            SetAllTestDataValues();
            TestDataClassPlayerPrefs.Delete(x => x.BoolField);
            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreNotEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());
        }

        [Test]
        public void DeleteAll_Test()
        {
            string nonDeletedPrefsKey = nameof(nonDeletedPrefsKey);
            string nonDeletedPrefsValue = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(nonDeletedPrefsKey, nonDeletedPrefsValue);

            SetAllTestDataValues();

            Assert.AreEqual(TestData[TestDataClassMember.IntProperty].Value, TestData[TestDataClassMember.IntProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatProperty].Value, TestData[TestDataClassMember.FloatProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringProperty].Value, TestData[TestDataClassMember.StringProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolProperty].Value, TestData[TestDataClassMember.BoolProperty].Get());
            Assert.AreEqual(TestData[TestDataClassMember.IntField].Value, TestData[TestDataClassMember.IntField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.FloatField].Value, TestData[TestDataClassMember.FloatField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.StringField].Value, TestData[TestDataClassMember.StringField].Get());
            Assert.AreEqual(TestData[TestDataClassMember.BoolField].Value, TestData[TestDataClassMember.BoolField].Get());
            Assert.AreEqual(nonDeletedPrefsValue, PlayerPrefs.GetString(nonDeletedPrefsKey, null));

            TestDataClassPlayerPrefs.DeleteAll();

            DefaultValue_Test();
            Assert.AreEqual(nonDeletedPrefsValue, PlayerPrefs.GetString(nonDeletedPrefsKey, null));
        }
    }
}
