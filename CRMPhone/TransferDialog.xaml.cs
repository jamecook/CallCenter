using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CRMPhone
{
    /// <summary>
    /// Логика взаимодействия для TransferDialog.xaml
    /// </summary>
    public partial class TransferDialog : Window
    {
        private TrasferDialogViewModel _context;
        public TransferDialog(TrasferDialogViewModel context)
        {
            _context = context;
            DataContext = _context;
            _context.SetView(this);
            InitializeComponent();
        }
    }
}
