using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using IMAPLayer;
using IMAPLayer.Models;
using DependencyInjection;

namespace EmailClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMailServer MailServer;
        private readonly Dictionary<int, MailHeaderObject> Refs = new Dictionary<int, MailHeaderObject>();
        private const int MaxCharLength = 60;
        public MainWindow()
        {
            InitializeComponent();
            //DI.Initialize();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginDTO login = new LoginDTO();
            LoginWindow window = new LoginWindow(login);
            window.Show();
            window.Closed += async (e, h) =>
            {
                MailServer = login.MailServer;
                if (MailServer == null)
                {
                    Close();
                    return;
                }
                BoxTree.ItemsSource = await MailServer.GetMailTree();
            };
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(MailServer != null) MailServer.Dispose();
        }
        private void ShowBrowser()
        {
            if (BrowserContainer.Opacity == 0)
            {
                ((Storyboard)BrowserContainer.Resources["FadeIn"]).Begin();
            }
        }
        private async void MailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 1)
            {
                MailHeaderObject o = (MailHeaderObject)e.AddedItems[0];
                var res = await MailServer.GetMail(o.Id);
                ShowBrowser();
                if(res.TryGetValue("text/html", out string body))
                {
                    Browser.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>" + body);
                }
                else
                {
                    Browser.NavigateToString("<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>" + res.Values.First());
                }
                SubjectLabel.Content = $"Subject: {Refs[o.Id].Subject}";
                FromLabel.Content = $"From: {Refs[o.Id].From}";
            }
        }
        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if(e.Uri != null)
            {
                var ps = new ProcessStartInfo(e.Uri.AbsoluteUri)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
                e.Cancel = true;
            }
        }
        private async void BoxTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MailNode n = (MailNode)e.NewValue;
            if (!n.IsSelectable) return;
            ObservableCollection<MailHeaderObject> Mails = new ObservableCollection<MailHeaderObject>();
            Refs.Clear();
            MailList.ItemsSource = Mails;
            await MailServer.SelectMailBox(n.FullName);
            foreach (var mail in (await MailServer.GetMails(50)).Reverse())
            {
                MailHeaderObject o = new MailHeaderObject
                {
                    Id = mail.Id,
                    From = mail.From.Split('<')[0].Trim(),
                    Subject = (mail.Subject.Length > MaxCharLength) ? mail.Subject[0..(MaxCharLength - 3)] + "..." : mail.Subject
                };
                Refs.Add(o.Id, mail);
                Mails.Add(o);
            }
        }
    }
}
