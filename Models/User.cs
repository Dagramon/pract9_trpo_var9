using System;
using System.Collections.Generic;

namespace pract8_trpo.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
