﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Normaleezie
{
    public class Normalizer
    {
        public virtual List<List<List<object>>> Normalize<T>(List<T> denormalizedList)
        {
            if (null == denormalizedList)
            {
                return new List<List<List<object>>>();
            }

            return ConvertToNormalizedForm(denormalizedList);
        }

        internal virtual List<List<List<object>>> ConvertToNormalizedForm<T>(List<T> denormalizedList)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            List<List<object>> normalizedDataList = CreateNormalizedDataList(denormalizedList);
            List<List<object>> normalizedStructureList = CreateNormalizedStructureList(denormalizedList, normalizedDataList);

            return new List<List<List<object>>>() {
                normalizedDataList, normalizedStructureList
            };
        }

        internal virtual List<List<object>> CreateNormalizedDataList<T>(List<T> denormalizedList)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            List<List<object>> normalizedDataList = GetNormalizedDataForList(denormalizedList);

            normalizedDataList.Sort((a, b) => string.CompareOrdinal(a[0].ToString(), b[0].ToString()));

            return normalizedDataList;
        }

        internal virtual List<List<object>> GetNormalizedDataForProperty<T>(List<T> denormalizedList, PropertyInfo property, string propertyNamePrefix = null)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            if (null == property)
            {
                throw new ArgumentException(nameof(property) + " must not be null.", nameof(property));
            }

            if (!IsSimpleType(property.PropertyType))
            {
                if (null != property.PropertyType.GetInterface("IEnumerable") && property.PropertyType != typeof(string))
                {
                    return GetNormalizedDataForListProperty(denormalizedList, property, propertyNamePrefix);
                }

                return GetNormalizedDataForComplexProperty(denormalizedList, property, propertyNamePrefix);
            }

            return GetNormalizedDataForSimpleProperty(denormalizedList, property, propertyNamePrefix);
        }

        internal virtual List<List<object>> GetNormalizedDataForListProperty<T>(List<T> denormalizedList, PropertyInfo property, string propertyNamePrefix = null)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }
            
            if (null == property)
            {
                throw new ArgumentException(nameof(property) + " must not be null.", nameof(property));
            }
            
            List<object> childList = denormalizedList.Select(t => Convert.ChangeType(property.GetValue(t, null), property.PropertyType)).ToList();
            string subPropertyNamePrefix = propertyNamePrefix + property.Name + "~";
            
            childList = childList.SelectMany(i => (IEnumerable<object>)i).ToList();

            return GetNormalizedDataForList(childList, subPropertyNamePrefix);
        }

        internal virtual List<List<object>> GetNormalizedDataForList<T>(List<T> denormalizedList, string propertyNamePrefix = null)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            object firstItem = denormalizedList.FirstOrDefault(o => o != null);

            if (firstItem == null)
            {
                return new List<List<object>>() { new List<object>() { propertyNamePrefix, null} };
            }

            List<List<object>> normalizedDataForList = new List<List<object>>();

            foreach (var subProperty in firstItem.GetType().GetProperties())
            {
                normalizedDataForList.AddRange(GetNormalizedDataForProperty(denormalizedList, subProperty, propertyNamePrefix));
            }

            return normalizedDataForList;
        }

        internal virtual List<List<object>> GetNormalizedDataForComplexProperty<T>(List<T> denormalizedList, PropertyInfo property, string propertyNamePrefix = null)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            if (null == property)
            {
                throw new ArgumentException(nameof(property) + " must not be null.", nameof(property));
            }

            List<List<object>> normalizedDataForProperty = new List<List<object>>();
            List<object> childList = denormalizedList.Select(t => Convert.ChangeType(property.GetValue(t, null), property.PropertyType)).ToList();
            string subPropertyNamePrefix = propertyNamePrefix + property.Name + ".";

            foreach (var subProperty in property.PropertyType.GetProperties())
            {
                normalizedDataForProperty.AddRange(GetNormalizedDataForProperty(childList, subProperty, subPropertyNamePrefix));
            }

            return normalizedDataForProperty;
        }

        internal virtual List<List<object>> GetNormalizedDataForSimpleProperty<T>(List<T> denormalizedList, PropertyInfo property, string propertyNamePrefix = null)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            if (null == property)
            {
                throw new ArgumentException(nameof(property) + " must not be null.", nameof(property));
            }

            List<object> normalizedPropertyData = new List<object>
            {
                string.Join(string.Empty, propertyNamePrefix, property.Name)
            };

            List<object> uniquePropertyValues = GetUniquePropertyValues(denormalizedList, property);

            if (uniquePropertyValues.Count < denormalizedList.Count)
            {
                normalizedPropertyData.AddRange(uniquePropertyValues);
            }

            return new List<List<object>>() { normalizedPropertyData };
        }

        internal virtual bool IsSimpleType(Type type)
        {
            if (null == type)
            {
                throw new ArgumentException(nameof(type) + " must not be null.", nameof(type));
            }

            return Type.GetTypeCode(type) != TypeCode.Object;
        } 

        internal virtual List<object> GetUniquePropertyValues<T>(List<T> objects, PropertyInfo property)
        {
            if (null == objects)
            {
                throw new ArgumentException(nameof(objects) + " must not be null.", nameof(objects));
            }

            if (null == property)
            {
                throw new ArgumentException(nameof(property) + " must not be null.", nameof(property));
            }

            return objects.Select(t => Convert.ChangeType(property.GetValue(t, null), property.PropertyType))
                .Distinct()
                .ToList();
        }

        internal virtual List<List<object>> CreateNormalizedStructureList<T>(List<T> denormalizedList
            , List<List<object>> normalizedDataList)
        {
            if (null == denormalizedList)
            {
                throw new ArgumentException(nameof(denormalizedList) + " must not be null.", nameof(denormalizedList));
            }

            if (null == normalizedDataList)
            {
                throw new ArgumentException(nameof(normalizedDataList) + " must not be null.", nameof(normalizedDataList));
            }

            return denormalizedList.Select(denormalizedItem => CreateNormalizedStructureItem(denormalizedItem, normalizedDataList)).ToList();
        }

        internal virtual List<object> CreateNormalizedStructureItem<T>(T denormalizedItem
            , List<List<object>> normalizedDataList)
        {
            if (null == denormalizedItem)
            {
                throw new ArgumentException(nameof(denormalizedItem) + " must not be null.", nameof(denormalizedItem));
            }

            if (null == normalizedDataList)
            {
                throw new ArgumentException(nameof(normalizedDataList) + " must not be null.", nameof(normalizedDataList));
            }

            return normalizedDataList.Select(normalizedPropertyData => GetNormalizedItemPropertyObject(denormalizedItem, normalizedPropertyData)).ToList();
        }

        internal virtual object GetNormalizedItemPropertyObject<T>(T denormalizedItem, List<object> normalizedPropertyData)
        {
            if (null == denormalizedItem)
            {
                throw new ArgumentException(nameof(denormalizedItem) + " must not be null.", nameof(denormalizedItem));
            }

            if (null == normalizedPropertyData)
            {
                throw new ArgumentException(nameof(normalizedPropertyData) + " must not be null.", nameof(normalizedPropertyData));
            }

            string propName = (string)normalizedPropertyData[0];
            List<object> subPropertyNormalizedPropertyData = null;

            if (propName.Contains('.'))
            {
                string[] propertyNameParts = propName.Split('.');
                propName = propertyNameParts[0];
                string subPropName = string.Join(".", propertyNameParts.Skip(1));

                subPropertyNormalizedPropertyData = new List<object>() { subPropName };

                subPropertyNormalizedPropertyData.AddRange(normalizedPropertyData.Skip(1));
            }

            PropertyInfo propInfo = denormalizedItem.GetType().GetProperty(propName);
            object val = propInfo.GetValue(denormalizedItem, null);

            if (null != subPropertyNormalizedPropertyData)
            {
                return GetNormalizedItemPropertyObject(val, subPropertyNormalizedPropertyData);
            }

            if (1 == normalizedPropertyData.Count)
            {
                return val;
            }

            int position = normalizedPropertyData.IndexOf(val);

            if (-1 == position)
            {
                throw new InvalidOperationException(val.ToString() + " is missing from the denormalized data list.");
            }

            return position;
        }
    }
}
