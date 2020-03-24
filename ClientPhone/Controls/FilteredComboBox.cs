using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace CRMPhone.Controls
{
    class FilteredComboBox : System.Windows.Controls.ComboBox
    {
        #region Свойства
        private IEnumerable originalData;
        private ICollectionView filteredData;
        private bool itemsSourceChangedByControl;
        private bool filterTextChangedByControl;
        private TextBox textBox;
        private Popup popup;
        private ScrollViewer scrollViewer;
        #endregion

        #region Конструктор
        public FilteredComboBox()
        {
            this.filteredData = CollectionViewSource.GetDefaultView(this.originalData);
            this.IsEditable = true;
        }
        #endregion

        #region Методы
        private void SetFilter()
        {
            if (this.originalData == null || string.IsNullOrWhiteSpace(this.FilterPath))
                return;

            if (this.originalData.AsQueryable().ElementType.GetProperty(this.FilterPath) == null)
                throw new Exception(string.Format("FilteredComboBox.CollectionFilter: свойство {0} не найдено в объекте {1}", this.FilterPath, this.originalData.AsQueryable().ElementType));
            if(this.filteredData != null)
                this.filteredData.Filter = this.CollectionFilter;
        }

        ///<summary>
        /// Фильтр коллекции
        ///</summary>
        ///<param name="item">Прверяемый объект</param>
        private bool CollectionFilter(object item)
        {
            if (this.FilterIgnoreCase)
                return item.GetType().GetProperty(this.FilterPath).GetValue(item, null).ToString().ToLower().Contains(this.FilterText.ToLower());
            else
                return item.GetType().GetProperty(this.FilterPath).GetValue(item, null).ToString().Contains(this.FilterText);
        }
        #endregion

        #region Dependency properties
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register("FilterText", typeof(string), typeof(FilteredComboBox), new UIPropertyMetadata(string.Empty, FilterTextChanged));

        ///<summary>
        /// Значение фильтра
        ///</summary>
        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }

        public static readonly DependencyProperty FilterPathProperty =
            DependencyProperty.Register("FilterPath", typeof(string), typeof(FilteredComboBox), new UIPropertyMetadata(string.Empty, FilterPathChanged));

        ///<summary>
        /// Путь к фильтруемому полю
        ///</summary>
        public string FilterPath
        {
            get { return (string)GetValue(FilterPathProperty); }
            set { SetValue(FilterPathProperty, value); }
        }

        public static readonly DependencyProperty FilterIgnoreCaseProperty =
            DependencyProperty.Register("FilterIgnoreCase", typeof(bool), typeof(FilteredComboBox), new UIPropertyMetadata(true, FilterIgnoreCaseChanged));

        ///<summary>
        /// Игнорировать регистр
        ///</summary>
        public bool FilterIgnoreCase
        {
            get { return (bool)GetValue(FilterIgnoreCaseProperty); }
            set { SetValue(FilterIgnoreCaseProperty, value); }
        }
        #endregion

        #region События
        private static void FilterTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FilteredComboBox fcb = sender as FilteredComboBox;
            if (fcb.filteredData != null && fcb.filteredData.Filter == null)
            {
                fcb.filteredData.Filter = fcb.CollectionFilter;
            }
            fcb.OnFilterTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        private static void FilterPathChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FilteredComboBox fcb = sender as FilteredComboBox;
            fcb.OnFilterPathChanged((string)e.OldValue, (string)e.NewValue);
            fcb.OnDisplayMemberPathChanged((string)e.OldValue, (string)e.NewValue);

            fcb.SetFilter();
        }

        private static void FilterIgnoreCaseChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FilteredComboBox fcb = sender as FilteredComboBox;
            fcb.OnFilterIgnoreCaseChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected void OnFilterTextChanged(string oldValue, string newValue)
        {
            this.filteredData?.Refresh();

            if (!this.filterTextChangedByControl)
            {
                this.IsDropDownOpen = true;
            }
            else
                this.filterTextChangedByControl = false;
        }

        protected void OnFilterPathChanged(string oldValue, string newValue)
        {
            this.filteredData?.Refresh();
        }

        protected void OnFilterIgnoreCaseChanged(bool oldValue, bool newValue)
        {
            this.filteredData?.Refresh();
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
        }

        void PART_TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            //if (textBox.SelectionLength > 0)
            //{
            //    textBox.CaretIndex = textBox.Text.Length;
            //    textBox.SelectionLength = 0;
            //}

            e.Handled = true;
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.textBox = this.Template.FindName("PART_EditableTextBox", this) as TextBox;
            this.popup = this.Template.FindName("PART_Popup", this) as Popup;
            this.scrollViewer = this.Template.FindName("PART_ContentHost", this) as ScrollViewer;

            textBox.SelectionChanged += PART_TextBox_SelectionChanged;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (this.itemsSourceChangedByControl)
            {
                this.itemsSourceChangedByControl = false;
                return;
            }

            this.originalData = newValue;
            this.SetFilter();
            this.filteredData = CollectionViewSource.GetDefaultView(this.originalData);
            this.itemsSourceChangedByControl = true;
            this.ItemsSource = this.filteredData;
        }

        protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //base.OnSelectionChanged(e);

            this.filterTextChangedByControl = true;

            if (e.AddedItems.Count == 1)
            {
                if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    this.Text = this.SelectedItem.GetType().GetProperty(this.DisplayMemberPath).GetValue(this.SelectedItem, null).ToString();
                else if (!string.IsNullOrEmpty(this.FilterPath))
                    this.Text = this.SelectedItem.GetType().GetProperty(this.FilterPath).GetValue(this.SelectedItem, null).ToString();
                else
                    this.Text = this.SelectedItem as string;

                this.SelectedItem = e.AddedItems[0];
            }
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);

            if (this.SelectedItem == null)
                return;

            if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                this.Text = this.SelectedItem.GetType().GetProperty(this.DisplayMemberPath).GetValue(this.SelectedItem, null).ToString();
            else if (!string.IsNullOrEmpty(this.FilterPath))
                this.Text = this.SelectedItem.GetType().GetProperty(this.FilterPath).GetValue(this.SelectedItem, null).ToString();
            else
                this.Text = this.SelectedItem as string;
        }
        #endregion
    }
}
