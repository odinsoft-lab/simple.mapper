namespace Simple.AutoMapper.Tests
{
    // Minimal shims to satisfy tests without referencing Simple.AutoMapper package
    public abstract class MapperSpecBase
    {
        protected abstract MapperConfiguration CreateConfiguration();
    }

    public sealed class MapperConfiguration
    {
        public MapperConfiguration(Action<IProfileExpression> configure)
        {
            // Execute the provided configuration action on a dummy profile expression
            configure?.Invoke(new ProfileExpression());
        }
    }

    public interface IProfileExpression
    {
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
    }

    public interface IMappingExpression<TSource, TDestination>
    {
        IMappingExpression<TDestination, TSource> ReverseMap();
    }

    internal sealed class ProfileExpression : IProfileExpression
    {
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return new MappingExpression<TSource, TDestination>();
        }
    }

    internal sealed class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
    {
        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            return new MappingExpression<TDestination, TSource>();
        }
    }
}