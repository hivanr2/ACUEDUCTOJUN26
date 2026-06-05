using Microsoft.AspNetCore.Identity;

namespace ACUEDUCTO2.Components.Account.Shared;

internal sealed class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = $"Las contraseñas deben tener al menos {length} caracteres." };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Las contraseñas deben contener al menos un carácter no alfanumérico." };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = "Las contraseñas deben contener al menos un dígito ('0'-'9')." };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = "Las contraseñas deben contener al menos una letra minúscula ('a'-'z')." };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = "Las contraseñas deben contener al menos una letra mayúscula ('A'-'Z')." };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = $"Las contraseñas deben usar al menos {uniqueChars} caracteres diferentes." };

    public override IdentityError DuplicateUserName(string userName)
        => new() { Code = nameof(DuplicateUserName), Description = $"El nombre de usuario '{userName}' ya está en uso." };

    public override IdentityError DuplicateEmail(string email)
        => new() { Code = nameof(DuplicateEmail), Description = $"El correo electrónico '{email}' ya está registrado." };

    public override IdentityError InvalidUserName(string userName)
        => new() { Code = nameof(InvalidUserName), Description = $"El nombre de usuario '{userName}' no es válido." };
}
