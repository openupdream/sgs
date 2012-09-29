﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sanguosha.Core.UI;
using Sanguosha.Core.Skills;
using Sanguosha.Core.Players;
using Sanguosha.Core.Games;
using Sanguosha.Core.Triggers;
using Sanguosha.Core.Exceptions;
using Sanguosha.Core.Cards;

namespace Sanguosha.Expansions.Basic.Cards
{
    public abstract class ShunChai : CardHandler
    {
        public class ShunChaiCardChoiceVerifier : ICardChoiceVerifier
        {
            public VerifierResult Verify(List<List<Card>> answer)
            {
                Trace.Assert(answer.Count == 1);
                if (answer[0].Count == 0)
                {
                    return VerifierResult.Partial;
                }
                return VerifierResult.Success;
            }
        }

        protected abstract string ResultDeckName {get;}


        protected abstract string ChoicePrompt {get;}

        protected abstract DeckPlace ShunChaiDest(Player source, Player dest);

        protected override void Process(Player source, Player dest)
        {
            IUiProxy ui = Game.CurrentGame.UiProxies[dest];
            ShunChaiCardChoiceVerifier v1 = new ShunChaiCardChoiceVerifier();
            List<DeckPlace> places = new List<DeckPlace>();
            places.Add(new DeckPlace(dest, DeckType.DelayedTools));
            places.Add(new DeckPlace(dest, DeckType.Equipment));
            places.Add(new DeckPlace(dest, DeckType.Hand));
            List<string> resultDeckPlace = new List<string>();
            resultDeckPlace.Add(ResultDeckName);
            List<int> resultDeckMax = new List<int>();
            resultDeckMax.Add(1);
            List<List<Card>> answer;
            if (!ui.AskForCardChoice(ChoicePrompt, places, resultDeckPlace, resultDeckMax, v1, out answer))
            {
                Trace.TraceInformation("Player {0} Invalid answer", dest);
                answer = new List<List<Card>>();
                answer.Add(new List<Card>());
                var collection = Game.CurrentGame.Decks[dest, DeckType.Hand].Concat
                                 (Game.CurrentGame.Decks[dest, DeckType.DelayedTools].Concat
                                 (Game.CurrentGame.Decks[dest, DeckType.Equipment]));
                answer[0].Add(collection.First());
            }
            Trace.Assert(answer.Count == 1 && answer[0].Count == 1);

            CardsMovement m;
            m.cards = new List<Card>(answer[0]);
            m.to = ShunChaiDest(source, dest);
            Game.CurrentGame.MoveCards(m);
        }

        protected abstract bool ShunChaiAdditionalCheck(Player source, Player dest);

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            Player player = targets[0];
            if (!ShunChaiAdditionalCheck(source, player))
            {
                return VerifierResult.Fail;
            }
            if (Game.CurrentGame.Decks[player, DeckType.Hand].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.DelayedTools].Count == 0 &&
                Game.CurrentGame.Decks[player, DeckType.Equipment].Count == 0)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }
    }
}