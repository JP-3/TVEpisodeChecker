using FileMover;
using System.Text;
using TMDbLib.Client;
using TVEpisodeChecker;


Email email = new Email();
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
StringBuilder stringBuilder = new StringBuilder();

foreach (var tvShow in tvShows)
{
    var fileName = Path.GetFileName(tvShow);

    if (fileName.Contains("("))
    {
        fileName = fileName.Remove(fileName.IndexOf('(')).TrimEnd();
    }

    if (!tvShowToSkip.Contains(fileName))
    {
        var tmdbShow = client.SearchTvShowAsync(fileName).Result;
        if (tmdbShow.TotalResults == 0 || fileName == ".grab")
        {
            Console.WriteLine($"{fileName} Nothing returned from TMDB");
            File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} Nothing returned from TMDB\r\n");
            stringBuilder.AppendLine($"{fileName} Nothing returned from TMDB");
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
                    stringBuilder.AppendLine($"{fileName} missing seasons folder");
                    break;
                }
            }

            var seasons = Directory.GetDirectories(tvShow);
            if (seasons.Count() != 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    var season = client.GetTvSeasonAsync(id, i + 1).Result;
                    if (season == null || season.AirDate == null || season.AirDate.Value.Date >= DateTime.Now.Date)
                    {
                        break;
                    }
                    int episodeCount = -1;
                    try
                    {
                        if (seasons[i].ToLower().Contains("season"))
                        {
                            var episodes = Directory.GetFiles(seasons[i]).Count();

                            episodeCount++;
                            for (int episode = 0; episode < season.Episodes.Count; episode++)
                            {
                                try
                                {
                                    if (season.Episodes[episode].AirDate.Value.Date <= DateTime.Now.Date)
                                    {
                                        episodeCount++;
                                    }
                                }
                                catch (Exception)
                                {
                                    if (episodeCount > episodes)
                                    {
                                        Console.WriteLine($"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes count {season.Episodes.Count}, Air count {episodeCount}");
                                        File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}, Air count {episodeCount}\r\n");
                                        stringBuilder.AppendLine($"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}, Air count {episodeCount}");
                                    }
                                    break;
                                }
                            }
                            if (episodeCount > episodes)
                            {
                                Console.WriteLine($"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes count {season.Episodes.Count}, Air count {episodeCount}");
                                File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}, Air count {episodeCount}\r\n");
                                stringBuilder.AppendLine($"{fileName} - {Path.GetFileName(seasons[i])} Episodes {episodes}, TMDB Episodes {season.Episodes.Count}, Air count {episodeCount}");
                            }
                        }
                    }
                    catch
                    {
                        if (episodeCount == -1)
                        {
                            Console.WriteLine($"{fileName} - Season {i + 1} Missing Season Folder");
                            File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - Season {i + 1} Missing Season Folder");
                            stringBuilder.AppendLine($"{fileName} - Season {i + 1} Missing Season Folder");
                        }
                        else
                        {
                            Console.WriteLine($"{fileName} - Season {i + 1} Episodes 0, TMDB Episodes {season.Episodes.Count}, Air Count {episodeCount}");
                            File.AppendAllText(data[PropertiesEnum.EpisodeCheckerLog.ToString()], $"{fileName} - Season {i + 1} Episodes 0, TMDB Episodes {season.Episodes.Count}, Air Count {episodeCount}");
                            stringBuilder.AppendLine($"{fileName} - Season {i + 1} Episodes 0, TMDB Episodes {season.Episodes.Count}, Air Count {episodeCount}");
                        }
                    }
                }
            }
        }
    }
    else
    {
        Console.WriteLine($"Skipped File - {fileName}");
    }
}

email.SendEmail(data[PropertiesEnum.gName.ToString()], data[PropertiesEnum.gKey.ToString()], "TV Check", stringBuilder.ToString());

