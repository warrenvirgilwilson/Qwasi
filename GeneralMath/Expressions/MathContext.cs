using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GeneralMath.Expressions
{
    public class MathContext : IEnumerable<IdentifierObjectPair>
    {
        private IdentifierKeyedDictionary<object> _Objects = new IdentifierKeyedDictionary<object>();

        public object this[object key]
        {
            get { return _Objects.ContainsKey(key) ? _Objects[key] : null; }
            set { _Objects[key] = value; }
        }

        public object this[params object[] keyList]
        {
            get { return this[(IEnumerable<object>)keyList]; }
            set { this[(IEnumerable<object>)keyList] = value; }
        }

        public IEnumerator<IdentifierObjectPair> GetEnumerator()
        {
            return _Objects.Select(kvp => new IdentifierObjectPair(kvp.Key, kvp.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class IdentifierKeyedDictionary<TObject> : Dictionary<object, TObject>
    {
        private static MathIdentifierEqualityComparer s_EqualityComparer = new MathIdentifierEqualityComparer();

        public IdentifierKeyedDictionary()
            : base(s_EqualityComparer)
        {
        }
    }

    public class MathIdentifierEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            IEnumerable<object> xEnumerable = x as IEnumerable<object>;
            IEnumerable<object> yEnumerable = y as IEnumerable<object>;

            if (xEnumerable == null && yEnumerable == null)
                return x.Equals(y);

            if (xEnumerable == null || yEnumerable == null)
                return false;

            return xEnumerable.SequenceEqual(yEnumerable);
        }

        public int GetHashCode(object obj)
        {
            IEnumerable<object> objEnumerable = obj as IEnumerable<object>;

            if (objEnumerable == null)
                return obj.GetHashCode();

            int hashCode = 0;

            foreach (object item in objEnumerable)
                unchecked { hashCode += item.GetHashCode(); }

            return hashCode;
        }
    }

    public struct IdentifierObjectPair
    {
        public object Identifier { get; private set; }
        public object Object { get; private set; }

        public IdentifierObjectPair(object identifier, object obj)
        {
            this.Identifier = identifier;
            this.Object = obj;
        }
    }
}
