using Examine;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Examine;

namespace SkytearHorde.Business.Startup.Indexes
{
    internal class CardIndexPopulator : IndexPopulator
    {
        private readonly CardService _cardService;
        private readonly CardIndexValueSetBuilder _cardIndexValueSetBuilder;
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;

        public CardIndexPopulator(CardService cardService, CardIndexValueSetBuilder cardIndexValueSetBuilder, ISiteService siteService, ISiteAccessor siteAccessor)
        {
            _cardService = cardService;
            _cardIndexValueSetBuilder = cardIndexValueSetBuilder;
            _siteService = siteService;
            _siteAccessor = siteAccessor;

            RegisterIndex("CardIndex");
        }

        protected override void PopulateIndexes(IReadOnlyList<IIndex> indexes)
        {
            foreach (IIndex index in indexes)
            {
                foreach (var siteId in _siteService.GetAllSites())
                {
                    _siteAccessor.SetSiteId(siteId);

                    var cards = _cardService.GetAll(true).Where(it => it.VariantId != 0).ToArray();
                    index.IndexItems(_cardIndexValueSetBuilder.GetValueSets(cards));
                }
            }
        }
    }
}
