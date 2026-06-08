using System;

namespace PRN232.LMS.Repositories.Entities;

public partial class RefreshToken
{
    public int TokenId { get; set; }

    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
