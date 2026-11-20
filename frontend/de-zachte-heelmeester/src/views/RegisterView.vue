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
  <section class="register-card">
    <h1>Registreren</h1>

    <p v-if="success" class="success-message" role="status">
      Je account is aangemaakt.
    </p>

    <form v-else class="register-form" @submit.prevent="handleRegister">
      <label>
        <span>BSN</span>
        <input v-model="bsn" inputmode="numeric" maxlength="9" required />
      </label>

      <label>
        <span>E-mail</span>
        <input v-model="email" type="email" autocomplete="username" required />
      </label>

      <label>
        <span>Wachtwoord</span>
        <input v-model="password" type="password" autocomplete="new-password" required />
      </label>

      <button type="submit" :disabled="auth.loading">
        {{ auth.loading ? 'Bezig met registreren...' : 'Registreren' }}
      </button>
    </form>

    <ul v-if="auth.errors.length > 1" class="error-message" role="alert">
      <li v-for="message in auth.errors" :key="message">{{ message }}</li>
    </ul>
    <p v-else-if="auth.error" class="error-message" role="alert">
      {{ auth.error }}
    </p>
  </section>
</template>