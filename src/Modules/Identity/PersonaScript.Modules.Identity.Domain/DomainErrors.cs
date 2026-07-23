using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Identity.Domain;

public static class DomainErrors
{
    public static class Identity
    {
        public static Error FullNameRequired =>
            Error.Validation("identity.full_name_required", "Informe seu nome completo.");

        public static Error EmailInvalid =>
            Error.Validation("identity.email_invalid", "Informe um e-mail válido.");

        public static Error PasswordTooShort =>
            Error.Validation("identity.password_too_short", "A senha deve ter no mínimo 8 caracteres.");

        public static Error PasswordHashRequired =>
            Error.Validation("identity.password_hash_required", "Hash de senha é obrigatório.");

        public static Error EmailAlreadyExists =>
            Error.Validation("identity.email_already_exists", "Este e-mail já está cadastrado.");

        public static Error TermsNotAccepted =>
            Error.Validation("identity.terms_not_accepted", "Aceite os Termos de Uso e a Política de Privacidade.");

        public static Error InvalidCredentials =>
            Error.Unauthorized("identity.invalid_credentials", "E-mail ou senha inválidos.");
    }
}
