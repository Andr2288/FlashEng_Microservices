using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Domain.models
{
    public class UserSettings
    {
        public int SettingsId { get; set; }
        public int UserId { get; set; }
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en";
        public bool NotificationsEnabled { get; set; } = true;
    }
}
