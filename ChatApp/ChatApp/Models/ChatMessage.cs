using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Models
{
    public class ChatMessage
    {
        public int MsgID { get; set; }
        public string PhoneNumOfWriter { get; set; }
        public string Msg { get; set; }
        public string Date { get; set; }
        public string Writer { get; set; }
        public string SeenByRecipient { get; set; }

    }
}
