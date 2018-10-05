using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using MySql.Data.MySqlClient;
using RequestServiceImpl;
using RequestServiceImpl.Dto;
//using SIPEVOActiveXLib;

namespace CRMPhone.ViewModel
{
    public partial class CRMContext
    {
        /*
        private SIPClientCtl _sipClient;
        private void GetCallFromQuery(object currentChannel)
        {
            var item = currentChannel as ActiveChannelsDto;
            if (item == null || string.IsNullOrEmpty(item.Channel))
                return;
            if (_sipClient.CallState[0] != CallState.CallState_Free)
            {
                MessageBox.Show("Невозможно взять из очереди если занята первай линия!");
                return;
            }
            string callId = string.Format("sip:{0}@{1}", "123123321", _serverIP);
            _sipClient.PhoneLine = 0;
            _sipClient.Connect(callId);
            var bridgeThread = new Thread(BridgeFunc); //Создаем новый объект потока (Thread)

            bridgeThread.Start(item); //запускаем поток

        }

        public void InitSipNew()
        {
            #region Создаем и настраиваем SIP-агента
            try
            {
                if (_sipClient == null)
                {
                    _sipClient = new SIPClientCtl();

                    _sipClient.OnConnected += SipClientOnConnected;
                    _sipClient.OnRegistrationSuccess += SipClientOnRegistrationSuccess;
                    _sipClient.OnUnregistration += SipClientOnUnregistration;
                    _sipClient.OnRegistrationFailure += SipClientOnRegistrationFailure;

                    _sipClient.OnConnectingLine += SipClientOnConnectingLine;
                    _sipClient.OnTerminatedLine += SipClientOnTerminatedLine;
                    _sipClient.OnHold += SipClientOnHold;

                    _sipClient.LogEnabled = false;
                    _sipClient.UserID = _sipUser;
                    _sipClient.LoginID = _sipUser;
                    _sipClient.Password = _sipSecret;
                    _sipClient.RegistrationProxy = _serverIP;
                    _sipClient.DisplayName = _sipUser;
                    _sipClient.Initialize(null);
                    _sipClient.TCPPort = -1;
                    _sipClient.Register();
                    _sipClient.PlayRingtone = false;

                    _sipClient.MaxPhoneLines = 2;
                    _sipClient.NoiseReduction = false;
                    _sipClient.AEC = false;
                    //_sipClient.EchoTail = 0;

                    //_sipClient.ConferenceJoin();
                    //_sipClient.ConferenceRemove();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при подключении к АТС!\r\n" +
                                "Для использования звонков необходимо перезагрузить приложение!\r\n"
                                + ex.Message, "Ошибка");
            }
            #endregion
        }

        private void SipClientOnHold(string sFromUri, string sLocalUri, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromUri));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Hold";
                SipLines[nLine].Uri = sFromUri;
                SipLines[nLine].Phone = phoneNumber;
            }
        }

        private void SipClientOnConnectingLine(string sFromUri, string sLocalUri, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromUri));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Incoming";
                SipLines[nLine].Uri = sFromUri;
                SipLines[nLine].Phone = phoneNumber;
                SelectedLine = SipLines.FirstOrDefault(s => s.Id == nLine);
            }
            CallFromServiceCompany = _requestService?.ServiceCompanyByIncommingPhoneNumber(phoneNumber);
            SipState = $"Входящий вызов от {phoneNumber}";
            IncomingCallFrom = phoneNumber;

            _ringPlayer.PlayLooping();
            //Bring To Front
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (!mainWindow.IsVisible)
                {
                    mainWindow.Show();
                }

                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
                mainWindow.Topmost = true; // important
                mainWindow.Topmost = false; // important
                mainWindow.Focus(); // important
            }));
        }

        private void SipClientOnRegistrationFailure(string sLocalUri, int nCause)
        {
            SipState = $"Ошибка регистрации! {nCause}";
            _canRegistration = true;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnTerminatedLine(string sFromURI, string sLocalURI, int nStatusCode, string sStatusText, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromURI));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Free";
                SipLines[nLine].Uri = sFromURI;
                SipLines[nLine].Phone = phoneNumber;
            }

            SipState = $"Звонок завершен {phoneNumber}";
            _ringPlayer.Stop();
            IsMuted = false;
            _sipCallActive = false;
        }

        private void SipClientOnUnregistration(string sLocalURI)
        {
            SipState = "UnRegistered!";
            _canRegistration = true;
            _ringPlayer.Stop();
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnRegistrationSuccess(string slocaluri)
        {
            foreach (SipLine line in _sipLines)
            {
                line.State = "Free";
                line.Phone = "";
                line.Uri = "";
            }


            SipState = "Успешная регистрация!";
            _canRegistration = false;
            _sipCallActive = false;
            OnPropertyChanged(nameof(EnableRegistration));
        }

        private void SipClientOnConnected(string sFromURI, string sLocalURI, int nLine)
        {
            var phoneNumber = GetPhoneNumberFromUri(_sipClient.URLGetAOR(sFromURI));
            if (nLine < _maxLineNumber)
            {
                SipLines[nLine].State = "Connect";
                SipLines[nLine].Uri = sFromURI;
                SipLines[nLine].Phone = phoneNumber;
            }
            _ringPlayer.Stop();
            _sipCallActive = true;
            SipState = $"Связь установлена: {phoneNumber}";
        }

        public void Call()
        {
            if (_sipClient.CallState[SelectedLine.Id] == CallState.CallState_Inbound)
            {
                LastAnsweredPhoneNumber = IncomingCallFrom;
                _sipClient.PhoneLine = SelectedLine.Id;
                _sipClient.AcceptCall();
                return;
            }
            if (string.IsNullOrEmpty(_sipPhone))
                return;
            if (_sipClient.CallState[SelectedLine.Id] == CallState.CallState_Free)
            {
                string callId = string.Format("sip:{0}@{1}", _sipPhone, _serverIP);
                SipState = $"Исходящий вызов на номер {_sipPhone}";
                _sipClient.PhoneLine = SelectedLine.Id;
                _sipClient.Connect(callId);
            }
            else
            {
                MessageBox.Show("Линия занята, выберите другую линию!",
                    "Предупреждение");
            }
        }

        public void CallFromList()
        {
            if (SelectedCall == null)
                return;
            SipPhone = SelectedCall.CallerId;
            string callId = string.Format("sip:{0}@{1}", SelectedCall.CallerId, _serverIP);
            _sipClient.Connect(callId);
        }

        public void Mute()
        {
            IsMuted = !IsMuted;
            _sipClient.MicrophoneMuted = IsMuted;
        }

        private void Conference()
        {
            foreach (var sipLine in SipLines)
            {
                _sipClient.PhoneLine = sipLine.Id;
                _sipClient.ConferenceJoin();
            }
        }

        public void Transfer()
        {
            var phoneList = _requestService.GetTransferList();
            phoneList.Remove(phoneList.FirstOrDefault(p => p.SipNumber == _sipUser));
            var transferContext = new TrasferDialogViewModel(phoneList);
            var transfer = new TransferDialog(transferContext);
            transfer.Owner = Application.Current.MainWindow;
            if (transfer.ShowDialog() == true)
            {
                var phone = string.IsNullOrEmpty(transferContext.TransferPhone)
                    ? transferContext.ClientPhone.SipNumber
                    : transferContext.TransferPhone;
                string callId = string.Format("sip:{0}@{1}", phone, _serverIP);
                _sipClient.TransferCall(callId);
            }

        }
        public void Hold()
        {
            var callState = _sipClient.get_CallState(SelectedLine.Id);
            _sipClient.PhoneLine = SelectedLine.Id;
            if ((callState == CallState.CallState_LocalHeld) || (callState == CallState.CallState_RemoteHeld))
            {
                _sipClient.Unhold();
            }
            else
            {
                _sipClient.Hold();
            }
        }
        public void HangUp()
        {
            _sipClient.PhoneLine = SelectedLine.Id;
            _sipClient.Disconnect();
            IncomingCallFrom = "";
            CallFromServiceCompany = null;
        }


        public void Unregister()
        {
            using (var cmd = new MySqlCommand($"call CallCenter.LogoutUser({AppSettings.CurrentUser.Id})", AppSettings.DbConnection))
            {
                cmd.ExecuteNonQuery();
            }
            _sipClient?.UnRegister();
        }
    /**/
    }
}
