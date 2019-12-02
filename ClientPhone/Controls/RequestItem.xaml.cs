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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRMPhone.Controls
{
    /// <summary>
    /// Логика взаимодействия для RequestItem.xaml
    /// </summary>
    public partial class RequestItem : UserControl
    {
        public RequestItem()
        {
            InitializeComponent();
        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            var comboBoxControl = sender as ComboBox;
            CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(comboBoxControl.ItemsSource);
            //var displayPath = comboBoxControl.DisplayMemberPath;

            itemsViewOriginal.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(comboBoxControl.Text))
                    return true;
                return (o.ToString().ToUpper()).Contains(comboBoxControl.Text.ToUpper());
            });

            itemsViewOriginal.Refresh();
        }

        private void UIElement_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            var comboBoxControl = sender as ComboBox;
            CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(comboBoxControl.ItemsSource);
            //var displayPath = comboBoxControl.DisplayMemberPath;

            itemsViewOriginal.Filter = ((o) =>
            {
                if (String.IsNullOrEmpty(comboBoxControl.Text))
                    return true;
                return (o.ToString().ToUpper()).Contains(comboBoxControl.Text.ToUpper());
            });

            itemsViewOriginal.Refresh();
        }
    }
}
