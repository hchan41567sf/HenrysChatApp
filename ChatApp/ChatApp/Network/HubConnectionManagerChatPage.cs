using ChatApp.GlobalConstants;
using ChatApp.Models;
using ChatApp.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ChatApp.Network
{
    public class HubConnectionManagerChatPage: HubConnectionManager
    {
        //Precondition: ConnectToHub() has been called
        public void SetUpHubConnectionListenersChatPage(string yourPhoneNumber, Friend friendChattingWith, Page chatPage)
        {
            bool isPhoneNumberNull = yourPhoneNumber.Equals(ApplicationConstants.NO_PHONE_NUMBER);
            Debug.Assert(!isPhoneNumberNull, "SetUpHubConnectionListenersChatPage function received a null phone number");

            SetHubConnectionhandlerToRecieveChatMsg(yourPhoneNumber, chatPage, friendChattingWith);
            SetHubConnectionHandlerToRecieveLeftBehindMessages(yourPhoneNumber, chatPage, friendChattingWith);
            SetHubConnectionHandlerToReceiveMessagesSeenNotice(yourPhoneNumber, chatPage, friendChattingWith);
            SetHubConnectionHandlerToReceiveIDsOfSeenMessages(yourPhoneNumber, chatPage, friendChattingWith);
        }

        private void SetHubConnectionhandlerToRecieveChatMsg(string yourPhoneNumber, Page chatPage, Friend friendChattingWith)
        {
            hubConnection.On<ChatMessage>(yourPhoneNumber, async (chatMessage) =>
            {
                //Friend friendChattingWith = (Friend)BindingContext;
                if (friendChattingWith.ID == chatMessage.PhoneNumOfWriter) //if the person's phone # that sent me the message is = to the phone # of the person i started the chat with
                {
                    ScrollView scrollViewChatMsgArea = (ScrollView)chatPage.FindByName("scrollViewChatMsgArea");
                    StackLayout chat_messages_area = (StackLayout)chatPage.FindByName("chat_messages_area");

                    //CREATE AND DISPLAY THE CHAT MESSAGE THAT WAS RECIEVED
                    chatMessage.Writer = "Friend";
                    ChatMessageLabelGenerator.CreateAndDisplayChatMessageLabel(chatMessage, friendChattingWith, chat_messages_area);

                    //SCROLL TO THE END POSITION OF THE SCROLL VIEW SO WE CAN SEE THE MESSAGE ADDED
                    scrollViewChatMsgArea.ScrollToAsync(chat_messages_area, ScrollToPosition.End, false);

                    //PLACE THE MESSAGE RECEIVED IN THE DATABASE
                    await PlaceChatMessageInDatabase(chatMessage.Msg, "Friend", friendChattingWith);

                    //TELL THE HUB I RECEIVED THE MESSAGE
                    List<PrimaryKeyLeftBehindMsg> listOfPrimaryKeysForLeftBehindMsgs = new List<PrimaryKeyLeftBehindMsg>();
                    PrimaryKeyLeftBehindMsg primaryKey = new PrimaryKeyLeftBehindMsg();
                    primaryKey.MsgID = chatMessage.MsgID; primaryKey.PhoneNumWriter = chatMessage.PhoneNumOfWriter;
                    listOfPrimaryKeysForLeftBehindMsgs.Add(primaryKey);
                    await hubConnection.InvokeAsync("MessagesReceived", yourPhoneNumber, listOfPrimaryKeysForLeftBehindMsgs);
                }
            });
        }

        private void SetHubConnectionHandlerToRecieveLeftBehindMessages(string yourPhoneNumber, Page chatPage, Friend friendChattingWith)
        {
            ScrollView scrollViewChatMsgArea = (ScrollView)chatPage.FindByName("scrollViewChatMsgArea");
            StackLayout chat_messages_area = (StackLayout)chatPage.FindByName("chat_messages_area");

            hubConnection.On<List<ChatMessage>>(yourPhoneNumber + "-left_behind", async (leftBehindChatMessages) =>
            {
                List<PrimaryKeyLeftBehindMsg> listOfPrimaryKeysForLeftBehindMsgs = new List<PrimaryKeyLeftBehindMsg>(); //Used to tell chatchub that messages have been received
                foreach (ChatMessage leftBehindChatMsg in leftBehindChatMessages)
                {
                    if ((leftBehindChatMsg.SeenByRecipient).Equals("no"))
                    {
                        //PLACE THE LEFT BEHIND MESSAGE IN YOUR CHAT AREA
                        leftBehindChatMsg.Writer = "Friend";
                        ChatMessageLabelGenerator.CreateAndDisplayChatMessageLabel(leftBehindChatMsg, friendChattingWith, chat_messages_area);
                        scrollViewChatMsgArea.ScrollToAsync(chat_messages_area, ScrollToPosition.End, false);

                        //PLACE THE MESSAGE IN YOUR DATABASE
                        await PlaceChatMessageInDatabase(leftBehindChatMsg.Msg, "Friend", friendChattingWith);

                        //PLACE THE PRIMARY KEY OF THIS MESSAGE IN A LIST
                        PrimaryKeyLeftBehindMsg primaryKey = new PrimaryKeyLeftBehindMsg();
                        primaryKey.MsgID = leftBehindChatMsg.MsgID; primaryKey.PhoneNumWriter = leftBehindChatMsg.PhoneNumOfWriter;
                        listOfPrimaryKeysForLeftBehindMsgs.Add(primaryKey);
                    }

                }
                //TELL CHAT HUB THAT THESE LEFT BEHIND MESSAGES HAVE BEEN RECEIVED
                await hubConnection.InvokeAsync("MessagesReceived", yourPhoneNumber, listOfPrimaryKeysForLeftBehindMsgs);
            });
        }

        private void SetHubConnectionHandlerToReceiveMessagesSeenNotice(string yourPhoneNumber, Page chatPage, Friend friendChattingWith)
        {
            hubConnection.On<List<PrimaryKeyLeftBehindMsg>>(yourPhoneNumber + "-messages_seen", async (listOfPrimaryKeysForLeftBehindMsgs) =>
            {
                await App.Database.MarkChatMessagesAsSeen(friendChattingWith.ID, listOfPrimaryKeysForLeftBehindMsgs);

                StackLayout chat_messages_area = (StackLayout)chatPage.FindByName("chat_messages_area");
                //NOW SET THE "Msg seen" labels for the messages that have just been seen TO "Msg seen"
                foreach (PrimaryKeyLeftBehindMsg primaryKeyLeftBehindMsg in listOfPrimaryKeysForLeftBehindMsgs)
                {
                    IList<View> listOfChatMsgViews = chat_messages_area.Children;
                    foreach (View chatMsgView in listOfChatMsgViews) //for each of the chat message views that you see
                    {
                        string chatMsgViewStyleID = chatMsgView.StyleId;
                        //Console.WriteLine("StyleID: " + chatMsgViewStyleID);
                        string leftBehindMsgID = primaryKeyLeftBehindMsg.MsgID.ToString();
                        if (chatMsgViewStyleID.Equals(leftBehindMsgID)) //if the id of the chat msg view is = the message id of the chat message that was seen by friend
                        {
                            //GET THE CHILDREN OF THE CHAT MESSAGE VIEW
                            StackLayout chatMsgViewStackLayout = (StackLayout)chatMsgView;
                            IList<View> childrenOfChatMsgView = chatMsgViewStackLayout.Children;

                            //GET THE SECTION OF THE CHAT MESSAGE VIEW THAT HOLDS THE CHAT MESSAGE INFORMATION
                            StackLayout chatMsgInfoSection = (StackLayout)childrenOfChatMsgView[1];
                            IList<View> chatMsgInfoSectChildren = chatMsgInfoSection.Children;

                            //IN THE CHAT MESSAGE INFORMATION SECTION, GET THE FRAME THAT HOLDS THE "MSG SEEN" LABEL AND SET THE CONTENT TO A NEW LABEL THAT SAYS "MSG SEEN"
                            Frame msgSeenFrame = (Frame)chatMsgInfoSectChildren[1];
                            msgSeenFrame.Content = new Label { Text = "Msg Seen", FontSize = 8.0 };
                        }
                    }
                }

                //Tell Chathub that I have received notice that my messages have been seen so chathub can delete those messages in its sql server
                await hubConnection.InvokeAsync("DeleteAllYourChatMessagesFromFriendsTable", yourPhoneNumber, friendChattingWith.ID);
            });
        }

        private void SetHubConnectionHandlerToReceiveIDsOfSeenMessages(string yourPhoneNumber, Page chatPage, Friend friendChattingWith)
        {
            hubConnection.On<List<string>>(yourPhoneNumber + "-return_ids_seen_messages", async (listOfIDsOfSeenMessages) =>
            {
                await App.Database.MarkChatMessagesAsSeen(friendChattingWith.ID, listOfIDsOfSeenMessages);

                //WE NOW KNOW MY MESSAGES HAVE BEEN SEEN. TELL CHATHUB TO DELETE THOSE MESSAGES IN ITS SQL SERVER
                if (listOfIDsOfSeenMessages.Count > 0)
                    await hubConnection.InvokeAsync("DeleteAllYourChatMessagesFromFriendsTable", yourPhoneNumber, friendChattingWith.ID);

                StackLayout chat_messages_area = (StackLayout)chatPage.FindByName("chat_messages_area");
                IList<View> listOfChatMsgViews = chat_messages_area.Children;
                foreach (string idOfSeenMsg in listOfIDsOfSeenMessages)
                {
                    foreach (View chatMsgView in listOfChatMsgViews) //for each of the chat message views that you see
                    {
                        string chatMsgViewStyleID = chatMsgView.StyleId;
                        string seenMsgID = idOfSeenMsg.ToString();
                        if (chatMsgViewStyleID.Equals(seenMsgID)) //if the id of the chat msg view is = the message id of the chat message that was seen by friend
                        {
                            //GET THE CHILDREN OF THE CHAT MESSAGE VIEW
                            StackLayout chatMsgViewStackLayout = (StackLayout)chatMsgView;
                            IList<View> childrenOfChatMsgView = chatMsgViewStackLayout.Children;

                            //GET THE SECTION OF THE CHAT MESSAGE VIEW THAT HOLDS THE CHAT MESSAGE INFORMATION
                            StackLayout chatMsgInfoSection = (StackLayout)childrenOfChatMsgView[1];
                            IList<View> chatMsgInfoSectChildren = chatMsgInfoSection.Children;

                            //IN THE CHAT MESSAGE INFORMATION SECTION, GET THE FRAME THAT HOLDS THE "MSG SEEN" LABEL AND SET THE CONTENT TO A NEW LABEL THAT SAYS "MSG SEEN"
                            Frame msgSeenFrame = (Frame)chatMsgInfoSectChildren[1];
                            msgSeenFrame.Content = new Label { Text = "Msg Seen", FontSize = 8.0 };
                        }
                    }
                }

            });
        }

        //@writer should be either "You" or "Friend"
        //Returns the chat message just placed in database
        private async Task<ChatMessage> PlaceChatMessageInDatabase(string message, string writer, Friend friendChattingWith)
        {
            //CREATE THE CHAT MESSAGE
            ChatMessage chatMessage = new ChatMessage();
            string seenByRecipient;
            string phoneNumOfWriter;
            if (writer.Equals("You"))
            {
                seenByRecipient = "no";
                phoneNumOfWriter = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
            }
            else
            {
                seenByRecipient = "na";
                phoneNumOfWriter = friendChattingWith.ID;
            }
            chatMessage.PhoneNumOfWriter = phoneNumOfWriter; chatMessage.Msg = message; chatMessage.Date = DateTime.Now.ToString();
            chatMessage.Writer = writer; chatMessage.SeenByRecipient = seenByRecipient;

            //ADD THE CHAT MESSAGE TO DATABASE
            chatMessage.MsgID = await App.Database.PutChatMessageOfGivenFriendInDatabase(friendChattingWith, chatMessage);

            return chatMessage;
        }
    }
}
