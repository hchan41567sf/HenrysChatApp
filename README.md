**For now, this app only works on phones with a phone number in the United States, and it may not work on iOS because I have not tested it on iOS.**

# Project Overview

HenrysChatApp is a mobile chat client app that users can use to chat with each other in a chat room setting. 
This app is similar to WhatsApp and Telegram. Users can chat with other users in their contact list. A few of the features include 
being able to see if a message has been seen by the recepient, and seeing how many new messages have been left for you by each of your contacts.
It is developed using Xamarin Forms and Visual Studio 2019. Technically, the app should work on both Android and iOS because Xamarin Forms is a 
multiplatform development tool, but I have only tested it on Android so it **may not work on iOS**.


# HenrysChatApp: How to build and use the app

### 1) Download, setup, and build the ChatHub
First start by downloading and then building the [source code](https://github.com/hchan41567sf/ChatHubApp2) for the ChatHub. The instructions for setting up 
the ChatHub is provided in the link. The ChatHub is needed for the client apps (this app) to send chat messages to one another.
 Every chat message sent goes through the ChatHub. Also, messages left behind to users that are not online are handled by the ChatHub. 
 These are a few of the things the ChatHub is responsible for. <br>
 
 ### 2) Download this project (HenrysChatApp) and make some changes. 
 This project is the chat app client. Multiple users will use this app on different phones to chat with each other. Download 
 the project and run **ChatApp.sln** using Visual Studio 2019 (I have not tested it on other versions). Now make some changes to some 
 configuration files so that your chat client knows the url it needs to use to connect to the Chathub.
 
 #### Android:
 In the **solution** window, select the **ChatApp.Android** project. Double click the **Assets** folder and open **configuration.json**. This file
 contains the URLs that the client will use to connect to the ChatHub, and you can specify wether you are connecting to the ChatHub running on your local machine or the one 
 on your Azure server.
 Here is what the file looks like with comments telling you what to change: <br>
 ```
 {
    "LocalHost": "http://your-local-ip-address/chatHub",   //replace your-local-ip-address with the actual local ip address of the computer hosting the Chathub. Add port 5001 to the address. Example: "http://192.168.42.81:5001/chatHub"
    "RemoteHost": "https://your-azure-url/chatHub",        //replace your-azure-url with the url of your azure server that you set up in step 1 when setting up the Chathub. Example: "https://mychathubapp.azurewebsites.net/chatHub"
    "IsUsingLocalHost": true                               //Keep this value at true if you want to use the ChatHub on your local machine. Change it to false
                                                           //if you want to use the ChatHub on your Azure server.
}
 ```
 
 #### iOS:
 In the *solution window*, select the *ChatApp.iOS* project. Double click the *Resources* folder and open *configuration.json*. This file
 contains the URLs to the ChatHub, and you can specify wether you are connecting to the ChatHub running on your local machine or the one 
 on your Azure server.
 Here is what the file looks like with comments telling you what to change: <br>
```
 {
    "LocalHost": "http://your-local-ip-address/chatHub",   //replace your-local-ip-address with the actual local ip address of the computer hosting the Chathub. Add port 5001 to the address. Example: "http://192.168.42.81:5001/chatHub"
    "RemoteHost": "https://your-azure-url/chatHub",        //replace your-azure-url with the url of your azure server that you set up in step 1 when setting up the Chathub. Example: "https://mychathubapp.azurewebsites.net/chatHub"
    "IsUsingLocalHost": true                               //Keep this value at true if you want to use the ChatHub on your local machine. Change it to false
                                                           //if you want to use the ChatHub on your Azure server.
}
 ```
 
 ### 3) Build this project (HenrysChatApp)
  Connect your phone to your computer or use a virtual phone, select the phone in visual studio, then click **Debug > Start Debugging** to build the app onto your phone. 
  This will start the app but don't do anything yet. Just close it. You now have the app installed on your phone. 
  Do the same for another phone or virtual phone so you can test the app by sending chat messages between the phones. Close the app on this phone too.
  
  ### 4) Use the app on your phones
 If you set *IsUsingLocalHost* to true in the **configuration.json** file, then you will have to first start up the ChatHub
 on your local machine. If *IsUsingLocalHost* is set to false then the ChatHub is already open and running in your Azure server so you dont 
 have to do anything.<br><br>
 Before you start the apps on the phones, for each phone, make sure the other phone is in your contacts. If not then add it. Now that you have done that, start the apps on both phones.
 For each phone, look in the apps contact list and start a chat with the other phone. Start chatting to see if the app works as expected.
