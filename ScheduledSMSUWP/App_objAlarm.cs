using System;
using System.Runtime.Serialization;

namespace ScheduledSMSUWP
{
    public sealed class objAlarm
    {

        [DataMember]
        public string Id { get; set; }// tbl task GUID

        [DataMember]
        public string Name { get; set; }//task name whatsapp,

        [DataMember]
        public AppType appType { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string To { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        public bool IsOneTime()
        {
            return SingleFireTime != DateTimeOffset.MinValue;
        }

        [DataMember]
        public DateTimeOffset SingleFireTime { get; set; }


    }
}
