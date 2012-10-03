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
    public class JueDou : CardHandler
    {
        protected override void Process(Player source, Player dest)
        {
            Player current = dest;
            while (true)
            {
                IUiProxy ui = Game.CurrentGame.UiProxies[current];
                SingleCardUsageVerifier v1 = new SingleCardUsageVerifier((c) => { return c.Type is Sha; });
                ISkill skill;
                List<Player> p;
                List<Card> cards;
                if (!ui.AskForCardUsage("JueDou", v1, out skill, out cards, out p))
                {
                    Trace.TraceInformation("Player {0} Invalid answer", current);
                    break;
                }
                if (!HandleCardUseWithSkill(current, skill, cards))
                {
                    continue;
                }
                Trace.TraceInformation("Player {0} SHA, ", current.Id);
                if (current == dest)
                {
                    current = source;
                }
                else
                {
                    current = dest;
                }
            }
            Player won = current == dest ? source : dest;
            Game.CurrentGame.DoDamage(won, current, 1, DamageElement.Fire, Game.CurrentGame.Decks[DeckType.Compute]);
        }

        protected override VerifierResult Verify(Player source, ICard card, List<Player> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return VerifierResult.Partial;
            }
            if (targets.Count > 1)
            {
                return VerifierResult.Fail;
            }
            return VerifierResult.Success;
        }

        public override CardCategory Category
        {
            get { return CardCategory.ImmediateTool; }
        }
    }
}