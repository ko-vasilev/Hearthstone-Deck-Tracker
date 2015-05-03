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
using Hearthstone_Collection_Tracker.ViewModels;

namespace Hearthstone_Collection_Tracker.Controls
{
    /// <summary>
    /// Interaction logic for SetSummary.xaml
    /// </summary>
    public partial class SetSummary
    {
        public SetSummary()
        {
            InitializeComponent();
        }

        public event Action<SetDetailInfoViewModel> DecreaseClicked;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (DecreaseClicked != null)
                DecreaseClicked(button.DataContext as SetDetailInfoViewModel);
        }
    }
}
