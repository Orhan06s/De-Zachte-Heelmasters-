<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth.js'

const auth = useAuthStore()
const bsn = ref('')
const email = ref('')
const password = ref('')
const success = ref(false)

async function handleRegister() {
  success.value = await auth.register(bsn.value, email.value, password.value)

  if (!success.value) {
    password.value = ''
  }
}
</script>

<template>
  <main class="register-page">
    <section class="register-card">
      <h1>Registreren</h1>

      <p v-if="success" class="success-message" role="status">Je account is aangemaakt.</p>

      <form v-else class="register-form" @submit.prevent="handleRegister">
        <label class="field">
          <span>BSN</span>
          <input v-model="bsn" inputmode="numeric" maxlength="9" aria-describedby="bsn-hint" :aria-invalid="!!auth.error" required />
          <span id="bsn-hint" class="hint">9 cijfers, zonder spaties of punten.</span>
        </label>

        <label class="field">
          <span>E-mail</span>
          <input v-model="email" type="email" autocomplete="username" :aria-invalid="!!auth.error" required />
        </label>

        <label class="field">
          <span>Wachtwoord</span>
          <input v-model="password" type="password" autocomplete="new-password" aria-describedby="password-hint" :aria-invalid="auth.errors.length > 0" required />
          <span id="password-hint" class="hint">Minimaal 8 tekens, met een hoofdletter, een kleine letter, een cijfer en een speciaal teken. Niet drie keer hetzelfde teken achter elkaar.</span>
        </label>

        <div v-if="auth.error" class="error-message" role="alert">
          <p class="error-title">Let op:</p>
          <ul v-if="auth.errors.length > 1">
            <li v-for="message in auth.errors" :key="message">{{ message }}</li>
          </ul>
          <p v-else>{{ auth.error }}</p>
        </div>

        <button type="submit" class="primary-button" :disabled="auth.loading">
          {{ auth.loading ? 'Bezig met registreren...' : 'Registreren' }}
        </button>
      </form>
    </section>
  </main>
</template>

<style scoped>
.register-page {
  display: flex;
  justify-content: center;
  padding: 3rem 1rem;
}

.register-card {
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

.register-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

label {
  font-weight: 700;
}

input {
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

.error-message {
  padding: 1rem;
  border-left: 6px solid #b91c1c;
  border-radius: 14px;
  background: #fef2f2;
  color: #991b1b;
}

.error-title {
  margin: 0 0 0.25rem;
  font-weight: 700;
}

.error-message ul,
.error-message p {
  margin: 0;
}

.error-message ul {
  padding-left: 1.25rem;
}

.success-message {
  margin: 0;
  padding: 1rem;
  border-left: 6px solid #166534;
  border-radius: 14px;
  background: #f0fdf4;
  color: #166534;
  font-weight: 700;
}

@media (max-width: 480px) {
  .register-card {
    padding: 1.25rem;
  }

  .primary-button {
    align-self: stretch;
  }
}
</style>