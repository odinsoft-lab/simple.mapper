using Simple.AutoMapper.Interfaces;

namespace Simple.AutoMapper.Core
{
    /// <summary>
    /// Base class for organizing mapping configurations.
    /// Inherit from this class and override Configure() to define mappings.
    /// </summary>
    public abstract class Profile
    {
        /// <summary>
        /// Override this method to configure mappings.
        /// </summary>
        protected abstract void Configure();

        /// <summary>
        /// Creates a mapping configuration between source and destination types.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type. Must have parameterless constructor.</typeparam>
        /// <returns>Mapping expression to configure the map.</returns>
        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            where TDestination : new()
        {
            return Mapper.CreateMap<TSource, TDestination>();
        }

        /// <summary>
        /// Internal method called by Mapper.AddProfile to execute configuration.
        /// </summary>
        internal void ExecuteConfigure()
        {
            Configure();
        }
    }
}
