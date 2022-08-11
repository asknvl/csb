using csb.server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.bot_moderator
{
    public class BotModerator_v1 : BotModerator
    {

        #region properties       
        int DeltaFollowersCounter { get; set; } = 0;
        int DeltaRequestsCounter { get; set; } = 0;
        #endregion

        public BotModerator_v1(string token, string geotag) : base(token, geotag) { }

        #region override
        protected override async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update == null)
                return;

            if (update.ChatMember != null)
            {
                try
                {
                    var member = update.ChatMember;

                    long user_id = member.NewChatMember.User.Id;
                    long chat_id = update.ChatMember.Chat.Id;

                    string fn = member.NewChatMember.User.FirstName;
                    string ln = member.NewChatMember.User.LastName;
                    string un = member.NewChatMember.User.Username;

                    string link = member.InviteLink?.InviteLink;

                    List<Follower> followers = new();
                    var follower = new Follower()
                    {
                        tg_chat_id = chat_id,
                        tg_user_id = user_id,
                        username = un,
                        firstname = fn,
                        lastname = ln,
                        invite_link = link,
                        tg_geolocation = GeoTag
                    };

                    switch (member.NewChatMember.Status)
                    {
                        case Telegram.Bot.Types.Enums.ChatMemberStatus.Member:
                            follower.is_subscribed = true;
                            DeltaFollowersCounter++;
                            DeltaRequestsCounter--;
                            break;

                        case Telegram.Bot.Types.Enums.ChatMemberStatus.Left:
                            follower.is_subscribed = false;
                            DeltaFollowersCounter--;
                            break;
                    }

                    followers.Add(follower);
                    await statApi.UpdateFollowers(followers);

                    Console.WriteLine(follower);
                    Console.WriteLine($"{GeoTag} dRequests={DeltaRequestsCounter}, dFollowes={DeltaFollowersCounter}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"------------------------ {DateTime.Now} {ex.Message} --------------------------------");
                }
            }

            if (update.ChatJoinRequest != null)
            {
                try
                {
                    DeltaRequestsCounter++;
                    var chatJoinRequest = update.ChatJoinRequest;
                    bool res = await bot.ApproveChatJoinRequest(chatJoinRequest.Chat.Id, chatJoinRequest.From.Id);


                    //string info = $"{DateTime.Now} {GeoTag}: " +
                    //              $"req {RequestsCounter.ToString().PadLeft(6)} " +
                    //              $"link={chatJoinRequest.InviteLink.InviteLink} " +
                    //              $"from {chatJoinRequest.From.FirstName} {chatJoinRequest.From.LastName} " +
                    //              $"approved={res} " +
                    //              $"approves={ApprovesCounter.ToString().PadLeft(6)} " +
                    //              $"apicntr={ApisendsCounter.ToString().PadLeft(6)}";
                    //Console.WriteLine(info);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"------------------------ {DateTime.Now} {ex.Message} --------------------------------");
                }
            }            
        }
        #endregion
    }

}
