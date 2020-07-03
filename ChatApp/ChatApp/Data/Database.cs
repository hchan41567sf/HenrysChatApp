using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Models;
using SQLite;
using ChatApp.GlobalConstants;
using System;

namespace ChatApp.Data
{
    public class Database
    {
        readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);

        }

        //Add table for friend
        public void CreateTableForFriend(Friend friend)
        {
            string sql = "CREATE TABLE " + "[" + friend.ID + "]" + " ("
                + ApplicationConstants.MSG_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + ApplicationConstants.MSG_PHONE_NUM_WRITER + " varchar(255),"
                + ApplicationConstants.MSG_COLUMN + " varchar(255),"
                + ApplicationConstants.DATE_COLUMN + " varchar(255),"
                + ApplicationConstants.WRITER_COLUMN + " varchar(255),"
                + ApplicationConstants.SEEN_COLUMN + " varchar(255)"
                + ");";

            _database.ExecuteAsync(sql);
        }

        public async Task<bool> IsFriendInDatabase(Friend friend)
        {
            List<TableName> tableNames = await GetAllTablesAsync();

            foreach (TableName tableName in tableNames)
            {
                if((tableName.name).Equals(friend.ID))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<List<TableName>> GetAllTablesAsync()
        {
            string queryString = $"SELECT name FROM sqlite_master WHERE type = 'table'";
            return await _database.QueryAsync<TableName>(queryString).ConfigureAwait(false);
        }

        //Put a message in table for given friend
        //Returns the primary key of the message just inserted
        public async Task<int> PutChatMessageOfGivenFriendInDatabase(Friend friend, ChatMessage chatMessage)
        {
            string sql = "INSERT INTO " + "[" + friend.ID + "]" + " (" + ApplicationConstants.MSG_PHONE_NUM_WRITER + ", " + ApplicationConstants.MSG_COLUMN + ", " + ApplicationConstants.DATE_COLUMN + ", " + ApplicationConstants.WRITER_COLUMN + ", " + ApplicationConstants.SEEN_COLUMN + ") "
                + "VALUES " + "( " + "'" + chatMessage.PhoneNumOfWriter + "' ," + "'" + chatMessage.Msg + "' ," + "'" + chatMessage.Date + "'" + " ," + "'" + chatMessage.Writer + "'" + " ," + "'" + chatMessage.SeenByRecipient + "'" + ");";

            await _database.ExecuteAsync(sql);

            int lastRowPrimaryKey = await _database.ExecuteScalarAsync<int>("SELECT last_insert_rowid();");
            return lastRowPrimaryKey;
        }

        //Retrieve all chat messages for given friend
        public async Task<List<ChatMessage>> GetAllChatMessagesForGivenFriend(Friend friend)
        {
            string sql = "SELECT * " + "FROM " + "[" + friend.ID + "]";
            return await _database.QueryAsync<ChatMessage>(sql);
        }

        public async Task MarkChatMessagesAsSeen(string friendPhoneNum, List<PrimaryKeyLeftBehindMsg> primaryKeyLeftBehindMessages)
        {
            foreach (PrimaryKeyLeftBehindMsg primaryKeyLeftBehindMsg in primaryKeyLeftBehindMessages)
            {
                string sql = "UPDATE " + "[" + friendPhoneNum + "] "
                    + "SET " + ApplicationConstants.SEEN_COLUMN + "=" + "'yes' "
                    + "WHERE " + ApplicationConstants.MSG_ID + "=" + (primaryKeyLeftBehindMsg.MsgID).ToString();
                await _database.ExecuteAsync(sql);
            }
        }

        public async Task MarkChatMessagesAsSeen(string friendPhoneNum, List<string> msgIDs)
        {
            foreach (string msgID in msgIDs)
            {
                string sql = "UPDATE " + "[" + friendPhoneNum + "] "
                    + "SET " + ApplicationConstants.SEEN_COLUMN + "=" + "'yes' "
                    + "WHERE " + ApplicationConstants.MSG_ID + "=" + msgID;
                await _database.ExecuteAsync(sql);
            }
        }

        public async Task<bool> HasMessagesNotSeen(string friendPhoneNum)
        {
            string sql = "SELECT * " + "FROM " + "[" + friendPhoneNum + "] "
                + "WHERE " + ApplicationConstants.SEEN_COLUMN + "=" + "'no';";
            List<ChatMessage> notSeenMessages = await _database.QueryAsync<ChatMessage>(sql);

            bool hasMessagesNotSeen;
            if (notSeenMessages.Count > 0)
                hasMessagesNotSeen = true;
            else
                hasMessagesNotSeen = false;

            return hasMessagesNotSeen;
        }

    }
}
