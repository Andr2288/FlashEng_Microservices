using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Bll.Dto
{
    public class UserSettingsDto
    {
        public int SettingsId { get; set; }
        public int UserId { get; set; }
        public string Theme { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool NotificationsEnabled { get; set; }
    }
}
