using ChatApp.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ChatApp.GlobalConstants;
using Xamarin.Essentials;
using ChatApp.AppStartup;
using ChatApp.Network;
using System.Net.Http;
using System.Collections.ObjectModel;
using ChatApp.Configuration;

namespace ChatApp
{    
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {      
        private HubConnectionManagerMainPage hubConnectionManager;
        private ObservableCollection<Friend> friendList;
        private bool isFirstAppearance;


        public MainPage()
        {
            InitializeComponent();
            isFirstAppearance = true;           
        }

        protected override async void OnAppearing()
        {            
            if (isFirstAppearance)
            {
                isFirstAppearance = false;

                base.OnAppearing();

                AppInitiator appInitiator = new AppInitiator(this);

                //SET FRIEND'S OBSERVABLE COLLECTION SOURCE
                friendList = await App.FriendsManagement.returnListOfFriendsAsync();
                friendListView.ItemsSource = friendList;

                //INITIATE APP IF FIRST TIME START UP
                bool isFirstTimeStartingApp = Preferences.Get(ApplicationConstants.FIRST_TIME_START_APP_PREF, true);
                if (isFirstTimeStartingApp)
                {
                    await appInitiator.IniatiateAppForFirstTimeStartup();
                }

                //CONNECT TO THE CHAT HUB
                hubConnectionManager = new HubConnectionManagerMainPage();
                await hubConnectionManager.AttemptToConnectToHub(this);

                //NOW THAT WE HAVE CONNECTED TO THE CHAT HUB, WE ARE ABLE TO CHAT WITH FRIENDS SO ACTIVATE CHAT BUTTON FOR EACH FRIEND
                await ActivateChatButtonForEachFriend();

                //CHECK IF USER HAS A CHAT MESSAGES TABLE IN THE DATABASE OF THE CHAT HUB IF WE JUST ENTERED THE PHONE NUMBER
                bool justEnteredPhoneNum = Preferences.Get(ApplicationConstants.JUST_ENTERED_PHONENUM_PREF, true);
                if (justEnteredPhoneNum)
                {
                    await appInitiator.GenerateEmptyTableInChatHubDatabase(hubConnectionManager.GetHubConnection());
                }

                string yourPhoneNumber = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
                hubConnectionManager.SetUpHubConnectionListenersMainPage(yourPhoneNumber, this);

                //REQUEST FROM CHATHUB A COUNT OF NEW MESSAGES
                await hubConnectionManager.GetHubConnection().InvokeAsync("GetNewMessageCountForEachFriendThatLeftAMessage", yourPhoneNumber);

            }
        }

        async void OnChatButtonClicked(object sender, EventArgs e)
        {   
            Friend friendToChatWith = (Friend)((Button)sender).BindingContext;
            await Navigation.PushAsync(new ChatPage
            {
                BindingContext = friendToChatWith
            });
        }

        private async Task ActivateChatButtonForEachFriend()
        {
            ObservableCollection<Friend> newFriendList = await App.FriendsManagement.returnListOfFriendsAsync();

            //friendList is the item source for the list view, but just changing the Friends in friend list is not good enough.
            //We must clear the friends in the list first and then populate it again with the updated friends 
            friendList.Clear();
            foreach (Friend friend in newFriendList)
            {
                friend.CanChatWith = "True";
                friendList.Add(friend);
            }
            
        }      

        public ObservableCollection<Friend> GetFriendList()
        {
            return friendList;
        }

        public HubConnectionManagerMainPage GetHubConnectionManager()
        {
            return hubConnectionManager;
        }

        public async Task SetNewMsgCountZeroAllFriends()
        {
            ObservableCollection<Friend> newFriendList = await App.FriendsManagement.returnListOfFriendsAsync();

            friendList.Clear();
            foreach (Friend friend in newFriendList)
            {
                friend.CanChatWith = "True";
                friend.NewMsgCount = 0;
                friendList.Add(friend);
            }
        }

        public void IncreaseNewMsgCountOfFriendByOneInUI(string friendsPhoneNum)
        {
            for(int i = 0; i< friendList.Count; i++)
            {
                if((friendList[i].ID).Equals(friendsPhoneNum))
                {
                    Friend friendWithUpdatedCount = new Friend();
                    friendWithUpdatedCount.CanChatWith = friendList[i].CanChatWith;
                    friendWithUpdatedCount.FriendName = friendList[i].FriendName;
                    friendWithUpdatedCount.ID = friendList[i].ID;
                    friendWithUpdatedCount.NewMsgCount = friendList[i].NewMsgCount + 1;

                    friendList.RemoveAt(i);

                    friendList.Insert(i, friendWithUpdatedCount);
                }
            }
        }

    }
}
