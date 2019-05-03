using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComboBoxControl
{
    class ComboModel :INotifyPropertyChanging
    {
        private ObservableCollection<string> _items;

        public ComboModel()
        {
            var list = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                list.Add($"Item №{i}");
            }
            Items = new ObservableCollection<string>(list);
        }
        public ObservableCollection<string> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public event PropertyChangingEventHandler PropertyChanging;
    }
}
