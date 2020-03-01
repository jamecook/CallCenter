using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using ClientPhone.Services;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;

namespace CRMPhone.ViewModel
{
    public class LoginContext : INotifyPropertyChanged
    {
        public List<UserDto> Users { get; set; }
        public UserDto CurrentUser { get; set; }

        public string UserName { get; set; }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public LoginContext()
        {
            try
            {
                var localIp = GetLocalIpAddress();
                AppSettings.SetSipInfo(RestRequestService.GetSipInfoByIp(localIp));
                var companyIdStr = ConfigurationManager.AppSettings["CompanyId"];
                if (!int.TryParse(companyIdStr, out int companyId))
                {
                    companyId = 1;
                }
                Users = new List<UserDto>(RestRequestService.GetDispatchers(companyId));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка в приложении! Приложение будет закрыто!\r\n" + ex.Message, "Ошибка");
                Environment.Exit(0);
                //Application.Current.Shutdown();
            }
        }

        public LoginView View { get; set; }

        private bool _canExecute = true;
        private ICommand _loginCommand;
        public ICommand LoginCommand { get { return _loginCommand ?? (_loginCommand = new CommandHandler(Login, _canExecute)); } }

        private bool _canCancelExecute = true;
        private ICommand _cancelCommand;
        public ICommand CancelCommand { get { return _cancelCommand ?? (_cancelCommand = new CommandHandler(Cancel, _canCancelExecute)); } }

        private void Cancel()
        {
            View.Cancel();
        }

        private void Login()
        {
            CurrentUser = RestRequestService.Login(UserName, Password, AppSettings.SipInfo?.SipUser);
            if (CurrentUser != null)
            {
                AppSettings.SetUser(CurrentUser);
                View.Done();
            }
            else
            {
                MessageBox.Show("Введены неверные логин или пароль!", "Ошибка авторизации");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}