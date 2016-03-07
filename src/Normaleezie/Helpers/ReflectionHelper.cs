﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Normaleezie.Helpers
{
    internal class ReflectionHelper
    {
        internal virtual List<T> ConvertList<T>(List<object> list)
        {
            if (null == list)
            {
                throw new ArgumentException(nameof(list) + " must not be null.", nameof(list));
            }

            return list.Cast<T>().ToList();
        }

        internal virtual bool IsSimpleType(Type type)
        {
            if (null == type)
            {
                throw new ArgumentException(nameof(type) + " must not be null.", nameof(type));
            }

            return Type.GetTypeCode(type) != TypeCode.Object;
        }
    }
}