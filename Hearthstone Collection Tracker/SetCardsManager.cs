using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Collection_Tracker
{
    internal class SetCardsManager
    {
        private readonly string[] CollectableSets = { "Classic", "Goblins vs Gnomes" };

        private string StorageFilePath
        {
            get { return Config.Instance.DataDir + "CardCollection.xml"; }
        }

        public Dictionary<string, List<CardInCollection>> SetCards { get; private set; }

        protected SetCardsManager()
        {
        }

        private static SetCardsManager _instance;

        public static SetCardsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SetCardsManager();
                    _instance.LoadSetsInfo();
                }
                return _instance;
            }
        }

        protected void LoadSetsInfo()
        {
            bool infoLoadedFromFile = false;
            if (File.Exists(StorageFilePath))
            {
                try
                {
                    var setInfos = XmlManager<Dictionary<string, List<CardInCollection>>>.Load(StorageFilePath);
                    if (setInfos != null)
                    {
                        SetCards = setInfos;
                        infoLoadedFromFile = true;
                    }
                }
                catch (Exception)
                {
                    throw new Exception("File with your collection information is corrupted.");
                }
            }
            if (!infoLoadedFromFile)
            {
                var cards = Game.GetActualCards();
                SetCards = CollectableSets.ToDictionary(set => set,
                    set => cards.Where(c => c.Set == set)
                                .SelectMany(c => new[] { new CardInCollection(c, false, 2), new CardInCollection(c, true, 2) })
                                .ToList());
            }
        }

    }
}
