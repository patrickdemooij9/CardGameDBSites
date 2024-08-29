using SkytearHorde.Entities.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardTooltipViewModel
    {
        public Card Card { get; }
        public bool AlignLeft { get; set; }

        public CardTooltipViewModel(Card card)
        {
            Card = card;
        }
    }
}
