using System.Linq.Expressions;

namespace API.Services
{
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> New<T>(bool value = true) => f => value;

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);
            var rightVisitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        private class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ReferenceEquals(node, _oldParameter) ? _newParameter : base.VisitParameter(node);
            }
        }
    }
}