using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

using KomeTubeR.ViewModel;

namespace KomeTubeR
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Private Member

        private MainWindowVM _mainVM;
        private ScrollViewer _svCommentDataGrid;
        private int _clickCount = 0;

        #endregion Private Member

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            _mainVM = new MainWindowVM();
            this.DataContext = _mainVM;

            DG_Comments.ItemsSource = _mainVM.CommentColle;
            _mainVM.CommentColle.CollectionChanged += On_CommentColle_CollectionChanged;
            _mainVM.CloseWindow = new Action(this.Close);

            _svCommentDataGrid = GetDescendantByType(DG_Comments, typeof(ScrollViewer)) as ScrollViewer;

            if (App.AppStartupParameter.IsParsed)
            {
                if (App.AppStartupParameter.IsHide)
                {
                    this.Visibility = Visibility.Hidden;
                }
                _mainVM.AutoStart();
            }
        }

        #endregion Constructor

        #region Private Method

        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }

        #endregion Private Method

        #region Event Handle

        private void On_Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _clickCount++;
            if (_clickCount % 5 == 0 && _clickCount < 15)
            {
                MessageBox.Show("裡面什麼都沒有喔", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (_clickCount == 15)
            {
                MessageBox.Show("中に誰もいませんよ～", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                _clickCount = 0;
            }
        }

        private void On_CommentColle_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_svCommentDataGrid.VerticalOffset + 20 > _svCommentDataGrid.ScrollableHeight)
            {
                _svCommentDataGrid.ScrollToBottom();
            }
        }

        #endregion Event Handle
    }
}