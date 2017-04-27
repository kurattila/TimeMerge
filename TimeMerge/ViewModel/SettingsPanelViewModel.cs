using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace TimeMerge.ViewModel
{
    public class SettingsPanelViewModel : ObservableBase
    {
        public string UserID
        {
            get { return Properties.Settings.Default.UserID; }
            set
            {
                Properties.Settings.Default.UserID = value;
                NotifyPropertyChanged(nameof(UserID));
                NotifyPropertyChanged(nameof(WebAccessAddress));
                NotifyPropertyChanged(nameof(WebAccessAddressNoPassword));
            }
        }

        public bool IsWebAccessOn
        {
            get { return Properties.Settings.Default.IsWebAccessOn; }
            set
            {
                if (Properties.Settings.Default.IsWebAccessOn != value)
                {
                    Properties.Settings.Default.IsWebAccessOn = value;
                    NotifyPropertyChanged(nameof(IsWebAccessOn));
                }
            }
        }

        public string WebAccessAddressNoPassword
        {
            get
            {
                string addr = (App.Current as App)?.Context.WebDavConnection.XmlAddressNoPassword();
                return addr;
            }
        }

        public string WebAccessAddress
        {
            get
            {
                string addr = (App.Current as App)?.Context.WebDavConnection.XmlAddress();
                return addr;
            }
        }

        ICommand _GotoWebAccessAddress;
        public ICommand GotoWebAccessAddress
        {
            get
            {
                if (_GotoWebAccessAddress == null)
                    _GotoWebAccessAddress = new RelayCommand((arg) =>
                    {
                        Process.Start(new ProcessStartInfo(WebAccessAddress));

                        CloseCommand.Execute(null);
                    });
                return _GotoWebAccessAddress;
            }
        }

        ICommand _SendMailWithWebAccessLink;
        public ICommand SendMailWithWebAccessLink
        {
            get
            {
                if (_SendMailWithWebAccessLink == null)
                    _SendMailWithWebAccessLink = new RelayCommand((arg) => onSendMailWithWebAccessLink());
                return _SendMailWithWebAccessLink;
            }
        }

        private void onSendMailWithWebAccessLink()
        {
            // Render our E-mail request so that the default Mail Client will be able to send it
            // (syntax is e.g. "mailto:abc@abc.com?cc=def@def.com&subject=this is my subject&body=this is my body")
            var mailMessage = new StringBuilder();
            mailMessage.Append("mailto:" + "");
            mailMessage.Append("?subject=TimeMerge: prístup na web");
            mailMessage.Append("&body=" + WebAccessAddress);

            Utils.SendMail.Send(mailMessage.ToString());

            CloseCommand.Execute(null);
        }

        public string MailToRecipients
        {
            get { return Properties.Settings.Default.MailToRecipients; }
            set
            {
                Properties.Settings.Default.MailToRecipients = value;
                NotifyPropertyChanged(nameof(MailToRecipients));
            }
        }

        public string MailCcRecipients
        {
            get { return Properties.Settings.Default.MailCcRecipients; }
            set
            {
                Properties.Settings.Default.MailCcRecipients = value;
                NotifyPropertyChanged(nameof(MailCcRecipients));
            }
        }

        public bool DeskBandShowsMonthBalance
        {
            get { return Properties.Settings.Default.DeskBandShowsMonthBalance; }
            set
            {
                Properties.Settings.Default.DeskBandShowsMonthBalance = value;
                NotifyPropertyChanged(nameof(DeskBandShowsMonthBalance));
            }
        }

        public bool IsHomeOfficeDetectionOn
        {
            get { return Properties.Settings.Default.IsHomeOfficeDetectionOn; }
            set
            {
                Properties.Settings.Default.IsHomeOfficeDetectionOn = value;
                NotifyPropertyChanged(nameof(IsHomeOfficeDetectionOn));
            }
        }

        static ICommand _NullCommand = new RelayCommand(arg => { });
        ICommand _CloseCommand = _NullCommand;
        public ICommand CloseCommand
        {
            get
            {
                return _CloseCommand;
            }
            set
            {
                if (_CloseCommand != value)
                {
                    _CloseCommand = value;
                    NotifyPropertyChanged(nameof(CloseCommand));
                }
            }
        }
    }
}
