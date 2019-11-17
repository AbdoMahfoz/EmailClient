using IMAPLayer;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EmailClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginDTO login;
        public LoginWindow(LoginDTO login)
        {
            InitializeComponent();
            this.login = login;
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IMailServer MailServer = MailFactory.CreateFor(EmailInput.Text);
            if(MailServer == null)
            {
                if(string.IsNullOrWhiteSpace(IMAPInput.Text))
                {
                    MessageBox.Show("Unknown IMAP provider, please input the server address:port and if it needs SSL");
                    return;
                }
                string[] args = IMAPInput.Text.Split(':');
                if(args.Length != 2)
                {
                    MessageBox.Show("Incorrect IMAP server format. Must be {server:port}");
                    return;
                }
                MailServer = MailFactory.CreateCustom(args[0].Trim(), int.Parse(args[1]), (bool)SSL.IsChecked);
            }
            else if(!string.IsNullOrWhiteSpace(IMAPInput.Text))
            {
                var res = MessageBox.Show("This provider's IMAP server is known but a custom IMAP server has been provided. Use the custom one?",
                                          "Custom IMAP server", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(res == MessageBoxResult.Yes)
                {
                    MailServer.Dispose();
                    string[] args = IMAPInput.Text.Split(':');
                    if (args.Length != 2)
                    {
                        MessageBox.Show("Incorrect IMAP server format. Must be {server:port}");
                        return;
                    }
                    MailServer = MailFactory.CreateCustom(args[0].Trim(), int.Parse(args[1]), (bool)SSL.IsChecked);
                }
            }
            EmailInput.IsEnabled = false;
            PasswordInput.IsEnabled = false;
            LoginButton.IsEnabled = false;
            if(await MailServer.Login(EmailInput.Text, PasswordInput.Password))
            {
                login.MailServer = MailServer;
                Close();
                return;
            }
            EmailInput.IsEnabled = true;
            PasswordInput.IsEnabled = true;
            LoginButton.IsEnabled = true;
            MailServer.Dispose();
            EmailInput.Background = Brushes.Red;
            PasswordInput.Background = Brushes.Red;
            await Task.Delay(2000);
            EmailInput.Background = Brushes.White;
            PasswordInput.Background = Brushes.White;
        }
    }
}
