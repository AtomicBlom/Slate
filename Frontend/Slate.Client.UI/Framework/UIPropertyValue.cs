using System;

namespace Slate.Client.UI.Framework
{
    internal abstract record UIPropertyValue
    {
        public abstract void ClearValue();
        internal bool ValueIsSet { get; private protected set; }
        internal bool DefaultIsSet { get; private protected set; }
    }

    internal record UIPropertyValue<T> : UIPropertyValue
    {
        private T _value = default!;
        private T _defaultValue = default!;
        
        internal T Value
        {
            get => ValueIsSet ? _value! : throw new Exception("Attempted to get a UIPropertyValue without first checking if the Value is set");
            set
            {
                _value = value;
                ValueIsSet = true;
            }
        }

        internal T DefaultValue
        {
            get => DefaultIsSet ? _defaultValue! : throw new Exception("Attempted to get a UIPropertyValue default without first checking if it was set");
            set
            {
                _defaultValue = value;
                DefaultIsSet = true;
            }
        }

        public override void ClearValue()
        {
            ValueIsSet = false;
            _value = default!;
        }
    };
}