using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CRMPhone.Dto;

namespace CRMPhone
{
    /// <summary>
    /// Логика взаимодействия для RequestDialog.xaml
    /// </summary>
    public partial class RequestDialog : Window
    {
        private RequestDialogViewModel _context;

        public RequestDialog(RequestDialogViewModel context)
        {
            _context = context;

            Owner = Application.Current.MainWindow;
            DataContext = _context;

            _context.SetView(this);
            InitializeComponent();
        }

        private void SelectCurrentItem(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListBoxItem;
            _context.SelectedContact = (ContactDto)item.Content;
        }
    }
}
