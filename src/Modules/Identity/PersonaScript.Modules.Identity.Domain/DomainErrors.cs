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

        public static Error PasswordResetTokenInvalid =>
            Error.Validation("identity.password_reset_token_invalid", "Token de redefinição de senha inválido ou expirado.");

        public static Error PasswordResetTokenExpired =>
            Error.Validation("identity.password_reset_token_expired", "Token de redefinição de senha expirado.");

        public static Error UserNotFound =>
            Error.NotFound("identity.user_not_found", "Usuário não encontrado.");

        public static Error ProviderRequired =>
            Error.Validation("identity.provider_required", "Provedor de autenticação é obrigatório.");

        public static Error AccountFrozen =>
            Error.Unauthorized("identity.account_frozen", "Sua conta está temporariamente congelada pelo suporte.");

        public static Error FreezeReasonRequired =>
            Error.Validation("identity.freeze_reason_required", "O motivo do congelamento é obrigatório.");
    }
}
