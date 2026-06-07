
// SEARCH INDEX
// MODEL, USER, LOGIN, REGISTER, ADMIN, PASSWORD, ROLE
//
// Topic: USER MODEL
// Purpose: Represents one row from the users table in C#.
// Search keywords: MODEL USER LOGIN REGISTER ADMIN PASSWORD ROLE
// When to use it: Show this when explaining what user data moves between DB, auth, pages, and API.
// Important notes: PasswordHash must stay server-side and should not be exposed to clients.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    // SECTION: USER DATA SHAPE
    // Topic: User data model
    // Purpose: Holds account identity, role, password hash, and creation date.
    // Search keywords: MODEL USER ROLE PASSWORD
    // When to use it: Use when tracing UserDB rows into AuthService or admin pages.
    // Important notes: Mobile uses MobileUserDto instead of returning this full model.
    public class User
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "customer";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
