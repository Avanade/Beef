// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.Reflection
{
    /// <summary>
    /// Represents the complex <see cref="Type"/> code.
    /// </summary>
    public enum ComplexTypeCode
    {
        /// <summary>
        /// Is an <see cref="T:Object"/> (not identified as one of the possible collection types).
        /// </summary>
        Object,

        /// <summary>
        /// Is an <see cref="T:Array"/>.
        /// </summary>
        Array,

        /// <summary>
        /// Is an <see cref="T:ICollection"/>.
        /// </summary>
        ICollection,

        /// <summary>
        /// Is an <see cref="T:IEnumerable"/>.
        /// </summary>
        IEnumerable
    }

    /// <summary>
    /// Utility reflection class for identifying, creating and updating complex types and collections.
    /// </summary>
    public class ComplexTypeReflector
    {
        private MethodInfo _addMethod;

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="T:CollectionType"/>.
        /// </summary>
        public ComplexTypeCode ComplexTypeCode { get; private set; }

        /// <summary>
        /// Gets or sets the collection item <see cref="Type"/> where <see cref="IsCollection"/>; otherwise, the <see cref="PropertyInfo.PropertyType"/>.
        /// </summary>
        public Type ItemType { get; private set; }

        /// <summary>
        /// Indicates whether the <see cref="ItemType"/> is considered a complex type.
        /// </summary>
        public bool IsItemComplexType { get; private set; }

        /// <summary>
        /// Indicates whether the <see cref="ComplexTypeCode"/> is a collection of some description.
        /// </summary>
        public bool IsCollection => ComplexTypeCode != ComplexTypeCode.Object;

        /// <summary>
        /// Creates the collection type from a <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/>.</param>
        /// <returns></returns>
        public static ComplexTypeReflector Create(PropertyInfo pi)
        {
            var ctr = new ComplexTypeReflector
            {
                PropertyInfo = pi ?? throw new ArgumentNullException(nameof(pi)),
                ItemType = pi.PropertyType
            };

            if (pi.PropertyType == typeof(string) || pi.PropertyType.IsPrimitive || pi.PropertyType.IsValueType)
                return ctr;

            if (pi.PropertyType.IsArray)
            {
                ctr.ComplexTypeCode = ComplexTypeCode.Array;
                ctr.ItemType = pi.PropertyType.GetElementType();
            }
            else
            {
                if (pi.PropertyType.GetInterfaces().Any(x => x == typeof(IEnumerable)))
                {
                    var t = GetCollectionType(pi.PropertyType);
                    if (t != null)
                    {
                        ctr.ComplexTypeCode = ComplexTypeCode.ICollection;
                        ctr.ItemType = t;
                        ctr._addMethod = pi.PropertyType.GetMethod("Add", new Type[] { t });
                        if (ctr._addMethod == null)
                            throw new ArgumentException($"Type '{pi.DeclaringType.Name}' Property '{pi.Name}' is an ICollection<> however no Add method could be found.", nameof(pi));
                    }
                    else
                    {
                        t = GetEnumerableType(pi.PropertyType);
                        if (t != null)
                        {
                            ctr.ComplexTypeCode = ComplexTypeCode.IEnumerable;
                            ctr.ItemType = t;
                        }
                        else
                        {
                            var result = GetEnumerableTypeFromAdd(pi.PropertyType);
                            if (result.ItemType != null)
                            {
                                ctr.ComplexTypeCode = ComplexTypeCode.ICollection;
                                ctr.ItemType = result.ItemType;
                                ctr._addMethod = result.AddMethod;
                            }
                        }
                    }
                }
            }

            if (ctr.ItemType != null)
                ctr.IsItemComplexType = !(ctr.ItemType == typeof(string) || ctr.ItemType.IsPrimitive || ctr.ItemType.IsValueType);

            return ctr;
        }

        /// <summary>
        /// Gets the underlying item <see cref="Type"/> where an Array/ICollection/IEnumerable or itself. 
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The item <see cref="Type"/>.</returns>
        public static Type GetItemType(Type type)
        {
            if (type == typeof(string) || type.IsPrimitive || type.IsValueType)
                return type;

            if (type.IsArray)
               return type.GetElementType();

            var t = GetCollectionType(type);
            if (t != null)
                return t;

            t = GetEnumerableType(type);
            if (t != null)
                return t;

            var result = GetEnumerableTypeFromAdd(type);
            if (result.ItemType != null)
                return result.ItemType;

            return type;
        }

        /// <summary>
        /// Gets the underlying ICollection Type.
        /// </summary>
        private static Type GetCollectionType(Type type)
        {
            var t = type.GetInterfaces().FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
            if (t == null)
                return null;

            return ((t == typeof(ICollection<>)) ? type : t).GetGenericArguments()[0];
        }

        /// <summary>
        /// Gets the underlying IEnumerable Type.
        /// </summary>
        private static Type GetEnumerableType(Type type)
        {
            var t = type.GetInterfaces().FirstOrDefault(x => (x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
            if (t == null)
            {
                t = type.GetInterfaces().FirstOrDefault(x => x == typeof(IEnumerable));
                if (t == null)
                    return null;
            }

            var gas = ((t == typeof(IEnumerable)) ? type : t).GetGenericArguments();
            if (gas.Length == 0)
                return null;

            if (type == typeof(IEnumerable<>).MakeGenericType(new Type[] { gas[0] }))
                return gas[0];

            return null;
        }

        /// <summary>
        /// Gets the underlying IEnumerable Type by inferring from the Add method.
        /// </summary>
        private static (Type ItemType, MethodInfo AddMethod) GetEnumerableTypeFromAdd(Type type)
        {
            var mi = type.GetMethod("Add");
            if (mi == null)
                return (null, null);

            var ps = mi.GetParameters();
            return ps.Length == 1 ? (ps[0].ParameterType, mi) : (null, null);
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="obj">The object whose property value will be set.</param>
        /// <param name="value">The property value(s) to set.</param>
        public void SetValue(object obj, IEnumerable value)
        {
            if (obj == null || value == null)
                return;

            PropertyInfo.SetValue(obj, CreateValue(value));
        }

        /// <summary>
        /// Creates the property value as empty; i.e. instantiates only.
        /// </summary>
        /// <returns>The property value.</returns>
        public object CreateValue()
        {
            switch (ComplexTypeCode)
            {
                case ComplexTypeCode.ICollection:
                case ComplexTypeCode.Object:
                    return Activator.CreateInstance(PropertyInfo.PropertyType);

                default:
                    return Array.CreateInstance(ItemType, 0);
            }
        }

        /// <summary>
        /// Creates the property value from an object which is a list/array/collection.
        /// </summary>
        /// <param name="value">The property value(s) to set.</param>
        /// <returns>The property value.</returns>
        public object CreateValue(object value)
        {
            if (value == null)
                return null;

            if (value is IEnumerable)
                return CreateValue((IEnumerable)value);

            switch (ComplexTypeCode)
            {
                case ComplexTypeCode.Array:
                case ComplexTypeCode.IEnumerable:
                    var a = Array.CreateInstance(ItemType, 1);
                    a.SetValue(value, 0);
                    return a;

                case ComplexTypeCode.ICollection:
                    var c = Activator.CreateInstance(PropertyInfo.PropertyType);
                    _addMethod.Invoke(c, new object[] { value });
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Creates the property value from a list/array/collection.
        /// </summary>
        /// <param name="value">The property value(s) to set.</param>
        /// <returns>The property value.</returns>
        public object CreateValue(IEnumerable value)
        {
            IList a = null;
            Type aType = null;
            object c = null;

            if (IsCollection)
            {
                switch (ComplexTypeCode)
                {
                    case ComplexTypeCode.Array:
                    case ComplexTypeCode.IEnumerable:
                        aType = typeof(List<>).MakeGenericType(ItemType);
                        a = (IList)Activator.CreateInstance(aType);
                        break;

                    case ComplexTypeCode.ICollection:
                        c = Activator.CreateInstance(PropertyInfo.PropertyType);
                        break;
                }
            }

            foreach (var val in value)
            {
                if (!IsCollection)
                    return val;

                switch (ComplexTypeCode)
                {
                    case ComplexTypeCode.Array:
                    case ComplexTypeCode.IEnumerable:
                        a.Add(val);
                        break;

                    case ComplexTypeCode.ICollection:
                        _addMethod.Invoke(c, new object[] { val });
                        break;
                }
            }

            if (a != null)
                return aType.GetMethod("ToArray").Invoke(a, null);
            else if (c != null)
                return c;
            else
                return null;
        }

        /// <summary>
        /// Creates an instance of the item value.
        /// </summary>
        /// <returns>An instance of the item value.</returns>
        public object CreateItemValue()
        {
            return Activator.CreateInstance(ItemType);
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The second value.</param>
        /// <returns><c>true</c> if the two source sequences are of equal length and their corresponding elements are equal according to the default equality comparer for their type; otherwise, <c>false</c>.</returns>
        public bool CompareSequence(object left, object right)
        {
            if (ComplexTypeCode == ComplexTypeCode.Object)
                throw new InvalidOperationException("CompareSequence cannot be performed for a ComplexTypeCode.Object.");

            if (left == null && right == null)
                return true;

            if ((left != null && right == null) || (left == null && right != null))
                return false;

            if (left == right)
                return true;

            switch (ComplexTypeCode)
            {
                case ComplexTypeCode.Array:
                    var al = (Array)left;
                    var ar = (Array)right;
                    if (al.Length != ar.Length)
                        return false;

                    break;

                case ComplexTypeCode.ICollection:
                    var cl = (ICollection)left;
                    var cr = (ICollection)right;
                    if (cl.Count != cr.Count)
                        return false;

                    break;
            }

            // Inspired by: https://referencesource.microsoft.com/#System.Core/System/Linq/Enumerable.cs,9bdd6ef7ba6a5615
            var el = ((IEnumerable)left).GetEnumerator();
            var er = ((IEnumerable)right).GetEnumerator();
            {
                while (el.MoveNext())
                {
                    if (!(er.MoveNext() && Comparer.Default.Compare(el.Current, er.Current) == 0)) return false;
                }
                if (er.MoveNext()) return false;
            }

            return true;
        }
    }
}
