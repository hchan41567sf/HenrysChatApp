using ChatApp.GlobalConstants;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ChatApp.AppStartup
{
    public class AppInitiator
    {
        private Page initialPage;
        public AppInitiator(Page initialPage)
        {
            this.initialPage = initialPage;
        }
        public async Task IniatiateAppForFirstTimeStartup()
        {
            await DisplayEnterPhoneNumberPrompt(initialPage);
            Preferences.Set(ApplicationConstants.FIRST_TIME_START_APP_PREF, false);
            return;
        }

        public async Task GenerateEmptyTableInChatHubDatabase(HubConnection hubConnection)
        {
            string yourPhoneNumber = Preferences.Get(ApplicationConstants.YOUR_PHONE_NUM_PREF, ApplicationConstants.NO_PHONE_NUMBER);
            await hubConnection.InvokeAsync("GenerateEmptyUserTableInChatHub", yourPhoneNumber);
            Preferences.Set(ApplicationConstants.JUST_ENTERED_PHONENUM_PREF, false);
        }

        private async Task DisplayEnterPhoneNumberPrompt(Page initialPage)
        {
            string yourPhoneNumber = await initialPage.DisplayPromptAsync("Enter your phone number", ApplicationConstants.PHONE_PROMPT_DESC,
                                           maxLength: ApplicationConstants.NUM_DIGITS_PHONE_NUM);

            if (yourPhoneNumber == null) //if user canceled the pop up, start over again
            {
                DisplayEnterPhoneNumberPrompt(initialPage);
                return;
            }

            if(isPhoneNumInCorrectFormat(yourPhoneNumber))
            {
                Preferences.Set(ApplicationConstants.YOUR_PHONE_NUM_PREF, yourPhoneNumber);
            }
            else
            {
                DisplayEnterPhoneNumberPrompt(initialPage);
            }

        }

        private bool isPhoneNumInCorrectFormat(string yourPhoneNumber)
        {
            bool isPhoneNumberNumbersOnly = IsDigitsOnly(yourPhoneNumber);

            bool isPhoneNumberCorrectNumOfDigits;
            if (yourPhoneNumber.Length == ApplicationConstants.NUM_DIGITS_PHONE_NUM)
                isPhoneNumberCorrectNumOfDigits = true;
            else
                isPhoneNumberCorrectNumOfDigits = false;

            return isPhoneNumberNumbersOnly && isPhoneNumberCorrectNumOfDigits;
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
