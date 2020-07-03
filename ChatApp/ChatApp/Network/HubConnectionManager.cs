using ChatApp.Configuration;
using ChatApp.GlobalConstants;
using ChatApp.Models;
using ChatApp.Utilities;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ChatApp.Network
{
    public class HubConnectionManager 
    {
        protected HubConnection hubConnection;

        //@page the page that you are attempting connection from
        public async Task AttemptToConnectToHub(ContentPage page)
        {
            bool notConnected = true;
            while (notConnected) //keep trying to connect
            {
                try
                {
                    page.Title = "Connecting to server......";
                    string chatHubUrl = await GetChatHubUrl();
                    //hubConnection = new HubConnectionBuilder()
                    //                .WithUrl(ApplicationConstants.ChutHubUrl)
                    //                .Build();
                    hubConnection = new HubConnectionBuilder()
                                   .WithUrl(chatHubUrl)
                                    .Build();
                    await hubConnection.StartAsync();

                    //Connection succeded at this point because no exception was thrown
                    notConnected = false;

                    page.Title = "";
                }
                catch (HttpRequestException exception)
                {
                    //Handle the exception. Maybe pop up a message saying unable to connect to chat hub, try again
                    page.Title = "";
                    await page.DisplayAlert("Failed to connect to server", "Either the server is down or your internet is not working", "Try again");
                }
            }
        }

        private async Task<string> GetChatHubUrl()
        {
            AppConfiguration appConfiguration = App.AppConfiguration;
            await appConfiguration.ConstructAppConfiguration();
            
            string chatHubUrl;
            if (appConfiguration.IsUsingLocalServerHost())
                chatHubUrl = appConfiguration.GetLocalServerHost();
            else
                chatHubUrl = appConfiguration.GetRemoteServerHost();
            return chatHubUrl;
        }

        public HubConnection GetHubConnection()
        {
            return hubConnection;
        }

        public async Task DiposeHubConnectionAsync()
        {
            await hubConnection.DisposeAsync();
        }

        //Checks if the user needs a msg table in the database of the chathub. If so, then creates one.
        //Precondition: @yourPhoneNum is not null, ConnectToHub() called before this is called
        public async Task CheckIfYouNeedsMsgTableInChatHub(string yourPhoneNum)
        {
            await hubConnection.InvokeAsync("CheckIfUserNeedsMsgTable", yourPhoneNum);
            await hubConnection.DisposeAsync();
            return;
        }
    
    }


}
