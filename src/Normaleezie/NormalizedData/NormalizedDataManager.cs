﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Normaleezie.Helpers;

namespace Normaleezie.NormalizedData
{
    internal class NormalizedDataManager
    {
        private readonly ReflectionHelper _reflectionHelper;

        internal NormalizedDataManager() : this(new ReflectionHelper()) {}

        internal NormalizedDataManager(ReflectionHelper reflectionHelper)
        {
            this._reflectionHelper = reflectionHelper;
        }

        internal virtual List<List<object>> CreateNormalizedData<T>(List<T> denormalizedList, List<string> previousDataNames = null, string dataName = "")
        {
            if (null == denormalizedList)
            {
                throw new ArgumentNullException(nameof(denormalizedList));
            }

            if (!denormalizedList.Any())
            {
                return new List<List<object>>() { new List<object>() { dataName } };
            }

            CheckForCircularReferences(ref previousDataNames, dataName);

            if (_reflectionHelper.IsIEnumerableType(typeof(T)))
            {
                return CreateNormalizedDataForListOfIEnumerableType(denormalizedList, previousDataNames, dataName);
            }

            if (!_reflectionHelper.IsSimpleType(typeof(T)))
            {
                return CreateNormalizedDataForListOfComplexType(denormalizedList, previousDataNames, dataName);
            }

            return CreateNormalizedDataForListOfSimpleType(denormalizedList, dataName);
        }

        internal virtual List<List<object>> CreateNormalizedDataForListOfSimpleType<T>(List<T> denormalizedList, string dataName)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentNullException(nameof(denormalizedList));
            }

            List<object> normalizedData = new List<object>() {dataName};
            List<object> uniqueValues = denormalizedList.Select(i => (object) i).Distinct().ToList();

            if (uniqueValues.Count < denormalizedList.Count)
            {
                normalizedData.AddRange(uniqueValues);
            }

            return new List<List<object>>() {normalizedData};
        }

        internal virtual List<List<object>> CreateNormalizedDataForListOfComplexType<T>(List<T> denormalizedList, List<string> previousDataNames, string dataName)
        {
            List<List<object>> normalizedDataByProperty = new List<List<object>>();

            List<PropertyInfo> tProperties = typeof (T).GetProperties().ToList();

            tProperties.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            foreach (PropertyInfo property in tProperties)
            {
                List<object> propertyValues = denormalizedList.Select(t =>
                    Convert.ChangeType(property.GetValue(t, null), property.PropertyType)).ToList();
                
                normalizedDataByProperty.AddRange(CallCreateNormalizedDataGenerically(propertyValues, previousDataNames, property.Name, property.PropertyType));
            }

            if (string.IsNullOrEmpty(dataName))
            {
                return normalizedDataByProperty;
            }

            List<object> normalizedData = new List<object>() {dataName + "."};

            normalizedData.AddRange(normalizedDataByProperty);

            return new List<List<object>>() {normalizedData};
        }

        internal virtual List<List<object>> CreateNormalizedDataForListOfIEnumerableType<T>(List<T> denormalizedList, List<string> previousDataNames, string dataName)
        {
            List<object> listValues = denormalizedList.SelectMany(i => (IEnumerable<object>) i).ToList();

            List<object> normalizedDataForList = new List<object>() {dataName + "~"};

            normalizedDataForList.AddRange(CallCreateNormalizedDataGenerically(listValues, previousDataNames, null, typeof(T).GetGenericArguments().First()));

            return new List<List<object>>() {normalizedDataForList};
        }
        
        internal virtual List<List<object>> CallCreateNormalizedDataGenerically(List<object> denormalizedList, List<string> previousDataNames, string dataName, Type type)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type reflectionHelperType = _reflectionHelper.GetType();
            MethodInfo typelessConvertListMethod = reflectionHelperType.GetMethod("ConvertList", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericConvertListMethod = typelessConvertListMethod.MakeGenericMethod(type);

            var typedDenormalizedList = genericConvertListMethod.Invoke(_reflectionHelper, new object[] { denormalizedList });

            Type normalizedDataManagerType = this.GetType();
            MethodInfo typelessCreateNormalizedData = normalizedDataManagerType.GetMethod("CreateNormalizedData", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo genericCreateNormalizedData = typelessCreateNormalizedData.MakeGenericMethod(type);

            return (List<List<object>>)genericCreateNormalizedData.Invoke(this, new[] { typedDenormalizedList, previousDataNames, dataName });
        }

        internal virtual void CheckForCircularReferences(ref List<string> previousDataNames, string dataName)
        {
            if (previousDataNames == null)
            {
                previousDataNames = new List<string>();
            }

            previousDataNames.Add(dataName);

            int largestNumberOfCallsToCreateNormalizeData = previousDataNames
                .GroupBy(previousDataName => previousDataName)
                .OrderByDescending(grouping => grouping.Count())
                .Select(grouping => grouping.Count())
                .First();

            if (largestNumberOfCallsToCreateNormalizeData > 20)
            {
                throw new Exception("Circular Reference Detected in object.");
            }
        }
    }
}
