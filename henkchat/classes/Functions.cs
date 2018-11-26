using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace HenkChat
{
    class Functions
    {
        const int MIN_MESSAGE_LENGTH = 500;

        public static ListViewItem CreateMessageBox(string Message, string MeUserName)
        {
            ListViewItem item = new ListViewItem();
            item.Margin = new Thickness(0, 0, 0, 3);
            item.MinWidth = MIN_MESSAGE_LENGTH;

            Grid content = new Grid();

            TextBlock text = new TextBlock();
            text.FontSize = 14;
            text.TextWrapping = TextWrapping.Wrap;
            text.Margin = new Thickness(0, 15, 0, 0);
            text.Foreground = new SolidColorBrush(Colors.White);

            TextBlock header = new TextBlock();
            header.Foreground = new SolidColorBrush(Colors.White);
            header.FontSize = 9;


            if (Message.StartsWith(MeUserName + ":"))
            {
                header.Text = "you, " + DateTime.Now.ToString("HH:mm");
                text.Text = Message.Replace(MeUserName + ":", "");

                item.Background = new SolidColorBrush(Color.FromArgb(100, 60, 60, 60));
                item.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else if (Message.Contains(":"))
            {
                int place = Message.IndexOf(":");
                string msg = Message.Substring(place);
                string head = Message.Replace(msg, "");

                header.Text = head + ", " + DateTime.Now.ToString("HH:mm");
                text.Text = msg.Remove(0, 1);

                item.Background = new SolidColorBrush(Color.FromArgb(100, 80, 80, 80));
                item.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                item.MinWidth = 0;

                header.Visibility = Visibility.Collapsed;
                text.Text = Message;

                header.HorizontalAlignment = HorizontalAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                item.Background = new SolidColorBrush(Color.FromArgb(100, 40, 40, 40));
                item.HorizontalAlignment = HorizontalAlignment.Center;
            }
            content.Children.Add(header);
            content.Children.Add(text);

            item.Content = content;
            return item;
        }
    }
}
