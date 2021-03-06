#region

using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Replay;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class HsGameState : IHsGameState
	{
		private readonly GameV2 _game;

		public HsGameState(GameV2 game)
		{
			_game = game;
			KnownCardIds = new Dictionary<int, string>();
		}
		public bool CurrentEntityHasCardId { get; set; }
		public int CurrentEntityId { get; set; }
		public bool GameEnded { get; set; }
		public IGameHandler GameHandler { get; set; }
		public DateTime LastGameStart { get; set; }
		public int LastId { get; set; }
		public int MaxId { get; set; }
		public bool OpponentUsedHeroPower { get; set; }
		public bool PlayerUsedHeroPower { get; set; }
		public ReplayKeyPoint ProposedKeyPoint { get; set; }
		public bool FoundSpectatorStart { get; set; }
		public int JoustReveals { get; set; }
		public Dictionary<int, string> KnownCardIds { get; set; }
		public int LastCardPlayed { get; set; }
		public bool WasInProgress { get; set; }
		public bool SetupDone { get; set; }
		public TAG_ZONE CurrentEntityZone { get; set; }
		public bool DeterminedPlayers { get; set; }

		public void ProposeKeyPoint(KeyPointType type, int id, ActivePlayer player)
		{
			if(ProposedKeyPoint != null)
				ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player, _game);
			ProposedKeyPoint = new ReplayKeyPoint(null, type, id, player);
		}

		public int GetTurnNumber()
		{
			if(!_game.IsMulliganDone)
				return 0;
			return (_game.GameEntity?.GetTag(GAME_TAG.TURN) + 1) / 2 ?? 0;
		}

		public void GameEndKeyPoint(bool victory, int id)
		{
			if(ProposedKeyPoint != null)
			{
				ReplayMaker.Generate(ProposedKeyPoint.Type, ProposedKeyPoint.Id, ProposedKeyPoint.Player, _game);
				ProposedKeyPoint = null;
			}
			ReplayMaker.Generate(victory ? KeyPointType.Victory : KeyPointType.Defeat, id, ActivePlayer.Player, _game);
		}

		public void Reset()
		{
			GameEnded = false;
			JoustReveals = 0;
			KnownCardIds.Clear();
			LastGameStart = DateTime.Now;
			MaxId = 0;
			WasInProgress = false;
			SetupDone = false;
			DeterminedPlayers = false;
		}
	}
}