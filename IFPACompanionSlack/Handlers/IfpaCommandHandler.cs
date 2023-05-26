using ConsoleTables;
using PinballApi;
using PinballApi.Models.WPPR.v2.Players;
using PinballApi.Models.WPPR.v2.Rankings;
using SlackNet.Interaction;
using SlackNet.WebApi;
using IFPACompanionSlack.BlockBuilders;
using PinballApi.Models.WPPR.v1.Calendar;
using IFPACompanionSlack.Extensions;

namespace IfpaSlackBot.Handlers
{
    public class IfpaCommandHandler : ISlashCommandHandler
    {
        public PinballRankingApiV2 IFPAApi { get; set; }
        public PinballRankingApiV1 IFPALegacyApi { get; set; }

        public const string SlashCommand = "/ifpa";

        public List<string> Commands => new List<string> { "help", "rank", "series", "player", "tournaments" };

        public enum RankType
        {
            Main,
            Women,
            Youth,
            Country
        }

        public IfpaCommandHandler(PinballRankingApiV2 pinballRankingApi, PinballRankingApiV1 apiV1)
        {
            IFPAApi = pinballRankingApi;
            IFPALegacyApi = apiV1;
        }

        public async Task<SlashCommandResponse> Handle(SlashCommand command)
        {
            Console.WriteLine($"{command.UserName} used the {SlashCommand} slash command in the {command.ChannelName} channel");
            
            try
            {
                var tokens = command.Text.ToLower().Split(' ');
                var commandToken = tokens?.FirstOrDefault();
                var commandDetailsTokens = tokens?.Skip(1).ToArray();

                if (tokens == null || tokens.Any() == false || Commands.Contains(commandToken) == false)
                {
                    // input string does not start with a valid command
                    return new SlashCommandResponse
                    {
                        Message = new Message
                        {
                            Text = $"No valid commands found in `{command.Text}`."
                        },
                        ResponseType = ResponseType.Ephemeral
                    };
                }

                switch (commandToken)
                {
                    case "rank":
                        return await Rank(commandDetailsTokens);
                    case "player":
                        return await Player(commandDetailsTokens);
                    case "series":
                        return await Series(commandDetailsTokens);
                    case "tournaments":
                        return await Tournaments(commandDetailsTokens);
                    case "help":
                    default:
                        return await Help();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = $"Encountered an error attempting to run your command."
                    },
                    ResponseType = ResponseType.Ephemeral
                };
            }
        }

        private async Task<SlashCommandResponse> Player(string[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
            {
                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = $"You must provide a player id or player name to search."
                    },
                    ResponseType = ResponseType.Ephemeral
                };
            }

            Player playerDetails;

            if (int.TryParse(tokens[0], out var playerId) == true)
            {
                playerDetails = await IFPAApi.GetPlayer(playerId);
            }
            else
            {
                var name = string.Join(' ', tokens);
                var players = await IFPAApi.GetPlayersBySearch(new PlayerSearchFilter { Name = name });
                if (players.Results.Count > 0)
                {
                    playerDetails = await IFPAApi.GetPlayer(players.Results.First().PlayerId);
                }
                else
                {
                    return new SlashCommandResponse
                    {
                        Message = new Message
                        {
                            Text = $"Search Criteria did not find any players."
                        },
                        ResponseType = ResponseType.Ephemeral
                    };
                }
            }

            var playerTourneyResults = await IFPAApi.GetPlayerResults(playerDetails.PlayerId);

            return new SlashCommandResponse
            {
                Message = new Message
                {
                    Blocks = PlayerBlockBuilder.FromPlayer(playerDetails, playerTourneyResults)
                },
                ResponseType = ResponseType.InChannel
            };
        }

        private async Task<SlashCommandResponse> Rank(string[] tokens)
        {

            Enum.TryParse(tokens.FirstOrDefault(), true, out RankType rankType);

            var table = new ConsoleTable("Rank", "Player", "Points");
            var index = 1;

            if (rankType == RankType.Main)
            {
                var rankings = await IFPAApi.GetWpprRanking(1, 40);

                foreach (var ranking in rankings.Rankings)
                {
                    table.AddRow(index,
                                 ranking.FirstName + " " + ranking.LastName,
                                 ranking.WpprPoints.ToString("N2"));
                    index++;
                }
            }
            else if (rankType == RankType.Women)
            {
                WomensRanking womensRanking = await IFPAApi.GetRankingForWomen(TournamentType.Open, 1, 40);
                foreach (var ranking in womensRanking.Rankings)
                {
                    table.AddRow(index,
                                 ranking.FirstName + " " + ranking.LastName,
                                 ranking.WpprPoints.ToString("N2"));
                    index++;
                }
            }
            else if (rankType == RankType.Youth)
            {
                YouthRanking youthRanking = await IFPAApi.GetRankingForYouth(1, 40);
                foreach (var ranking in youthRanking.Rankings)
                {
                    table.AddRow(index,
                                 ranking.FirstName + " " + ranking.LastName,
                                 ranking.WpprPoints.ToString("N2"));
                    index++;
                }
            }
            else if (rankType == RankType.Country && tokens.Length > 1)
            {
                var countryRankings = await IFPAApi.GetRankingForCountry(tokens[1], 1, 40);

                foreach (var ranking in countryRankings.Rankings)
                {
                    table.AddRow(index,
                                 ranking.FirstName + " " + ranking.LastName,
                                 ranking.WpprPoints.ToString("N2"));
                    index++;
                }
            }

            var responseTable = table.ToMinimalString();
            responseTable = $"Top of the current IFPA {rankType} rankings\n```{responseTable.Substring(0, Math.Min(responseTable.Length, 1950))}```";

            return new SlashCommandResponse
            {
                Message = new Message
                {
                    Text = responseTable
                },
                ResponseType = ResponseType.InChannel
            };
        }


        private async Task<SlashCommandResponse> Series(string[] tokens) 
        {
            int year;
            string region;
            var seriesOptions = await IFPAApi.GetSeries();

            if (tokens == null || tokens.Length < 1 || seriesOptions.Any(n => n.Code.ToLower() == tokens.First().ToLower()) == false)
            {
                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = $"You must provide a valid series. Options are: {string.Join(' ', seriesOptions.Select(n => n.Code))}"
                    },
                    ResponseType = ResponseType.Ephemeral
                };
            }

            var series = tokens[0];

            if (tokens.Length < 2 || int.TryParse(tokens[1], out year) == false)
            {
                year = DateTime.Now.Year;
            }

            if (tokens.Length < 3)
            {               
                var seriesRanking = await IFPAApi.GetSeriesOverallStanding(series, year);

                var table = new ConsoleTable("Location", "Current Leader", "# Players", "Prize $");

                foreach (var ranking in seriesRanking.OverallResults)
                {
                    table.AddRow(ranking.RegionName,
                                 ranking.CurrentLeader.PlayerName,
                                 ranking.UniquePlayerCount,
                                 ranking.PrizeFund.ToString("c"));
                }

                var responseTableAsString = table.ToMinimalString();
                var response = $"{series.ToUpper()} IFPA standings for {year}\n\n```{responseTableAsString}```";

                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = response
                    },
                    ResponseType = ResponseType.InChannel
                };
            }
            else
            {
                region = tokens[2];

                var championshipSeries = await IFPAApi.GetSeriesStandingsForRegion(series, region, year);

                var table = new ConsoleTable("Rank", "Player", "Points", "# Events");
                table.Options.NumberAlignment = Alignment.Right;
                if (championshipSeries.Standings != null)
                {
                    foreach (var ranking in championshipSeries.Standings.Take(40))
                    {
                        table.AddRow(ranking.SeriesRank, ranking.PlayerName, ranking.WpprPoints, ranking.EventCount);
                    }
                    var responseTable = table.ToMinimalString();

                    return new SlashCommandResponse
                    {
                        Message = new Message
                        {
                            Text = $"{series.ToUpper()} IFPA standings for {year} in {region.ToUpper()}\n```{responseTable}```"
                        },
                        ResponseType = ResponseType.InChannel
                    };
                }
                else
                {
                    return new SlashCommandResponse
                    {
                        Message = new Message
                        {
                            Text = $"Region `{region}` returned no results"
                        },
                        ResponseType = ResponseType.InChannel
                    };
                }
            }
        }

        private async Task<SlashCommandResponse> Tournaments(string[] tokens)
        {
            string location;
            int radiusInMiles;

            if (tokens == null || tokens.Length < 2)
            {
                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = $"You must provide a location and radius distance in miles."
                    },
                    ResponseType = ResponseType.Ephemeral
                };
            }

            int.TryParse(tokens[0], out radiusInMiles);
            location = string.Join(' ', tokens.Skip(1));

            var tournaments = await IFPALegacyApi.GetCalendarSearch(location, radiusInMiles, DistanceUnit.Miles);

            if (tournaments.Calendar != null)
            {                
                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Blocks = TournamentBlockBuilder.FromCalendarSearch(tournaments, location, radiusInMiles)
                    },
                    ResponseType = ResponseType.InChannel
                };
            }
            else
            {
                return new SlashCommandResponse
                {
                    Message = new Message
                    {
                        Text = $"No upcoming tournaments near {location}"
                    },
                    ResponseType = ResponseType.InChannel
                };
            }
        }


        private async Task<SlashCommandResponse> Help()
        {
            var helpText = @"The following commands are available:
```rank [ranktype] [optional:countryname]
series [series] [optional:year] [optional:region]
player [IFPA ID or Player Name]
tournaments [radiusInMiles] [location]
```";

            return new SlashCommandResponse
            {
                Message = new Message
                {
                    Text = helpText
                },
                ResponseType = ResponseType.InChannel
            };
        }


    }
}
