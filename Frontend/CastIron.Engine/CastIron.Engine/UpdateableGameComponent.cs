using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace CastIron.Engine
{
    [PublicAPI]
    public abstract class UpdateableGameComponent : IGameComponent, IUpdateable
    {
        private bool _enabled = true;
        private int _updateOrder;

        #region IUpdateable

        public abstract void Update(GameTime gameTime);

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                EnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int UpdateOrder
        {
            get => _updateOrder;
            set
            {
                if (_updateOrder == value) return;
                _updateOrder = value;
                UpdateOrderChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs>? EnabledChanged;
        public event EventHandler<EventArgs>? UpdateOrderChanged;
        #endregion

        public virtual void Initialize()
        {
        }
    }
}
