import { computed, reactive } from 'vue'

export function useRegisterValidation(bsn, email, password, confirmPassword) {
  const touched = reactive({ bsn: false, email: false, password: false, confirmPassword: false })

  const passwordsMatch = computed(() => password.value === confirmPassword.value)

  const bsnError = computed(() => {
    if (bsn.value === '') return 'Vul je BSN in.'
    if (!/^\d{9}$/.test(bsn.value)) return 'Een BSN bestaat uit 9 cijfers.'
    return ''
  })

  const emailError = computed(() => {
    if (email.value === '') return 'Vul je e-mailadres in.'
    if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email.value))
      return 'Vul een geldig e-mailadres in, zoals jan@example.nl.'
    return ''
  })

  const passwordError = computed(() => {
  if (password.value === '') return 'Vul een wachtwoord in.'
  if (passwordRules.value.some((rule) => !rule.met)) return 'Je wachtwoord voldoet nog niet aan alle eisen.'
  return ''
    })

  const passwordRules = computed(() => [
    { label: 'Minimaal 8 tekens', met: password.value.length >= 8 },
    { label: 'Een hoofdletter', met: /[A-Z]/.test(password.value) },
    { label: 'Een kleine letter', met: /[a-z]/.test(password.value) },
    { label: 'Een cijfer', met: /[0-9]/.test(password.value) },
    { label: 'Een speciaal teken, zoals ! of #', met: /[^A-Za-z0-9]/.test(password.value) },
    {
      label: 'Niet drie keer hetzelfde teken achter elkaar',
      met: password.value !== '' && !/(.)\1\1/.test(password.value),
    },
  ])

  const confirmError = computed(() => {
    if (confirmPassword.value === '') return 'Herhaal je wachtwoord.'
    if (!passwordsMatch.value) return 'De wachtwoorden zijn niet gelijk.'
    return ''
  })

  const isFormValid = computed(
    () => !bsnError.value && !emailError.value && !passwordError.value && !confirmError.value,
  )

  return { touched, bsnError, emailError, passwordError, confirmError, isFormValid, passwordRules }
}
