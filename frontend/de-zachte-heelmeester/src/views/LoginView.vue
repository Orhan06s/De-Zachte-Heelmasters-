<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import ClearButton from '@/components/ClearButton.vue'
import PasswordToggle from '@/components/PasswordToggle.vue'
import ErrorSummary from '@/components/ErrorSummary.vue'

const auth = useAuthStore()
const email = ref('')
const password = ref('')
const showPassword = ref(false)

// Remove a message that another page left in the store
auth.error = null

async function handleLogin() {
    const success = await auth.login(email.value, password.value)

    if (!success) {
        // The email address stays filled in, only the password is cleared
        password.value = ''
        document.getElementById('password').focus()
    }
}
</script>

<template>
    <main class="login-page">
        <section class="login-card">
            <h1>Inloggen</h1>

            <form class="login-form" @submit.prevent="handleLogin">
                <div class="field">
                    <label for="email">E-mailadres</label>
                    <div class="input-wrapper">
                        <input id="email" v-model="email" type="email" autocomplete="username" required />
                        <ClearButton v-if="email" class="field-button" label="E-mailadres" @clear="email = ''" />
                    </div>
                </div>

                <div class="field">
                    <label for="password">Wachtwoord</label>
                    <div class="input-wrapper">
                        <input id="password" v-model="password" :type="showPassword ? 'text' : 'password'" autocomplete="current-password" required />
                        <PasswordToggle v-model="showPassword" class="field-button" />
                    </div>
                </div>

                <ErrorSummary :message="auth.error" />

                <button type="submit" class="primary-button" :disabled="auth.loading || !email || !password">
                    {{ auth.loading ? 'Bezig met inloggen...' : 'Inloggen' }}
                </button>

                <p class="hint">Nog geen account? <RouterLink to="/registreren">Registreren</RouterLink></p>
            </form>
        </section>
    </main>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  padding: 3rem 1rem;
}

.login-card {
  width: 100%;
  max-width: 560px;
  padding: 2rem;
  border-top: 6px solid #b91c1c;
  border-radius: 24px;
  background: #e5e5e5;
  color: #1f2937;
}

h1 {
  margin: 0 0 1.5rem;
  font-size: 1.75rem;
  color: #b91c1c;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field-button {
  position: absolute;
  top: 50%;
  right: 0.5rem;
  transform: translateY(-50%);
}

label {
  font-weight: 700;
}

input {
  box-sizing: border-box;
  min-height: 48px;
  padding: 0.75rem 1rem;
  border: 2px solid #767676;
  border-radius: 14px;
  background: #ffffff;
  color: #1f2937;
  font-size: 1rem;
}

input[aria-invalid='true'] {
  border-color: #b91c1c;
}

input:focus-visible,
.primary-button:focus-visible {
  outline: 3px solid #1d4ed8;
  outline-offset: 2px;
}

.input-wrapper {
  position: relative;
}

.input-wrapper input {
  width: 100%;
  padding-right: 3rem;
}

.hint {
  margin: 0;
  font-size: 0.875rem;
  color: #4b5563;
}

.primary-button {
  align-self: flex-end;
  min-height: 48px;
  padding: 0.75rem 1.75rem;
  border: none;
  border-radius: 999px;
  background: #b91c1c;
  color: #ffffff;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
}

.primary-button:hover {
  background: #991b1b;
}

.primary-button:disabled {
  background: #4b5563;
  cursor: not-allowed;
}

.field-error {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 700;
  color: #b91c1c;
}

@media (max-width: 480px) {
  .login-card {
    padding: 1.25rem;
  }

  .primary-button {
    align-self: stretch;
  }
}
</style>
