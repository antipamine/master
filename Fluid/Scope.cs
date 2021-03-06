using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fluid.Values;

namespace Fluid
{
    public class Scope
    {
        private Dictionary<string, FluidValue> _properties;
        private readonly Scope _parent;

        public Scope()
        {
            _parent = null;
        }

        public Scope(Scope parent)
        {
            _parent = parent;
        }

        public IEnumerable<string> Properties => _properties == null ? Array.Empty<string>() : _properties.Keys;

        /// <summary>
        /// Returns the value with the specified name in the chain of scopes, or undefined
        /// if it doesn't exist.
        /// </summary>
        /// <param name="name">The name of the value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FluidValue GetValue(string name)
        {
            if (name == null)
            {
                ExceptionHelper.ThrowArgumentNullException(nameof(name));
            }

            if (_properties != null && _properties.TryGetValue(name, out var result))
            {
                return result;
            }

            return _parent != null
                ? _parent.GetValue(name)
                : NilValue.Instance;
        }

        public void Delete(string name)
        {
            if (_properties != null)
            {
                _properties.Remove(name);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(string name, FluidValue value)
        {
            _properties ??= new Dictionary<string, FluidValue>();

            _properties[name] = value;
        }

        public Scope EnterChildScope()
        {
            return new Scope(this);
        }

        public Scope ReleaseScope()
        {
            return _parent;
        }

        public FluidValue GetIndex(FluidValue index)
        {
            return GetValue(index.ToString());
        }
    }
}
