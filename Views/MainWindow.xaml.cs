using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using ViewModels;
using DependencyInjection;
using BussinessLogic.Interfaces;

namespace Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMailLogic MailLogic;
        private readonly Dictionary<int, MailHeader> Refs = new Dictionary<int, MailHeader>();
        private const int MaxCharLength = 60;
        public MainWindow()
        {
            DI.Initialize();
            MailLogic = DI.Get<IMailLogic>();
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow window = DI.Get<LoginWindow>();
            window.Show();
            window.Closed += async (e, h) =>
            {
                if (!MailLogic.IsAuthenticated)
                {
                    Close();
                    return;
                }
                BoxTree.ItemsSource = await MailLogic.GetMailBoxTree();
            };
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(MailLogic != null) MailLogic.Dispose();
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
                MailHeader o = (MailHeader)e.AddedItems[0];
                var res = await MailLogic.GetMail(o);
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
            MailBox n = (MailBox)e.NewValue;
            if (!n.IsSelectable) return;
            ObservableCollection<MailHeader> Mails = new ObservableCollection<MailHeader>();
            Refs.Clear();
            MailList.ItemsSource = Mails;
            foreach (var mail in await MailLogic.GetMails(n))
            {
                MailHeader o = new MailHeader
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
