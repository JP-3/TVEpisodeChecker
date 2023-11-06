using FileMover;
using TMDbLib.Client;

Dictionary<string, string> data = new Dictionary<string, string>();

foreach (var row in File.ReadAllLines(@"C:\\Plex\key.txt"))
{
    data.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
}

List<string> tvShowToSkip = new List<string>();
foreach (var row in File.ReadAllLines(@"C:\\Plex\TVShowsToSkip.txt"))
{
    tvShowToSkip.Add(row);
}

var tvShows = Directory.GetDirectories(data[PropertiesEnum.TV.ToString()]);
TMDbClient client = new TMDbClient(data[PropertiesEnum.apiKey.ToString()]);

foreach (var tvShow in tvShows)
{
    var fileName = Path.GetFileName(tvShow);
    if (!tvShowToSkip.Contains(fileName))
    {
        var tmdbShow = client.SearchTvShowAsync(fileName).Result;
        if (tmdbShow.TotalResults == 0 || fileName == ".grab")
        {
            Console.WriteLine($"{fileName} Nothing returned from TMDB");
            File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} Nothing returned from TMDB\r\n");
        }
        else
        {
            var id = tmdbShow.Results.FirstOrDefault().Id;
            foreach (var file in Directory.GetFiles(tvShow))
            {
                if (!file.ToLower().Contains("season"))
                {
                    Console.WriteLine($"{fileName} missing seasons folder");
                    File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} missing seasons folder\r\n");
                    break;
                }
            }

            var seasons = Directory.GetDirectories(tvShow);
            for (int i = 0; i < 50; i++)
            {
                var season = client.GetTvSeasonAsync(id, i + 1).Result;
                if (season == null || season.AirDate == null || season.AirDate > DateTime.Now)
                {
                    break;
                }

                try
                {
                    if (seasons[i].ToLower().Contains("season"))
                    {
                        var episodes = Directory.GetFiles(seasons[i]).Count();
                        if (season.Episodes.Count > episodes)
                        {
                            Console.WriteLine($"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}");
                            File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}\r\n");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"{fileName} - Season {i + 1} Episodes 0, TMDB Episodes {season.Episodes.Count}");
                    File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - Season {i + 1} Episodes 0, TMDB Episodes {season.Episodes.Count}\r\n");

                }
            }
        }
    }
    else
    {
        Console.WriteLine($"Skipped File - {fileName}");
    }
}