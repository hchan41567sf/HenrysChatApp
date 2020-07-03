using ChatApp.GlobalConstants;
using ChatApp.Models;
using ChatApp.Network;
using ChatApp.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace ChatApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public HubConnectionManagerChatPage hubConnectionManager;
        private bool isFirstTimeAppearing = true;


        public ChatPage()
        {
            InitializeComponent();
            hubConnectionManager = new HubConnectionManagerChatPage();
            
        }

        protected async override void OnAppearing()
        {
            if (isFirstTimeAppearing)
            {
                isFirstTimeAppearing = false;
                try
                {
                    string yourPhoneNumber = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
                    Friend friendChattingWith = (Friend)BindingContext;

                    await hubConnectionManager.AttemptToConnectToHub(this);

                    hubConnectionManager.SetUpHubConnectionListenersChatPage(yourPhoneNumber, friendChattingWith, this);

                    //CREATE LABEL FOR FRIEND YOU ARE CHATTING WITH
                    welcome_label.Text = "You are chatting with " + "'" + friendChattingWith.FriendName + "'";

                    //IF FIRST TIME STARTING CHAT WITH FRIEND AND FRIEND HAS A LEFT BEHIND MSG TABLE IN THE CHATHUB SQL SERVER, REMOVE ALL YOUR CHAT MESSAGES (THESE ARE MSGS FROM YOUR PREVIOUS INSTALL)
                    bool firstTimeChattingWithThisFriend = Preferences.Get("first_time_chat_with_" + friendChattingWith.ID, true);
                    if (firstTimeChattingWithThisFriend)
                    {
                        await hubConnectionManager.GetHubConnection().InvokeAsync("DeleteAllYourChatMessagesFromFriendsTable", yourPhoneNumber, friendChattingWith.ID);
                        Preferences.Set("first_time_chat_with_" + friendChattingWith.ID, false);
                    }

                    //CHECK IF WE NEED TO CREATE A TABLE FOR THE FRIEND WE ARE CHATTING WITH
                    bool isFriendInDatabase = await App.Database.IsFriendInDatabase(friendChattingWith);
                    if (!isFriendInDatabase)
                    {
                        App.Database.CreateTableForFriend(friendChattingWith);
                        await MakeRequestToRetrieveLeftBehindMessagesFromChatHub();
                    }
                    else
                    {
                        await InitChatMessagesArea();

                        //CHECK IF THE MESSAGES WE LEFT FOR OUR FRIEND HAVE BEEN SEEN
                        bool hasMessagesNotSeen = await App.Database.HasMessagesNotSeen(friendChattingWith.ID);
                        if (hasMessagesNotSeen)
                            //await hubConnection.InvokeAsync("GetIDsOfYourMessagesSeenByFriend", friendChattingWith.ID, yourPhoneNumber);
                            await hubConnectionManager.GetHubConnection().InvokeAsync("GetIDsOfYourMessagesSeenByFriend", friendChattingWith.ID, yourPhoneNumber);
                    }
                }
                catch (HttpRequestException httpRequestException)
                {

                }
                catch (Exception exception)
                {
                    await Navigation.PopAsync();
                }
            }

        }

        private async Task InitChatMessagesArea()
        {
            chat_messages_area.IsVisible = false;
            await RetrieveAndDisplayChatMessagesForFriend();
            await MakeRequestToRetrieveLeftBehindMessagesFromChatHub();
            await scrollViewChatMsgArea.ScrollToAsync(chat_messages_area, ScrollToPosition.End, false);
            chat_messages_area.IsVisible = true;
        }

        private async Task RetrieveAndDisplayChatMessagesForFriend()
        {
            Friend friendChattingWith = (Friend)BindingContext;
            List<ChatMessage> chatMessages = await App.Database.GetAllChatMessagesForGivenFriend(friendChattingWith);
            foreach (ChatMessage chatMessage in chatMessages)
            {
                //OutputChatMessage(chatMessage);
                ChatMessageLabelGenerator.CreateAndDisplayChatMessageLabel(chatMessage, friendChattingWith, chat_messages_area);
            }
        }

        private async Task MakeRequestToRetrieveLeftBehindMessagesFromChatHub()
        {
            string recepientPhoneNumber = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
            Friend friendChattingWith = (Friend)BindingContext;
            await hubConnectionManager.GetHubConnection().InvokeAsync("GetChatMessagesLeftForUserFromAnotherUser", recepientPhoneNumber, friendChattingWith.ID);
        }

        private void OutputChatMessage(ChatMessage chatMessage)
        {

            Console.WriteLine("Message ID: " + chatMessage.MsgID + ", Writer Phone Number: " + chatMessage.PhoneNumOfWriter + ", Message: " + chatMessage.Msg + ", Date: "
                             + chatMessage.Date + ", Writer: " + chatMessage.Writer + ", SeenByRecipient: " + chatMessage.SeenByRecipient);

        }


        //@writer should be either "You" or "Friend"
        //Returns the chat message just placed in database
        private async Task<ChatMessage> PlaceChatMessageInDatabase(string message, string writer)
        {
            //CREATE THE CHAT MESSAGE
            ChatMessage chatMessage = new ChatMessage();
            Friend friendChattingWith = (Friend)BindingContext;
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


        async void Chat_Entry_Completed(object sender, EventArgs e)
        {
            //PLACE THE MESSAGE IN YOUR DATABASE
            ChatMessage chatMessage = await PlaceChatMessageInDatabase(((Entry)sender).Text, "You");

            ChatMessageLabelGenerator.CreateAndDisplayChatMessageLabel(chatMessage, (Friend)BindingContext, chat_messages_area);

            //SEND THE MESSAGE TO YOUR FRIEND
            Friend friendChattingWith = (Friend)BindingContext;
            await SendMessage(friendChattingWith.ID, chatMessage);

            //clear chat entry box
            chat_entry_box.Text = "";

            //set scroll position to bottom
            await scrollViewChatMsgArea.ScrollToAsync(chat_messages_area, ScrollToPosition.End, false);
        }

        async Task SendMessage(string msgRecepientID, ChatMessage chatMessage)
        {
            await hubConnectionManager.GetHubConnection().InvokeAsync("SendMessageToPerson", msgRecepientID, chatMessage);
        }

        
        override protected async void OnDisappearing()
        {

        }
        

    }
}