using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private List<Card> _cards;

        protected List<Card> Cards
        {
            get
            {
                if (_cards == null)
                {
                    _cards = Game.GetActualCards();
                }
                return _cards;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SetStats.ItemsSource = SetCardsManager.Instance.SetCards.Select(gr => new SetDetailInfoViewModel
            {
                SetName = gr.Key,
                SetCards = new TrulyObservableCollection<CardInCollection>(gr.Value)
            });

            ListViewDB.ItemsSource = Cards;
        }

        private void ListViewDB_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ListViewDB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (Card)ListViewDB.SelectedItem;
                if (card == null)
                    return;
            }
        }

        private void SetDecrease(SetDetailInfoViewModel setInfo)
        {
            var card = setInfo.SetCards.First();
            card.Amount -= 1;
        }
    }
}
