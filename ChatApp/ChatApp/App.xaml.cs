using ChatApp.Configuration;
using ChatApp.Data;
using ChatApp.Friends;
using ChatApp.GlobalConstants;
using ChatApp.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ChatApp
{
    public partial class App : Application
    {       
        static Database database;
        public static FriendsManagement friendsManagement;
        public static HubConnection hubConnection;
        public static AppConfiguration appConfiguration;

        public static AppConfiguration AppConfiguration
        {
            get
            {
                if (appConfiguration == null)
                {
                    appConfiguration = new AppConfiguration();
                }
                return appConfiguration;
            }
        }

        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "chatmessages.db3"));
                }
                return database;
            }
        }

        public static FriendsManagement FriendsManagement
        {
            get
            {
                if (friendsManagement == null)
                {
                    friendsManagement = new FriendsManagement();
                }
                return friendsManagement;
            }
        }


        public App()
        {
            InitializeComponent();

            var page = new NavigationPage(new MainPage());
            
            //Set the event handler that is called when a page is popped.
            page.Popped += OnNavigationPagePop;

            MainPage = page;
        }

        private async void OnNavigationPagePop(Object sender, NavigationEventArgs e)
        {
            Type pageType = (e.Page).GetType();
            string pageName = pageType.Name;
            if (pageName.Equals("ChatPage"))
            {
                //DISPOSE THE HUB CONNECTION OF CHAT PAGE
                await (e.Page as ChatPage).hubConnectionManager.DiposeHubConnectionAsync();

                await RequestUpdatedNewMsgCountForMainPageFromChatHub();
            }
        }

        private async Task RequestUpdatedNewMsgCountForMainPageFromChatHub()
        {
            IReadOnlyList<Page> navigationStackOfPages = MainPage.Navigation.NavigationStack;
            MainPage mainContentPage = (MainPage)navigationStackOfPages[0];
            string yourPhoneNumber = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
            await mainContentPage.GetHubConnectionManager().GetHubConnection().InvokeAsync("GetNewMessageCountForEachFriendThatLeftAMessage", yourPhoneNumber);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

    }
}
