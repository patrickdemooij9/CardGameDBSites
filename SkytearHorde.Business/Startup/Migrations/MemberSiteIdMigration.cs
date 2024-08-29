using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace SkytearHorde.Business.Startup.Migrations
{
    public class MemberSiteIdMigration : MigrationBase
    {
        private readonly IMemberService _memberService;

        public MemberSiteIdMigration(IMigrationContext context, IMemberService memberService) : base(context)
        {
            _memberService = memberService;
        }

        protected override void Migrate()
        {
            foreach(var member in _memberService.GetAll(0, int.MaxValue, out _))
            {
                member.SetValue("siteID", 1);
                member.Username = $"{member.Username}_1";
                _memberService.Save(member);
            }
        }
    }
}
