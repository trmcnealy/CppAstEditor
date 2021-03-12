using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CppAstEditor
{
    public sealed class BindableCollection<T> : ObservableCollection<T>
    {
        public BindableCollection(): base(new List<T>())
        {
        }

        public BindableCollection(IEnumerable<T> collection)
            : base(collection)
        {
            foreach (T item in Items)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
                }
            }
        }

        public BindableCollection(T[] collection)
            : base(collection)
        {
            foreach (T item in Items)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
                }
            }
        }

        public BindableCollection(List<T> collection)
            : base(collection)
        {
            foreach (T item in Items)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
                }
            }
        }

        public new T this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                Items[index] = value;

                if (Items[index] is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
                }
            }
        }

        private void ItemsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }

        protected override void ClearItems()
        {
            foreach (T item in Items)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                }
            }

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];

            if (removedItem is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
            }

            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, T item)
        {
            if (item is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
            }

            base.InsertItem(index, item);
        }


        protected override void SetItem(int index, T item)
        {
            if (item is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
            }

            base.SetItem(index, item);
        }


        public BindableCollection<T> AddRange(IEnumerable<T> items)
        {
            int index = Count;

            foreach (T item in items)
            {
                if (item is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    notifyPropertyChanged.PropertyChanged += ItemsPropertyChanged;
                }

                InsertItem(index, item);

                index++;
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            return this;
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                int index = IndexOf(item);

                if (index >= 0)
                {
                    if (item is INotifyPropertyChanged notifyPropertyChanged)
                    {
                        notifyPropertyChanged.PropertyChanged -= ItemsPropertyChanged;
                    }

                    RemoveItem(index);
                }
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }


    }
}
