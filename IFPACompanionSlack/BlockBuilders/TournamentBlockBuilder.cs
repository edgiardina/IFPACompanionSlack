using PinballApi.Models.MatchPlay;
using PinballApi.Models.WPPR.v1.Calendar;
using SlackNet.Blocks;

namespace IFPACompanionSlack.BlockBuilders
{
    public static class TournamentBlockBuilder
    {

        public static List<Block> FromCalendarSearch(CalendarSearch calendarSearch, string location, int distance)
        {
            var returnListBlock = new List<Block>
            {
                new HeaderBlock
                {
                    Text = new PlainText
                    {
                        Text =  $"Upcoming tournaments near {location}"
                    }
                },
                new DividerBlock(),
            };

            foreach(var tournament in calendarSearch.Calendar)
            {
                returnListBlock.Add(new SectionBlock
                {
                    Text = new Markdown
                    {
                        Text = $"*{tournament.TournamentName}*\n" +
                        $":round_pushpin: {tournament.City}\n" +
                        $":calendar: {tournament.StartDate.ToShortDateString()}\n"
                    },
                    Accessory = new Button
                    {
                        Text = new PlainText { Text = "Tournament Website" },
                        Url = $"{tournament.Website}"
                    }
                });
                
                returnListBlock.Add(
                    new ContextBlock
                    {
                        Elements =
                        {
                            new Markdown {  Text = $"<https://www.ifpapinball.com/tournaments/view.php?t={tournament.TournamentId}|View on IFPA website>" }
                        }
                    }
                );

                returnListBlock.Add(new DividerBlock());
            }            

            return returnListBlock;
        }
    }
}
