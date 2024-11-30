using System.Windows;
using System.Windows.Controls;

namespace MultiSelectPlayground
{
    /// <summary>
    /// Interaction logic for MultiSelectListBox.xaml
    /// </summary>
    public partial class MultiSelectListBox : ListBox
    {
        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IList<object>), typeof(MultiSelectListBox));

        public IList<object> SelectedItemsList
        {
            get { return (IList<object>)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public MultiSelectListBox()
        {
            // InitializeComponent();
            this.SelectionChanged += MultiSelectListBox_SelectionChanged;
        }

        void MultiSelectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItems is not IList<object>) { return; }
            if (SelectedItemsList is null) { SelectedItemsList = []; }
            foreach (object addedItem in e.AddedItems)
            {
                SelectedItemsList.Add(addedItem);
            }
            foreach (object removedItem in e.RemovedItems)
            {
                SelectedItemsList.Remove(removedItem);
            }
            SetValue(SelectedItemsListProperty, SelectedItemsList);
        }
    }
}
