using System;
using System.Collections.Specialized;
using System.Windows;

namespace RevitMcp.Plugin
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;

            // Auto-scroll log to bottom when new entries arrive
            ((INotifyCollectionChanged)_vm.LogEntries).CollectionChanged += (_, _) =>
            {
                try
                {
                    if (LogList.Items.Count > 0)
                        LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
                }
                catch
                {

                }
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            _vm.Dispose();
            base.OnClosed(e);
        }
    }
}
