using AuthSystem.Areas.Identity.Data;

namespace AuthSystem.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PageName { get; set; }
        public DateTime VisitTime { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

}
