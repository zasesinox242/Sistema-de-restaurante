using System.Collections.Generic;

namespace AppFinal.Models
{
    public class JhiUserCreateDto
    {
        public string login { get; set; } = "";
        public string firstName { get; set; } = "";
        public string lastName { get; set; } = "";
        public string email { get; set; } = "";
        public bool activated { get; set; } = true;
        public string langKey { get; set; } = "es";
        public string password { get; set; } = "";
        public List<string> authorities { get; set; } = new();
    }
}