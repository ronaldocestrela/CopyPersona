document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-password-toggle]');
    if (!button) {
        return;
    }

    const inputId = button.getAttribute('data-password-toggle');
    if (!inputId) {
        return;
    }

    const input = document.getElementById(inputId);
    if (!input) {
        return;
    }

    const isPassword = input.getAttribute('type') === 'password';
    input.setAttribute('type', isPassword ? 'text' : 'password');
    button.textContent = isPassword ? 'visibility_off' : 'visibility';
    button.setAttribute('aria-label', isPassword ? 'Ocultar senha' : 'Mostrar senha');
});
