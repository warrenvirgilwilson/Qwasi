using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GeneralMath.Expressions
{
    public interface IBasicExpression<TExpression> : IMathExpression<TExpression>
        where TExpression : class, IBasicExpression<TExpression>
    {
        public static TExpression Addition(TExpression a, TExpression b)
        {
            IEnumerable<TExpression> aTerms = null;
            IEnumerable<TExpression> bTerms = null;

            if (a is IMultitermOperation<TExpression, TExpression> aSum && aSum.OpName == "sum")
                aTerms = aSum.Terms;
            else if (a is IBinaryOperation<TExpression, TExpression, TExpression> aAdd && aAdd.OpName == "add")
                aTerms = new TExpression[] { aAdd.A, aAdd.B };

            if (b is IMultitermOperation<TExpression, TExpression> bSum && bSum.OpName == "sum")
                bTerms = bSum.Terms;
            else if (b is IBinaryOperation<TExpression, TExpression, TExpression> bAdd && bAdd.OpName == "add")
                bTerms = new TExpression[] { bAdd.A, bAdd.B };

            if (aTerms == null && bTerms == null)
                return a.NewBinaryOperation(a.AsExpressionType(), b, (v1, v2) => v1.Add(v2), "add", "+");

            return Sum(aTerms == null ? bTerms.Prepend(a) : aTerms.Append(b));
        }

        public static TExpression Subtraction(TExpression a, TExpression b)
        {
            return a.NewBinaryOperation(a.AsExpressionType(), b, (v1, v2) => v1.SubtractBy(v2), "subtract", "-");
        }

        public static TExpression Negation(TExpression value)
        {
            return value.NewUnaryOperation(value.AsExpressionType(), v => v.Negate(), "negate", "-");
        }

        public static TExpression Sum(params TExpression[] terms) => Sum(terms);
        public static TExpression Sum(IEnumerable<TExpression> terms)
        {
            if (!terms.Any())
                return null;

            TExpression e = terms.First();

            return e.NewMultitermOperation(terms, e.SumOperationFunction, "sum", "+");
        }

        public static TExpression operator +(
            IBasicExpression<TExpression> a,
            IBasicExpression<TExpression> b)
            => a.Add(b.AsExpressionType());

        public static TExpression operator -(
            IBasicExpression<TExpression> a,
            IBasicExpression<TExpression> b)
            => a.SubtractBy(b.AsExpressionType());

        public static TExpression operator -(
            IBasicExpression<TExpression> value)
            => value.Negate();

        public TExpression Add(TExpression other) => Addition(this.AsExpressionType(), other);
        public TExpression SubtractBy(TExpression other) => Subtraction(this.AsExpressionType(), other);
        public TExpression Negate() => Negation(this.AsExpressionType());

        protected TExpression SumOperationFunction(IEnumerable<TExpression> terms)
        {
            var valueTerms = terms.OfType<IBasicValue<TExpression>>().ToArray();
            var remainderTerms = terms.Except(valueTerms.Cast<TExpression>()).ToArray();

            if (!valueTerms.Any())
                return Sum(terms);

            TExpression valueSum = valueTerms.First().ValueSum(valueTerms.Cast<TExpression>());

            if (remainderTerms.Length == 0)
                return valueSum;
            else if (remainderTerms.Length == 1)
                return valueSum.Add(remainderTerms.First());

            return Sum(remainderTerms.Prepend(valueSum));
        }
    }

    public interface IBasicValue<TExpression> : IBasicExpression<TExpression>, IValueExpression<TExpression>
        where TExpression : class, IBasicExpression<TExpression>
    {
        TExpression ValueSum(IEnumerable<TExpression> terms)
        {
            if (!terms.Any())
                return null;

            TExpression sum = null;
            foreach (TExpression term in terms)
                sum = (sum == null) ? term : sum.Add(term);

            return sum;
        }
    }
}
