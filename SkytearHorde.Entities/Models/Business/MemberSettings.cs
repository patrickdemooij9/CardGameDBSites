namespace SkytearHorde.Entities.Models.Business
{
    public class MemberSettings
    {
        public int MemberId { get; set; }
        public int? MissingCopiesThreshold { get; set; }

        public MemberSettings(int memberId)
        {
            MemberId = memberId;
        }
    }
}
