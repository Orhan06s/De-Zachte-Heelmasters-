<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth.js'
import ClearButton from '../components/ClearButton.vue'
import PasswordToggle from '../components/PasswordToggle.vue'
import ErrorSummary from '../components/ErrorSummary.vue'
import { useRegisterValidation } from '@/composables/useRegisterValidation.js'

const auth = useAuthStore()
const bsn = ref('')
const email = ref('')
const password = ref('')
const showPassword = ref(false)
const confirmPassword = ref('')
const success = ref(false)



const fields = { bsn, email, password, confirmPassword }

const { touched, bsnError, emailError, passwordError, confirmError, isFormValid, passwordRules } =
  useRegisterValidation(bsn, email, password, confirmPassword)

function clearField(name, id) {
  fields[name].value = ''
  document.getElementById(id).focus()
}

async function handleRegister() {
  success.value = await auth.register(bsn.value, email.value, password.value)

  if (!success.value) {
    password.value = ''
    confirmPassword.value = ''
    touched.password = false
    touched.confirmPassword = false
  }
}

</script>

<template>
  <main class="register-page">
    <section class="register-card">
      <h1>Registreren</h1>

      <p v-if="success" class="success-message" role="status">Je account is aangemaakt.</p>

      <form v-else class="register-form" @submit.prevent="handleRegister">
        <div class="field">
            <label for="bsn">BSN</label>
            <div class="input-wrapper">
                <input id="bsn" v-model="bsn" inputmode="numeric" maxlength="9" aria-describedby="bsn-hint bsn-error" :aria-invalid="(touched.bsn && !!bsnError) || !!auth.error" required @blur="touched.bsn = true" />
                <ClearButton v-if="bsn" class="field-button" label="BSN" @clear="clearField('bsn', 'bsn')" />
            </div>
            <span id="bsn-hint" class="hint">9 cijfers, zonder spaties of punten.</span>
            <span v-if="touched.bsn && bsnError" id="bsn-error" class="field-error" role="alert">{{ bsnError }}</span>  
        </div>

       <div class="field">
          <label for="email">E-mailadres</label>
          <div class="input-wrapper">
            <input id="email" v-model="email" type="email" autocomplete="username" :aria-invalid="touched.email && !!emailError" required @blur="touched.email = true" aria-describedby="email-error"/>
            <ClearButton v-if="email" class="field-button" label="E-mailadres" @clear="clearField('email', 'email')" />
          </div>
          <span v-if="touched.email && emailError" id="email-error" class="field-error" role="alert">{{ emailError }}</span>
        </div>

        <div class="field">
          <label for="password">Wachtwoord</label>
          <div class="input-wrapper">
            <input id="password" v-model="password" class="has-two-buttons" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" aria-describedby="password-hint password-error" :aria-invalid="touched.password && !!passwordError" required @blur="touched.password = true" @copy.prevent @cut.prevent />
            <ClearButton v-if="password" class="field-button" label="Wachtwoord" @clear="clearField('password', 'password')" />
            <PasswordToggle v-model="showPassword" class="eye-button" />
          </div>
          <ul id="password-hint" class="password-rules">
            <li v-for="rule in passwordRules" :key="rule.label" :class="{ met: rule.met }">
            {{ rule.met ? '✓' : '✗' }} {{ rule.label }}
            </li>
            </ul>
          <span v-if="touched.password && passwordError" id="password-error" class="field-error" role="alert">{{ passwordError }}</span>
        </div>

        <div class="field">
          <label for="confirm-password">Herhaal wachtwoord</label>
          <div class="input-wrapper">
            <input id="confirm-password" v-model="confirmPassword" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" aria-describedby="confirm-password-error" :aria-invalid="(touched.confirmPassword || confirmPassword !== '') && !!confirmError" required @blur="touched.confirmPassword = true" @copy.prevent @cut.prevent />
            <ClearButton v-if="confirmPassword" class="field-button" label="Herhaal wachtwoord" @clear="clearField('confirmPassword', 'confirm-password')" />
          </div>
          <span v-if="(touched.confirmPassword || confirmPassword !== '') && confirmError" id="confirm-password-error" class="field-error" role="alert">{{ confirmError }}</span>
        </div>

        <ErrorSummary :message="auth.error" :messages="auth.errors" />

        <button type="submit" class="primary-button" :disabled="auth.loading || !isFormValid" aria-describedby="submit-hint">
            {{ auth.loading ? 'Bezig met registreren...' : 'Registreren' }}
        </button>
        <p v-if="!isFormValid" id="submit-hint" class="hint">Vul alle velden correct in om te registreren.</p>
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

.field-button {
  position: absolute;
  top: 50%;
  right: 0.5rem;
  transform: translateY(-50%);
}

.input-wrapper input.has-two-buttons {
  padding-right: 6rem;
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

.success-message {
  margin: 0;
  padding: 1rem;
  border-left: 6px solid #166534;
  border-radius: 14px;
  background: #f0fdf4;
  color: #166534;
  font-weight: 700;
}

.eye-button {
  position: absolute;
  top: 50%;
  right: 3.25rem;
  transform: translateY(-50%);
}

.password-rules {
  margin: 0;
  padding: 0;
  list-style: none;
  font-size: 0.875rem;
  color: #b91c1c;
}

.password-rules li.met {
  color: #00c64c;
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