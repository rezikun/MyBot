using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace MyBot
{
    public class Search
    {
        public class MovieBot
        {
            public static async Task<SearchContainerWithId<SearchMovie>> SearchMovieByID(int MovieID)
            {
                TMDbClient client = new TMDbClient("f345648a16265462c06f2d0136d0c9bd");
                var ListofMovies = client.GetGenreMoviesAsync(MovieID).Result;
                return ListofMovies;
            }
        }
    }
}