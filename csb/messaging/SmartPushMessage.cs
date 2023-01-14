using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace csb.messaging
{
    public class SmartPushMessage : PushMessageBase
    {
        [JsonProperty]
        public double TimePeriod { get; set; }

        public static async Task<SmartPushMessage> Create(long id, ITelegramBotClient bot, Message pattern, string chainName, double timeperiod)
        {
            SmartPushMessage res = new SmartPushMessage();

            res.TimePeriod = timeperiod;
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

        public SmartPushMessage Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<SmartPushMessage>(serialized);
        }
    }
}
