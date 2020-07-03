using ChatApp.Models;
using Plugin.ContactService.Shared;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;


namespace ChatApp.Friends
{
    public class FriendsManagement
    {
       
        //return list of friends
        public async System.Threading.Tasks.Task<ObservableCollection<Friend>> returnListOfFriendsAsync()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<ContactsPermission>();
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Contacts))
                    {
                        //show your own request permission dialog.
                    }
                    status = await CrossPermissions.Current.RequestPermissionAsync<ContactsPermission>();
                }

                if (status == PermissionStatus.Granted)
                {
                    //Query permission
                    var contacts = await Plugin.ContactService.CrossContactService.Current.GetContactListAsync();
                    var friendList = new ObservableCollection<Friend>();
                    foreach(Contact contact in contacts)
                    {
                        string formattedPhoneNumber = convertPhoneNumberToCorrectFormat(contact.Number);
                        Friend friend = new Friend(formattedPhoneNumber, contact.Name);
                        friendList.Add(friend);
                    }
                    return friendList;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //location denied
                    Console.WriteLine("CONTACT PERMISSION NOT GRANTED. CAN'T DO ANYTHING");
                }
            }
            catch (Exception ex)
            {
                //Something went wrong
            }

            return null;
        }

        private string convertPhoneNumberToCorrectFormat(string unformattedPhoneNum)
        {
            string formattedPhoneNumber = string.Concat(unformattedPhoneNum.Where(char.IsDigit)); //remove the non numbers from the phone number so instead of 1-415-676-3432, you have 14156763432
            int numOfDigits = formattedPhoneNumber.Length;
            if(numOfDigits > 10)
            {
                formattedPhoneNumber = formattedPhoneNumber.Substring(1, numOfDigits - 1); //remove the first digit so instead of 14153456789 you have 4153456789
            }
            return formattedPhoneNumber;
        }
    }
}
