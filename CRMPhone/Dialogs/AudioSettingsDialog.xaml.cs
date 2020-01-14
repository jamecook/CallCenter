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
using CRMPhone.ViewModel;

namespace CRMPhone
{
    /// <summary>
    /// Interaction logic for AddRatingDialog.xaml
    /// </summary>
    public partial class AudioSettingsDialog : Window
    {
        private AudioSettingsDialogViewModel _context;

        public AudioSettingsDialog(AudioSettingsDialogViewModel context)
        {
            _context = context;
            DataContext = _context;
            _context.SetView(this);
            InitializeComponent();
        }
    }
}
