using Flurl;
using Flurl.Http;
using IFPACompanionSlack.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlackNet.WebApi;

namespace IFPACompanionSlack.Controllers
{
    [Route("slack/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        public IOAuthV2Api oAuthV2Api { get; private set; }
        public SlackSettings settings { get; private set; }

        public OAuthController(IOAuthV2Api oAuthV2Api, SlackSettings slackSettings)
        {
            this.oAuthV2Api = oAuthV2Api;
            this.settings = slackSettings;
        }

        [HttpGet]
        public async Task<IActionResult> OAuthRedirect(string code)
        {
            var access = await oAuthV2Api.Access(settings.ClientId, settings.ClientSecret, code, null, null, null, CancellationToken.None);
            
            //Getting the user back to Slack is confusing, too. I want the user to install the app, approve, then I guess go back to slack, right?
            return Ok("IFPA Companion for Slack has successfully been installed.");
        }

    }
}
