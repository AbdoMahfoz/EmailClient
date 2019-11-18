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
using System.Threading.Tasks;
using System.IO;

namespace Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMailLogic MailLogic;
        private readonly IMailClassifier MailClassifier;
        private readonly ObservableCollection<MailHeader> Spam = new ObservableCollection<MailHeader>();
        private readonly ObservableCollection<MailHeader> Banking = new ObservableCollection<MailHeader>();
        private readonly ObservableCollection<MailHeader> Promotions = new ObservableCollection<MailHeader>();
        private readonly ObservableCollection<MailHeader> Updates = new ObservableCollection<MailHeader>();
        private Mail CurrentMail;
        public MainWindow()
        {
            DI.Initialize();
            MailLogic = DI.Get<IMailLogic>();
            MailClassifier = DI.Get<IMailClassifier>();
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
                var boxes = await MailLogic.GetMailBoxTree();
                var mails = await MailLogic.GetMails(await MailLogic.GetAllMailBox(), 0, 200);
                BoxTree.ItemsSource = boxes;
                foreach (var mail in mails)
                {
                    await MailClassifier.Classify(mail);
                    switch (mail.Category)
                    {
                        case MailCategory.Banking:
                            Banking.Add(mail);
                            break;
                        case MailCategory.Spam:
                            Spam.Add(mail);
                            break;
                        case MailCategory.Promotion:
                            Promotions.Add(mail);
                            break;
                        case MailCategory.Updates:
                            Updates.Add(mail);
                            break;
                    }
                }
                BankingButton.IsEnabled = true;
                PromotionsButton.IsEnabled = true;
                SpamButton.IsEnabled = true;
                UpdatesButton.IsEnabled = true;
            };
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MailLogic != null) MailLogic.Dispose();
        }
        private void CategoryClicked(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content)
            {
                case "Banking":
                    MailList.ItemsSource = Banking;
                    break;
                case "Spam":
                    MailList.ItemsSource = Spam;
                    break;
                case "Promotions":
                    MailList.ItemsSource = Promotions;
                    break;
                case "Updates":
                    MailList.ItemsSource = Updates;
                    break;
            }
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
            if (e.AddedItems.Count == 1)
            {
                MailHeader o = (MailHeader)e.AddedItems[0];
                var res = await MailLogic.GetMail(o);
                CurrentMail = res;
                ShowBrowser();
                string head = "<head><meta http-equiv='Content-Type' content='text/html;charset=UTF-8'></head>";
                if (res.Body.TryGetValue("text/html", out string body))
                {
                    Browser.NavigateToString(head + body);
                }
                else
                {
                    Browser.NavigateToString(head + res.Body.Values.First());
                }
                SubjectLabel.Content = $"Subject: {o.Subject}";
                FromLabel.Content = $"From: {o.From}";
                if(res.Attachments.Count > 0)
                {
                    AttachmentsBox.IsEnabled = true;
                    AttachmentDownloadButton.IsEnabled = true;
                    AttachmentsBox.ItemsSource = res.Attachments.Keys;
                    AttachmentsBox.SelectedIndex = 0;
                }
                else
                {
                    AttachmentsBox.IsEnabled = false;
                    AttachmentDownloadButton.IsEnabled = false;
                    AttachmentsBox.ItemsSource = null;
                }
            }
        }
        private void Browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
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
            MailList.ItemsSource = new ObservableCollection<MailHeader>(await MailLogic.GetMails(n, 0, 200));
        }
        private void AttachmentDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string key = AttachmentsBox.Text;
            File.WriteAllBytes(key, CurrentMail.Attachments[key]);
            MessageBox.Show("File saved successfully");
        }
    }
}
