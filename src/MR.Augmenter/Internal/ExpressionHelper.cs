using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MR.Augmenter.Internal
{
	internal static class ExpressionHelper
	{
		public static Func<T, object> CreateGet<T>(PropertyInfo propertyInfo)
		{
			Type instanceType = typeof(T);
			Type resultType = typeof(object);

			var parameterExpression = Expression.Parameter(instanceType, "instance");
			Expression resultExpression;

			var getMethod = propertyInfo.GetMethod;

			if (getMethod.IsStatic)
			{
				resultExpression = Expression.MakeMemberAccess(null, propertyInfo);
			}
			else
			{
				var readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType);

				resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
			}

			resultExpression = EnsureCastExpression(resultExpression, resultType);

			LambdaExpression lambdaExpression =
				Expression.Lambda(typeof(Func<T, object>), resultExpression, parameterExpression);

			Func<T, object> compiled = (Func<T, object>)lambdaExpression.Compile();
			return compiled;
		}

		private static Expression EnsureCastExpression(Expression expression, Type targetType, bool allowWidening = false)
		{
			var expressionType = expression.Type;
			var expressionTypeInfo = expressionType.GetTypeInfo();
			var targetTypeInfo = targetType.GetTypeInfo();

			if (expressionType == targetType ||
				(!expressionTypeInfo.IsValueType && targetTypeInfo.IsAssignableFrom(expressionTypeInfo)))
			{
				return expression;
			}

			if (targetTypeInfo.IsValueType)
			{
				Expression convert = Expression.Unbox(expression, targetType);

				if (allowWidening && targetTypeInfo.IsPrimitive)
				{
					MethodInfo toTargetTypeMethod = typeof(Convert)
						.GetRuntimeMethod("To" + targetType.Name, new[] { typeof(object) });

					if (toTargetTypeMethod != null)
					{
						convert = Expression.Condition(
							Expression.TypeIs(expression, targetType),
							convert,
							Expression.Call(toTargetTypeMethod, expression));
					}
				}

				return Expression.Condition(
					Expression.Equal(expression, Expression.Constant(null, typeof(object))),
					Expression.Default(targetType),
					convert);
			}

			return Expression.Convert(expression, targetType);
		}

		public static PropertyInfo GetSimplePropertyAccess(this LambdaExpression expression)
		{
			var memberExpression = expression.Body.RemoveConvert() as MemberExpression;
			var pi = memberExpression?.Member as PropertyInfo;
			return pi;
		}

		public static Expression RemoveConvert(this Expression expression)
		{
			while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
			{
				expression = ((UnaryExpression)expression).Operand;
			}

			return expression;
		}
	}
}
