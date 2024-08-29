using Moq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace SkytearHorde.Tests.Utils
{
    /// <summary>
    /// Use this class to Mock any item which is built by the Umbraco Models Builder.  This is required
    /// because by default the Models Builder item calls several extension methods of the IPublishedProperty & IPublishedContent.
    /// This makes life way easy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelsBuilderMock<T> where T : IPublishedElement
    {
        private readonly Mock<IPublishedContent> _mockedContent;

        public T Mock { get; }

        public Mock<IPublishedContent> MockedContent { get { return _mockedContent; } }

        public ModelsBuilderMock()
        {
            _mockedContent = new Mock<IPublishedContent>();
            Mock = CreateObjectWithAnyConstructor<T>(_mockedContent.Object, new Mock<IPublishedValueFallback>().Object);
            _mockedContent.Setup(x => x.ContentType.Alias).Returns(typeof(T).UnderlyingSystemType.GetField("ModelTypeAlias", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null).ToString());
        }

        public void SetProperty<TResult>(Expression<Func<T, TResult>> expression, TResult value)
        {

            var propertyRes = GetExpressionText(expression);

            if (propertyRes.IsGenericProperty)
            {
                //The Models Builder actual umbraco Alias are in the formate of, for e.g. 'metaRobots', or 'age'.
                var mockedProperty = new Mock<IPublishedProperty>();

                //This is because the extension method GetPropertyValue from the PublishedContentExtensions
                //_mockedContent.Setup(x => x.GetProperty(propertyRes.PropertyName, false)).Returns(mockedProperty.Object);
                _mockedContent.Setup(x => x.GetProperty(propertyRes.PropertyName)).Returns(mockedProperty.Object);
                mockedProperty.SetupAllProperties();
                mockedProperty.Setup(x => x.GetValue(null, null)).Returns(value);
                mockedProperty.Setup(x => x.HasValue(null, null)).Returns(true);
            }
            else
            {
                var convertedExpression = ToUntypedPropertyExpression<IPublishedContent, TResult>(expression, propertyRes.PropertyName);
                this.MockedContent.Setup<TResult>(convertedExpression).Returns((TResult)value);
            }

        }

        private static Expression<Func<TRet, TResult>> ToUntypedPropertyExpression<TRet, TResult>(Expression<Func<T, TResult>> expression, string memberName)
        {
            var param = Expression.Parameter(typeof(TRet));
            var field = Expression.Property(param, memberName);
            return Expression.Lambda<Func<TRet, TResult>>(field, param);
        }

        private static GetPropertyResult GetExpressionText<TModel, TRes>(Expression<Func<TModel, TRes>> selector)
        {
            var propName = GetPropertyName(selector);
            try
            {
                var pInfo = typeof(TModel).GetProperty(propName).GetCustomAttribute<ImplementPropertyTypeAttribute>();
                return new GetPropertyResult()
                {
                    IsGenericProperty = true,
                    PropertyName = pInfo.Alias
                };
            }
            catch (Exception e)
            {
                return new GetPropertyResult()
                {
                    IsGenericProperty = false,
                    PropertyName = propName

                };
            }
        }

        private static string GetPropertyName<TModel, TRes>(Expression<Func<TModel, TRes>> expression)
        {
            var currentExpression = expression.Body;

            while (true)
            {
                switch (currentExpression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return ((ParameterExpression)currentExpression).Name;
                    case ExpressionType.MemberAccess:
                        return ((MemberExpression)currentExpression).Member.Name;
                    case ExpressionType.Call:
                        return ((MethodCallExpression)currentExpression).Method.Name;
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        currentExpression = ((UnaryExpression)currentExpression).Operand;
                        break;
                    case ExpressionType.Invoke:
                        currentExpression = ((InvocationExpression)currentExpression).Expression;
                        break;
                    case ExpressionType.ArrayLength:
                        return "Length";
                    default:
                        throw new Exception("Not a proper member selector");
                }
            }
        }

        //Ideally place these static methods in a separate file
        private static T CreateObjectWithAnyConstructor<TMod>(params object[] parameters)
        {
            return (T)CreateObjectWithAnyConstructor(typeof(TMod), parameters);

        }
        private static object CreateObjectWithAnyConstructor(Type typeToCreate, params object[] parameters)
        {
            ConstructorInfo constructor = null;

            try
            {
                // Binding flags exclude public constructors.
                constructor =
                    typeToCreate.GetConstructors()
                        .Where(x => x.GetParameters().Length == parameters.Length)
                        .FirstOrDefault();
                //constructor = typeToCreate.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
            }
            catch (Exception)
            {
                throw;
            }

            if (constructor == null || constructor.IsAssembly)
                // Also exclude internal constructors.
                throw new InvalidOperationException(string.Format("A private or " + "protected constructor is missing for '{0}'.", typeToCreate.Name));
            object _instance = null;
            try
            {
                _instance = constructor.Invoke(parameters);
            }
            catch (MemberAccessException ex)
            {
                var m = Regex.Match(ex.Message, "Cannot create an instance of (.*?)Factory because it is an abstract class.");
                if (m.Success)
                {
                    string factoryName = m.Groups[1].Value + "Factory";
                    string msg = ex.Message +
                                 " - If you are using this factory, make sure you mark it as 'UsedInProject' in the builder";
                    throw new InvalidOperationException(msg);
                }
                else
                {
                    throw;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return _instance;
        }

        private class GetPropertyResult
        {
            public bool IsGenericProperty { get; set; }
            public string PropertyName { get; set; }
        }

    }
}
