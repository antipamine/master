using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Fluid.Values
{
    public sealed class DictionaryValue : FluidValue
    {
        private readonly IFluidIndexable _value;

        public DictionaryValue(IFluidIndexable value)
        {
            _value = value;
        }

        public override FluidValues Type => FluidValues.Dictionary;

        public override bool Equals(FluidValue other)
        {
            if (other.IsNil())
            {
                return _value.Count == 0;
            }

            if (other is DictionaryValue otherDictionary)
            {
                if (_value.Count != otherDictionary._value.Count)
                {
                    return false;
                }

                foreach (var key in _value.Keys)
                {
                    if (!otherDictionary._value.TryGetValue(key, out var otherItem))
                    {
                        return false;
                    }

                    _value.TryGetValue(key, out var item);

                    if (!item.Equals(otherItem))
                    {
                        return false;
                    }
                }
            }
            else if (other.Type == FluidValues.Empty)
            {
                return _value.Count == 0;
            }

            return false;
        }

        public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
        {
            if (name == "size")
            {
                return new ValueTask<FluidValue>(NumberValue.Create(_value.Count));
            }

            object value = null;

            // NOTE: This code block doesn't seem to make any sense for a dictionary, just keeping it
            // as a comment in case it breaks something at some point.

            //var accessor = context.MemberAccessStrategy.GetAccessor(_value.GetType(), name);

            //if (accessor != null)
            //{
            //    if (accessor is IAsyncMemberAccessor asyncAccessor)
            //    {
            //        value = await asyncAccessor.GetAsync(_value,  context);
            //    }
            //    else
            //    {
            //        value = accessor.Get(_value,  context);
            //    }
            //}

            if (value == null)
            {
                if (!_value.TryGetValue(name, out var fluidValue))
                {
                    return new ValueTask<FluidValue>(NilValue.Instance);
                }

                return new ValueTask<FluidValue>(fluidValue);
            }

            return new ValueTask<FluidValue>(FluidValue.Create(value, context.Options));
        }

        protected override FluidValue GetIndex(FluidValue index, TemplateContext context)
        {
            var name = index.ToStringValue();

            if (!_value.TryGetValue(name, out var value))
            {
                return NilValue.Instance;
            }

            return value;
        }

        public override bool ToBooleanValue()
        {
            return true;
        }

        public override decimal ToNumberValue()
        {
            return 0;
        }

        public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        {
        }

        public override string ToStringValue()
        {
            return "";
        }

        public override object ToObjectValue()
        {
            return _value;
        }

        public override bool Contains(FluidValue value)
        {
            foreach (var key in _value.Keys)
            {
                if (_value.TryGetValue(key, out var item) && item.Equals(value.ToObjectValue()))
                {
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<FluidValue> Enumerate()
        {
            foreach (var key in _value.Keys)
            {
                _value.TryGetValue(key, out var value);
                yield return new ArrayValue(new[] { new StringValue(key),  value });
            }
        }

        internal override string[] ToStringArray()
        {
            var array = new string[_value.Count];
            var i = 0;
            foreach (var key in _value.Keys)
            {
                _value.TryGetValue(key, out var value);
                array[i] = string.Join("", key, value.ToStringValue());
                i++;
            }

            return array;
        }

        internal override List<FluidValue> ToList()
        {
            var list = new List<FluidValue>(_value.Count);
            foreach (var key in _value.Keys)
            {
                _value.TryGetValue(key, out var value);
                list.Add(new ArrayValue(new[] { new StringValue(key), value }));
            }

            return list;
        }

        public override bool Equals(object other)
        {
            // The is operator will return false if null
            if (other is DictionaryValue otherValue)
            {
                return _value.Equals(otherValue._value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
