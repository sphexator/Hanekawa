using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hanekawa.Addons.Patreon.Entities;
using Hanekawa.Addons.Patreon.Entities.Campaign;
using Hanekawa.Addons.Patreon.Entities.old;
using Hanekawa.Addons.Patreon.Entities.Pledge;
using Newtonsoft.Json;

namespace Hanekawa.Addons.Patreon
{
    public class PatreonClient : IDisposable
    {
        private readonly string _token = "O2HpJPEuIbSq6Wn8aXO6zMuJQ2j5Q4WzKGqPyhhHNR8";
        private const string BaseUrl = "https://www.patreon.com/api/oauth2/api/";
        private string CampaignId { get; set; }

        public PatreonClient()
        {
           // _token = token;
        }

        public async Task InitializeAsync()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = (await client.GetAsync(BaseUrl + "current_user/campaigns")).EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CampaignResponse>(data);
                CampaignId = result.data[0].id;
            }
        }

        public async Task GetProfileInfo()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = (await client.GetAsync(BaseUrl + "current_user")).EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Campaign>(data);
                Console.WriteLine(result);
            }
        }

        public async Task<CampaignResponse> GetCampaignInfo()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = (await client.GetAsync(BaseUrl + "current_user/campaigns")).EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CampaignResponse>(data);
                Console.WriteLine(result);
                return result;
            }
        }

        public async Task<IEnumerable<PledgeReturn>> GetPledges()
        {
            if (CampaignId == null) return null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var url = BaseUrl + $"campaigns/{CampaignId}/pledges?page%5Bcount%5D=25&sort=-created";
                var paginate = true;
                var result = new List<PledgeResponse>();
                while (paginate)
                {
                    var response = (await client.GetAsync(url)).EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsStringAsync();
                    var deserialize = JsonConvert.DeserializeObject<PledgeResponse>(data);
                    result.Add(deserialize);
                    if (deserialize.links.next != null)
                    {
                        url = deserialize.links.next;
                    }
                    else paginate = false;
                }
                var parse = ToReadAble(result);
                return parse;
            }
        }

        private static IEnumerable<PledgeReturn> ToReadAble(IEnumerable<PledgeResponse> response)
        {
            var result = new List<PledgeReturn>();
            foreach (var x in response)
            {
                for (var i = 0; i < x.data.Length; i++)
                {
                    var pledge = x.data[i];
                    var user = x.included[i];
                    ulong dResult;
                    try
                    {
                        if (user.attributes.social_connections.discord.user_id == null) dResult = 0;
                        else
                        {
                            var discordId = ulong.TryParse(user.attributes.social_connections.discord.user_id, out dResult);
                            if (!discordId) dResult = 0;
                        }
                    }
                    catch
                    {
                        dResult = 0;
                    }

                    result.Add(new PledgeReturn
                    {
                        Pledges = new Pledge
                        {
                            AmountCents = pledge.attributes.amount_cents,
                            CreatedAt = pledge.attributes.created_at,
                            Creator = null,
                            DeclinedSince = pledge.attributes.declined_since,
                            PledgeCapCents = pledge.attributes.pledge_cap_cents,
                            PatronPaysFees = pledge.attributes.patron_pays_fees
                        },
                        Users = new User
                        {
                            DiscordId = dResult,
                            Created = user.attributes.created,
                            Email = user.attributes.email,
                            FullName = user.attributes.full_name,
                            IsEmailVerified = user.attributes.is_email_verified,
                            Url = user.attributes.url,
                            Vanity = user.attributes.vanity,
                        }
                    });
                }
            }

            return result;
        }

        public void Dispose()
        {

        }
    }
}