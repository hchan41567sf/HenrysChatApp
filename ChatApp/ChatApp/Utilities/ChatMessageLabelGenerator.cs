using ChatApp.Models;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ChatApp.Utilities
{
    public static class ChatMessageLabelGenerator
    {
        private static double NAME_LABEL_FONT_SIZE = 18.0;

        public static void CreateAndDisplayChatMessageLabel(ChatMessage chatMessage, Friend friendChattingWith, StackLayout chat_messages_area)
        {
            //Git test. Added Comment 
            if (chatMessage.Writer.Equals("Friend"))
                CreateChatMessageBoxForMsgSentByFriend(chatMessage, friendChattingWith, chat_messages_area);
            else
                CreateChatMessageBoxForMsgSentByYou(chatMessage, chat_messages_area);
        }

        private static void CreateChatMessageBoxForMsgSentByFriend(ChatMessage chatMessage, Friend friendChattingWith, StackLayout chat_messages_area)
        {
            var labelForName = new Label { Text = friendChattingWith.FriendName + ":", FontSize = NAME_LABEL_FONT_SIZE, FontAttributes = FontAttributes.Bold };
            var labelForChatMsg = new Label { Text = chatMessage.Msg };

            StackLayout stackLayoutForChatMessage = new StackLayout
            {
                StyleId = "friend_chat_msg",
                Margin = new Thickness(0, 0, 0, 15),
                Children =
                {
                    new Frame
                    {
                        Margin = new Thickness(3, 3, 3, 1),
                        BackgroundColor = Color.FromHex("#F0FFFF"),
                        CornerRadius = 5.0f,
                        Padding = new Thickness(10),
                        Content =
                        new StackLayout
                        {
                            Children =
                            {
                                labelForName,
                                labelForChatMsg
                            }
                        }
                    },

                    new Frame
                    {
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(3, 1, 1, 1),
                        BackgroundColor = Color.FromHex("#F6F6F6"),
                        CornerRadius = 3.0f,
                        Padding = new Thickness(5),
                        Content = new Label{Text = "Written on: " + chatMessage.Date, FontSize = 8.0}
                    },
                }
            };

            chat_messages_area.Children.Add(stackLayoutForChatMessage);
        }

        private static void CreateChatMessageBoxForMsgSentByYou(ChatMessage chatMessage, StackLayout chat_messages_area)
        {
            var labelForName = new Label { Text = "You:", FontSize = NAME_LABEL_FONT_SIZE, FontAttributes = FontAttributes.Bold };
            var labelForChatMsg = new Label { Text = chatMessage.Msg };

            //Determine what to display in the "Message seen or not" label
            string msgSeenStatus;
            Console.WriteLine(chatMessage.SeenByRecipient);
            if ((chatMessage.SeenByRecipient).Equals("yes"))
                msgSeenStatus = "Msg seen";
            else
                msgSeenStatus = "Msg not seen";

            StackLayout stackLayoutForChatMessage = new StackLayout
            {
                StyleId = (chatMessage.MsgID).ToString(),
                Margin = new Thickness(0, 0, 0, 15),
                Children =
                {
                    new Frame
                    {
                        Margin = new Thickness(3, 3, 3, 1),
                        BackgroundColor = Color.FromHex("#F0FFFF"),
                        CornerRadius = 5.0f,
                        Padding = new Thickness(10),
                        Content =
                        new StackLayout
                        {
                            Children =
                            {
                                labelForName,
                                labelForChatMsg
                            }
                        }
                    },

                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            new Frame
                            {
                                Margin = new Thickness(3, 1, 1, 1),
                                BackgroundColor = Color.FromHex("#F6F6F6"),
                                CornerRadius = 3.0f,
                                Padding = new Thickness(5),
                                Content = new Label{Text = "Msg sent", FontSize = 8.0}
                            },

                            new Frame
                            {
                                Margin = new Thickness(1, 1, 1, 1),
                                BackgroundColor = Color.FromHex("#F6F6F6"),
                                CornerRadius = 3.0f,
                                Padding = new Thickness(5),
                                //Content = new Label{Text = msgSeenStatus, FontSize = 8.0, StyleId = (chatMessage.MsgID).ToString()} //The id of the message seen or not label is set to the chat message ID.
                                                                                                                                 //So we can reference this label later when we need to change the text of this label
                                Content = new Label{Text = msgSeenStatus, FontSize = 8.0}
                            },

                            new Frame
                            {
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                Margin = new Thickness(3, 1, 1, 1),
                                BackgroundColor = Color.FromHex("#F6F6F6"),
                                CornerRadius = 3.0f,
                                Padding = new Thickness(5),
                                //Content = new Label{Text = "Place date here", FontSize = 8.0}
                                Content = new Label{Text = "Written on: " + chatMessage.Date, FontSize = 8.0}
                            }
                        }
                    }
                }
            };

            chat_messages_area.Children.Add(stackLayoutForChatMessage);
        }
    }
}
