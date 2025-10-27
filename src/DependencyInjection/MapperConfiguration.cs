namespace Simple.AutoMapper.DependencyInjection
{
    /// <summary>
    /// Configuration class for Simple.AutoMapper initialization.
    /// </summary>
    public class MapperConfiguration
    {
        /// <summary>
        /// Adds a profile containing mapping configurations.
        /// Creates an instance of <typeparamref name="TProfile"/> and executes its configuration.
        /// </summary>
        /// <typeparam name="TProfile">Profile type that inherits from <see cref="Core.Profile"/>.</typeparam>
        public MapperConfiguration AddProfile<TProfile>() where TProfile : Core.Profile, new()
        {
            Core.Mapper.AddProfile<TProfile>();
            return this;
        }

        /// <summary>
        /// Adds a profile instance containing mapping configurations.
        /// Executes the configuration of the provided profile instance.
        /// </summary>
        /// <param name="profile">Profile instance to add.</param>
        public MapperConfiguration AddProfile(Core.Profile profile)
        {
            Core.Mapper.AddProfile(profile);
            return this;
        }
    }
}
