using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ChatApp.Models
{
    
    
    public class Friend 
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string FriendName { get; set; }
        public string CanChatWith { get; set; }

        public int NewMsgCount { get; set; }

        public Friend()
        {

        }

        public Friend(string id, string friendName)
        {
            ID = id;
            FriendName = friendName;
            CanChatWith = "False";
        }

    }
}
