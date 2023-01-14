using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using csb.bot_poster;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace csb.messaging
{
    public class DailyPushMessage : PushMessageBase
    {
        public DailyPushMessage()
        {
        }

        public static async Task<DailyPushMessage> Create(long id, ITelegramBotClient bot, Message pattern, string chainName)
        {
            DailyPushMessage res = new DailyPushMessage();
            res.Message = pattern;

            string fileId = null;
            Telegram.Bot.Types.File fileInfo;
            string filePath = null;

            await Task.Run(async () => {

                switch (res.Message.Type)
                {
                    case MessageType.Text:
                        break;
                    case MessageType.Photo:
                        fileId = res.Message.Photo.Last().FileId;
                        break;
                    case MessageType.Video:
                        fileId = res.Message.Video.FileId;
                        break;
                    case MessageType.Document:
                        fileId = res.Message.Document.FileId;
                        break;
                }

                if (fileId != null)
                {
                    fileInfo = await bot.GetFileAsync(fileId);
                    filePath = fileInfo.FilePath;

                    var fileName = filePath.Split('/').Last();

                    string destinationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "chains", $"{id}", chainName);
                    if (!Directory.Exists(destinationFilePath))
                        Directory.CreateDirectory(destinationFilePath);

                    destinationFilePath = Path.Combine(destinationFilePath, fileName);

                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await bot.DownloadFileAsync(
                        filePath: filePath,
                        destination: fileStream);

                    res.FilePath = destinationFilePath;
                }

            });

            return res;

        }

        public DailyPushMessage Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<DailyPushMessage>(serialized);
        }

    }
}
