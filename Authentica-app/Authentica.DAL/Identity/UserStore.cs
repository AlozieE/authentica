using Authentica.DAL.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace Authentica.DAL.Identity
{
    public class UserStore :
        IUserStore<IdentityUser>,
        IUserPasswordStore<IdentityUser>,
        IUserEmailStore<IdentityUser>,
        IUserPhoneNumberStore<IdentityUser>,
        IUserLockoutStore<IdentityUser>,
        IUserSecurityStampStore<IdentityUser>,
        IUserTwoFactorStore<IdentityUser>,
        IUserAuthenticatorKeyStore<IdentityUser>
    {
        private readonly DatabaseConnection _db;

        public UserStore(DatabaseConnection db)
        {
            _db = db;
        }


        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.UserName);

        public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken ct)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken ct)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = """
                INSERT INTO AspNetUsers
                    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                     PasswordHash, SecurityStamp, ConcurrencyStamp,
                     PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
                     LockoutEnd, LockoutEnabled, AccessFailedCount)
                VALUES
                    (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                     @PasswordHash, @SecurityStamp, @ConcurrencyStamp,
                     @PhoneNumber, @PhoneNumberConfirmed, @TwoFactorEnabled,
                     @LockoutEnd, @LockoutEnabled, @AccessFailedCount)
                """;

            await using var cmd = new SqlCommand(sql, conn);
            BindUserParameters(cmd, user);
            await cmd.ExecuteNonQueryAsync(ct);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = """
                UPDATE AspNetUsers SET
                    UserName             = @UserName,
                    NormalizedUserName   = @NormalizedUserName,
                    Email                = @Email,
                    NormalizedEmail      = @NormalizedEmail,
                    EmailConfirmed       = @EmailConfirmed,
                    PasswordHash         = @PasswordHash,
                    SecurityStamp        = @SecurityStamp,
                    ConcurrencyStamp     = @ConcurrencyStamp,
                    PhoneNumber          = @PhoneNumber,
                    PhoneNumberConfirmed = @PhoneNumberConfirmed,
                    TwoFactorEnabled     = @TwoFactorEnabled,
                    LockoutEnd           = @LockoutEnd,
                    LockoutEnabled       = @LockoutEnabled,
                    AccessFailedCount    = @AccessFailedCount
                WHERE Id = @Id
                """;

            await using var cmd = new SqlCommand(sql, conn);
            BindUserParameters(cmd, user);
            await cmd.ExecuteNonQueryAsync(ct);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("DELETE FROM AspNetUsers WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", user.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            return IdentityResult.Success;
        }

        public async Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(SelectAll + " WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", userId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? MapUser(reader) : null;
        }

        public async Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(SelectAll + " WHERE NormalizedUserName = @NormalizedUserName", conn);
            cmd.Parameters.AddWithValue("@NormalizedUserName", normalizedUserName);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? MapUser(reader) : null;
        }

        // ── IUserPasswordStore ────────────────────────────────────────────────────

        public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, CancellationToken ct)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.PasswordHash);

        public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.PasswordHash != null);

        // ── IUserEmailStore ───────────────────────────────────────────────────────

        public Task SetEmailAsync(IdentityUser user, string? email, CancellationToken ct)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string?> GetEmailAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.EmailConfirmed);

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken ct)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<IdentityUser?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(SelectAll + " WHERE NormalizedEmail = @NormalizedEmail", conn);
            cmd.Parameters.AddWithValue("@NormalizedEmail", normalizedEmail);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? MapUser(reader) : null;
        }

        public Task<string?> GetNormalizedEmailAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.NormalizedEmail);

        public Task SetNormalizedEmailAsync(IdentityUser user, string? normalizedEmail, CancellationToken ct)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        // ── IUserPhoneNumberStore ─────────────────────────────────────────────────

        public Task SetPhoneNumberAsync(IdentityUser user, string? phoneNumber, CancellationToken ct)
        {
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string?> GetPhoneNumberAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.PhoneNumber);

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.PhoneNumberConfirmed);

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken ct)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        // ── IUserLockoutStore ─────────────────────────────────────────────────────

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.LockoutEnd);

        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken ct)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(++user.AccessFailedCount);

        public Task ResetAccessFailedCountAsync(IdentityUser user, CancellationToken ct)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.AccessFailedCount);

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.LockoutEnabled);

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled, CancellationToken ct)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        // ── IUserSecurityStampStore ───────────────────────────────────────────────

        public Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken ct)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string?> GetSecurityStampAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.SecurityStamp);

        // ── IUserTwoFactorStore ───────────────────────────────────────────────────

        public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken ct) =>
            Task.FromResult(user.TwoFactorEnabled);

        public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken ct)
        {
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }


  

        public async Task SetAuthenticatorKeyAsync(IdentityUser user, string key, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = """
                UPDATE AspNetUsers
                SET TwoFactorSecret = @Key
                WHERE Id = @Id
                """;

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Key", (object?)key ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id",  user.Id);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<string?> GetAuthenticatorKeyAsync(IdentityUser user, CancellationToken ct)
        {
            await using var conn = _db.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = "SELECT TwoFactorSecret FROM AspNetUsers WHERE Id = @Id";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", user.Id);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result is DBNull or null ? null : (string)result;
        }


        public void Dispose() { }

        private const string SelectAll = """
            SELECT Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                   PasswordHash, SecurityStamp, ConcurrencyStamp,
                   PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
                   LockoutEnd, LockoutEnabled, AccessFailedCount
            FROM AspNetUsers
            """;

        private static void BindUserParameters(SqlCommand cmd, IdentityUser user)
        {
            cmd.Parameters.AddWithValue("@Id",                   user.Id);
            cmd.Parameters.AddWithValue("@UserName",             (object?)user.UserName             ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedUserName",   (object?)user.NormalizedUserName   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email",                (object?)user.Email                ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedEmail",      (object?)user.NormalizedEmail      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EmailConfirmed",       user.EmailConfirmed);
            cmd.Parameters.AddWithValue("@PasswordHash",         (object?)user.PasswordHash         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SecurityStamp",        (object?)user.SecurityStamp        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConcurrencyStamp",     (object?)user.ConcurrencyStamp     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber",          (object?)user.PhoneNumber          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumberConfirmed", user.PhoneNumberConfirmed);
            cmd.Parameters.AddWithValue("@TwoFactorEnabled",     user.TwoFactorEnabled);
            cmd.Parameters.AddWithValue("@LockoutEnd",           (object?)user.LockoutEnd           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LockoutEnabled",       user.LockoutEnabled);
            cmd.Parameters.AddWithValue("@AccessFailedCount",    user.AccessFailedCount);
        }

        private static IdentityUser MapUser(SqlDataReader reader) => new()
        {
            Id                   = reader.GetString(reader.GetOrdinal("Id")),
            UserName             = NullableString(reader, "UserName"),
            NormalizedUserName   = NullableString(reader, "NormalizedUserName"),
            Email                = NullableString(reader, "Email"),
            NormalizedEmail      = NullableString(reader, "NormalizedEmail"),
            EmailConfirmed       = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
            PasswordHash         = NullableString(reader, "PasswordHash"),
            SecurityStamp        = NullableString(reader, "SecurityStamp"),
            ConcurrencyStamp     = NullableString(reader, "ConcurrencyStamp"),
            PhoneNumber          = NullableString(reader, "PhoneNumber"),
            PhoneNumberConfirmed = reader.GetBoolean(reader.GetOrdinal("PhoneNumberConfirmed")),
            TwoFactorEnabled     = reader.GetBoolean(reader.GetOrdinal("TwoFactorEnabled")),
            LockoutEnd           = NullableDateTimeOffset(reader, "LockoutEnd"),
            LockoutEnabled       = reader.GetBoolean(reader.GetOrdinal("LockoutEnabled")),
            AccessFailedCount    = reader.GetInt32(reader.GetOrdinal("AccessFailedCount"))
        };

        private static string? NullableString(SqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetString(ord);
        }

        private static DateTimeOffset? NullableDateTimeOffset(SqlDataReader r, string col)
        {
            var ord = r.GetOrdinal(col);
            return r.IsDBNull(ord) ? null : r.GetDateTimeOffset(ord);
        }
    }
}
