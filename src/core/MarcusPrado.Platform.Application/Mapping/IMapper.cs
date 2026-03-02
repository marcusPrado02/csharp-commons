namespace MarcusPrado.Platform.Application.Mapping;

/// <summary>Maps a source object of type <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.</summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>Maps <paramref name="source"/> to an instance of <typeparamref name="TDestination"/>.</summary>
    TDestination Map(TSource source);
}
