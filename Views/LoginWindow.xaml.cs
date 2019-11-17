using BussinessLogic.Interfaces;
using System.Windows;

namespace Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IMailLogic MailLogic;
        public LoginWindow(IMailLogic MailLogic)
        {
            InitializeComponent();
            this.MailLogic = MailLogic;
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            EmailInput.IsEnabled = false;
            PasswordInput.IsEnabled = false;
            LoginButton.IsEnabled = false;
            IMAPInput.IsEnabled = false;
            SSL.IsEnabled = false;
            LoginResult res;
            if (!string.IsNullOrWhiteSpace(IMAPInput.Text))
            {
                string[] imapServer = IMAPInput.Text.Split(':');
                if (imapServer.Length != 2)
                {
                    MessageBox.Show("Incorrect IMAP server format. Must be {server:port}");
                    return;
                }
                if (!int.TryParse(imapServer[1], out int port))
                {
                    MessageBox.Show("Port is a number... you did this before?");
                    return;
                }
                res = await MailLogic.Login(EmailInput.Text, PasswordInput.Password, imapServer[0], port, SSL.IsChecked);
            }
            else
            {
                res = await MailLogic.Login(EmailInput.Text, PasswordInput.Password);
            }
            switch (res)
            {
                case LoginResult.Ok:
                    Close();
                    return;
                case LoginResult.UnknownProvider:
                    if (string.IsNullOrWhiteSpace(IMAPInput.Text))
                    {
                        MessageBox.Show("Unknown IMAP provider, please input the server address:port and if it needs SSL");
                        return;
                    }
                    break;
                case LoginResult.WrongCredintials:
                    MessageBox.Show("Wrong email or password");
                    break;
                case LoginResult.IncorrectProviderData:
                    MessageBox.Show("Couldn't connect to supplied provider data");
                    break;
            }
            EmailInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            LoginButton.IsEnabled = true;
            IMAPInput.IsEnabled = true;
            SSL.IsEnabled = true;
        }
    }
}
