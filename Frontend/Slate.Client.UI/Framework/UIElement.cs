using System;
using System.Collections.Generic;

namespace Slate.Client.UI.Framework
{
    public abstract class UIElement
    {
        private Dictionary<UIPropertyKey, UIPropertyValue> uiProperties = new();

        public void SetProperty<T>(UIProperty<T> property, T value)
        {
            if (!uiProperties.TryGetValue(property.Key, out var valueStorage))
            {
                valueStorage = new UIPropertyValue<T>();
                uiProperties.Add(property.Key, valueStorage);
            }

            if (valueStorage is not UIPropertyValue<T> typedStorage) throw new Exception("Stored key did not have the expected type!");
            var previousValue = typedStorage.ValueIsSet ? typedStorage.Value :
                typedStorage.DefaultIsSet ? typedStorage.DefaultValue :
                default;

            typedStorage.Value = value;

            property.Notify(this, previousValue, value);
        }

        public void ClearProperty<T>(UIProperty<T> property)
        {
            if (uiProperties.TryGetValue(property.Key, out var valueStorage))
            {
                valueStorage.ClearValue();
            }
        }

        public T GetProperty<T>(UIProperty<T> property)
        {
            if (!uiProperties.TryGetValue(property.Key, out var valueStorage))
            {
                valueStorage = new UIPropertyValue<T>();
                uiProperties.Add(property.Key, valueStorage);
            }

            if (valueStorage is not UIPropertyValue<T> typedStorage) throw new Exception("Stored key did not have the expected type!");

            if (typedStorage.ValueIsSet) return typedStorage.Value;

            if (!typedStorage.DefaultIsSet)
            {
                typedStorage.DefaultValue = property.GetDefaultValue();
                property.Notify(this, default, typedStorage.DefaultValue);
            }

            return typedStorage.DefaultValue;
        }
    }
}