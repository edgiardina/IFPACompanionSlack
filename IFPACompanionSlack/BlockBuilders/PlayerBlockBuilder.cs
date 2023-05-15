using PinballApi.Extensions;
using PinballApi.Models.WPPR.v2.Players;
using SlackNet;
using SlackNet.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFPACompanionSlack.BlockBuilders
{
    public static class PlayerBlockBuilder
    {

        public static List<Block> FromPlayer(Player player, PlayerResults playerResults)
        {
            var returnListBlock = new List<Block>
            {
                new HeaderBlock
                {
                    Text = new PlainText
                    {
                        Text =  $"{player.FirstName} {player.LastName}"
                    }
                },
                new DividerBlock(),
                new SectionBlock
                {
                    Text = new Markdown
                    {
                        Text = $"IFPA #{player.PlayerId} [{player.Initials}]\n{player.City} {player.StateProvince} {player.CountryName}   :flag-{player.CountryCode.ToLower()}:"
                    },
                    Accessory = new Image
                    {
                        ImageUrl = player.ProfilePhoto?.ToString() ?? "https://www.ifpapinball.com/images/noplayerpic.png",
                        AltText = $"profile picture for {player.FirstName} {player.LastName}"
                    }
                },
                new SectionBlock
                {
                    Fields =
                    {
                        new PlainText{ Text = "Ranking" },
                        new PlainText{ Text = $"{player.PlayerStats.CurrentWpprRank.OrdinalSuffix()}     {player.PlayerStats.CurrentWpprValue.ToString("F2")}" },
                        new PlainText{ Text = "Rating" },
                        new PlainText{ Text = $"{player.PlayerStats.RatingsRank?.OrdinalSuffix()}     {player.PlayerStats.RatingsValue?.ToString("F2")}" },
                        new PlainText{ Text = "Eff Perct" },
                        new PlainText{ Text =  $"{player.PlayerStats.EfficiencyRank?.OrdinalSuffix() ?? "Not Ranked"}      {player.PlayerStats.EfficiencyValue?.ToString("F2")}" },
                    }
                },
                new SectionBlock
                {
                    Fields =
                    {
                        new Markdown
                        {
                            Text = "*Active Tournaments*"
                        }
                    }
                }
            };

            foreach (var result in playerResults.Results)
            {
                returnListBlock.Add(
                    new SectionBlock
                    {
                        Fields = {
                            new Markdown { Text = $":flag-{result.CountryCode.ToLower()}:   {result.TournamentName}" },
                            new PlainText { Text = $"{result.Position.OrdinalSuffix()}   {result.CurrentPoints.ToString("F2")}" },
                        }
                    }
                );
            }

            returnListBlock.Add(new DividerBlock());
            if (player.IfpaRegistered)
            {
                returnListBlock.Add(
                    new ContextBlock
                    {
                        Elements =
                        {
                            new Image 
                            { 
                                ImageUrl = "https://www.ifpapinball.com/images/confirmed.png",
                                AltText = "checkmark to indicate player is IFPA registered"
                            },
                            new PlainText { Text = "IFPA Registered" },
                            new Markdown { Text = $"<https://www.ifpapinball.com/player.php?p={player.PlayerId}|View on IFPA website>" }
                        }
                    }
                );
            }
            else
            {
                returnListBlock.Add(
                    new ContextBlock
                    {
                        Elements =
                        {
                            new Markdown {  Text = $"<https://www.ifpapinball.com/player.php?p={player.PlayerId}|View on IFPA website>" }
                        }
                    }
                );
            }

            return returnListBlock;
        }

    }
}
