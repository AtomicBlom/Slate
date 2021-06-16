﻿using System;
using System.Globalization;
using EmptyKeys.UserInterface.Data;

namespace Slate.Client.UI.Common.Converters
{
    public class GuidToStringConverter : IValueConverter
    {
        public object Convert(object value, Type target, object parameter, CultureInfo culture)
        {
            if (value is Guid guid)
            {
                return guid.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type target, object parameter, CultureInfo culture)
        {
            if (value is string guidAsString)
            {
                return Guid.Parse(guidAsString);
            }

            return null;
        }
    }
}