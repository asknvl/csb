using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTelegram;

namespace csb.usr_listener
{
    public class MediaGroupProcessor
    {

    }

    public class MediaGroup
    {
        #region vars
        System.Timers.Timer mediaTimer = new System.Timers.Timer();
        #endregion

        #region properties
        public long? GroupId { get; set; }
        public List<int> MessageIDs { get;} = new();
        public List<long> MessageRands { get; } = new();
        public bool IsEmpty => MessageIDs.Count == 0;
        #endregion

        public MediaGroup(MediaGroup source)
        {
            GroupId = source.GroupId;
            MessageIDs = new List<int>();
            foreach (var id in source.MessageIDs)
                MessageIDs.Add(id);
            MessageRands = new List<long>();
            foreach (var rand in source.MessageRands)
                MessageRands.Add(rand);
        }

        public MediaGroup() {
            mediaTimer.Interval = 3000;
            mediaTimer.AutoReset = false;
            mediaTimer.Elapsed += MediaTimer_Elapsed;
        }

        private void MediaTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MediaReadyEvent?.Invoke(new MediaGroup(this));
            clear();
        }

        public void Update(long? group_id, int message_id) {

            if (GroupId != group_id)
            {
                Console.WriteLine($"Group={GroupId} group={group_id} message {message_id} PURGE");
                purge();                
            }                     

            if (group_id == null || group_id == 0)
            {
                var mediaGroup = new MediaGroup();
                mediaGroup.add(null, message_id);
                Console.WriteLine($"Group={GroupId} group={group_id} message {message_id} INVOKE");
                MediaReadyEvent?.Invoke(mediaGroup);
                mediaTimer.Stop();
                clear();

            } else
            {
                add(group_id, message_id);
                Console.WriteLine($"Group={GroupId} group={group_id} message {message_id} ADD");
                if (!mediaTimer.Enabled)
                    mediaTimer.Start();
            }
        }

        public void add(long? group_id, int message_id)
        {
            GroupId = group_id;
            MessageIDs.Add(message_id);
            MessageRands.Add(Helpers.RandomLong());
        }

        private void purge()
        {
            if (MessageIDs.Count > 0)
            {
                MediaReadyEvent?.Invoke(new MediaGroup(this));
                mediaTimer.Stop();
                clear();
            }
        }

        public void clear()
        {
            GroupId = null;
            MessageIDs.Clear();
            MessageRands.Clear();
        }

        public event Action<MediaGroup> MediaReadyEvent;

        
    }
}
