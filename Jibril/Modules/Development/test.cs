using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Development
{
    public class Test : InteractiveBase
    {
        [Command("roleid", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetRoleID([Remainder] string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await ReplyAsync($"{role.Name}\n" +
                             $"{role.Id}\n" +
                             $"{role.Color.RawValue}\n" +
                             $"{role.Position}");
        }

        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TestRules()
        {
            const string rulestr = "**General**\n" +
                                   "- Post content in their respective channels.\n" +
                                   "- Do not post content NSFW outside of the NSFW channels.\n" +
                                   "- Do not ping the staff role unless a staff member is needed immediately.\n" +
                                   "- Do not roleplay. Small quotes or interactions is fine.\n" +
                                   "- Do not discuss about politics.\n" +
                                   "- Do not discuss about Religion.\n" +
                                   "\n" +
                                   "**Language**\n" +
                                   "- English only\n" +
                                   "- No racial slurs\n" +
                                   "- No sexist jokes or language\n" +
                                   "\n" +
                                   "**Behaviour**\n" +
                                   "- Do not claim X did Y without valid proof.\n" +
                                   "- Do not bait maliciously.\n" +
                                   "- Do not post personal info without consent.\n" +
                                   "- Do not fake account (impersonate users or staff members).\n" +
                                   "- Do not harass users in public or private.\n" +
                                   "- Do not attack, insult, or be offensive toward anyone. You can disagree but no need to take it further.\n" +
                                   "- Do not use foul language with the attempt to offend someone or cause harm.\n" +
                                   "\n" +
                                   "**Username, nick & profile picture**\n" +
                                   "- Profile picture, username and nickname are to be kept safe for work(sfw) and non disturbing.\n" +
                                   "- Avoid offensive or otherwise annoying username/nicknames. If it’s difficult to mention, don’t use it.\n" +
                                   "\n" +
                                   "**Advertisement**\n" +
                                   "- Advertising of any form is prohibited unless given written permission from a staff member.\n" +
                                   "- Content creators are allowed to share their work as long it’s done in the appropriate channels.\n" +
                                   "   - Content is meant to be for viewing and not advertisement\n" +
                                   "   - You are to not lead people into purchasing your product.\n" +
                                   "   - Staff reserves the right to remove any content deemed unit.\n" +
                                   "\n" +
                                   "**Ban**\n" +
                                   "- Bans are permanent till they’ve appealed & gotten accepted back into the server.\n" +
                                   "- Ban evasion results in an instant ban without warning.\n" +
                                   "\n" +
                                   "**Malicious activity**\n" +
                                   "- Do not organize any raid toward any server with the intent of (1)causing disturbance, (2)trolling and/or (3)humiliate.\n" +
                                   "- Any malicious activity toward the server or its members is forbidden.\n" +
                                   "\n" +
                                   "Keep in mind that channel specific ruleset are in the channel description & pins.\n" +
                                   "Rules apply to in-server chats, direct messages(DM/PM) and voice channels.";


            const string faqstr = "Q: I have a problem, who do I contact?\n" +
                                  "If you're experiencing a problem regarding a member and/or anything related to the server, please contact a @Staff member.\n" +
                                  "\n" +
                                  "Q: How does the leveling system work?\n" +
                                  "The leveling system is custom built for this server and gives experience & credit once per minute you type. You also gain experience through being in voice channels but at a much lower rate.\n" +
                                  "At any point there can be events that gives double the experience or other benefits on this server which will last for 24 hours (or more).\n" +
                                  "\n" +
                                  "Q: What's the Kai Ni role?\n" +
                                  "Kai Ni is the name of the MVP role, every Monday the 5 most active users will recieve the Kai Ni role until next Monday.\n" +
                                  "\n" +
                                  "Q: Is there a way to support the server?\n" +
                                  "We have a patreon in where you can support the server financially and get a special role in return for being a patreon.\n" +
                                  "https://www.patreon.com/KanColle\n" +
                                  "\n" +
                                  "Q: Who's the owner?\n" +
                                  "Admirals\n" +
                                  "\n" +
                                  "Q: I have a suggestion for the server, where do I post it?\n" +
                                  "Head over to #botspam and type !suggest <your suggestion> and it'll be posted in #server-suggestions for evalution.\n" +
                                  "\n" +
                                  "Q: I want to host events but only staff hosts events.. Can I host one?\n" +
                                  "Anyone is free to host events aslong they contact a staff member or admin about it. There's 0 difference from a staff member hosting an event and a regular member.\n" +
                                  "\n" +
                                  "Q: is Yamato public/open source?\n" +
                                  "Yamato is private/custom bot developed by Sphexator. (Written in C# using dnet lib for those curious).";

            const string staffstr = "Staff 1\n" +
                                    "Staff 2\n" +
                                    "Staff 3\n" +
                                    "Staff 4\n" +
                                    "Staff 5\n" +
                                    "Staff 6\n" +
                                    "Staff 7\n" +
                                    "Staff 8\n" +
                                    "Staff 9\n" +
                                    "Staff 10\n" +
                                    "Staff 11\n" +
                                    "Staff 12\n" +
                                    "Staff 13\n" +
                                    "Staff 14\n" +
                                    "Staff 15\n";

            const string levelstr = "Level 2 = Ship\n" +
                                    "Level 5 = Light Cruiser\n" +
                                    "Level 10 = Heavy Cruiser\n" +
                                    "Level 20 = Destroyer\n" +
                                    "Level 30 = Aircraft Carrier\n" +
                                    "Level 40 = Battleship\n" +
                                    "Level 50 = Aviation Cruiser\n" +
                                    "Level 65 = Aviation Battleship\n" +
                                    "Level 80 = Training Cruiser\n";

            var rules = new EmbedBuilder
            {
                Color = Color.Purple,
                Description = rulestr
            };

            var faq = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = faqstr
            };
            var staff = new EmbedBuilder
            {
                Color = Color.Orange,
                Description = staffstr
            };
            var level = new EmbedBuilder
            {
                Color = Color.Green,
                Description = levelstr
            };

            await Context.Channel.SendFileAsync(@"Data\Info\rules.png", null, false, rules.Build());
            await Context.Channel.SendFileAsync(@"Data\Info\faq.png", null, false, faq.Build());
            await Context.Channel.SendFileAsync(@"Data\Info\staff.png", null, false, staff.Build());
            await Context.Channel.SendFileAsync(@"Data\Info\level_roles.png", null, false, level.Build());
        }

        [Command("users", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetUsers()
        {
            await Context.Guild.DownloadUsersAsync();
            Console.WriteLine(Context.Guild.Users.Count);
            foreach (var x in Context.Guild.Users)
            {
                Console.WriteLine($"{x.Username} - {x.Id}");
            }
            Console.WriteLine(Context.Guild.Users.Count);
        }
    }
}
