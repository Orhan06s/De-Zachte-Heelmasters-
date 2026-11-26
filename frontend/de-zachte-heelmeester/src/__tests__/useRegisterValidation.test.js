import { describe, it, expect } from 'vitest'
import { ref } from 'vue'
import { useRegisterValidation } from '../composables/useRegisterValidation.js'

describe('useRegisterValidation', () => {
  it('lege BSN geeft een melding', () => {
    const { bsnError } = useRegisterValidation(ref(''), ref(''), ref(''), ref(''))

    expect(bsnError.value).toBe('Vul je BSN in.')
  })

  it('te kort BSN geeft een melding', () => {
    const { bsnError } = useRegisterValidation(ref('12345'), ref(''), ref(''), ref(''))

    expect(bsnError.value).toBe('Een BSN bestaat uit 9 cijfers.')
  })

  it('ongeldig e-mailadres geeft een melding', () => {
    const { emailError } = useRegisterValidation(ref(''), ref('test'), ref(''), ref(''))
    expect(emailError.value).toBe('Vul een geldig e-mailadres in, zoals jan@example.nl.')
  })

  it('geldig e-mailadres geeft geen melding', () => {
    const { emailError } = useRegisterValidation(ref(''), ref('test1@test.nl'), ref(''), ref(''))
    expect(emailError.value).toBe('')
  })

  it('verschillende wachtwoorden geven een melding', () => {
    const { confirmError } = useRegisterValidation(
      ref(''),
      ref(''),
      ref('!Test2026'),
      ref('!Test2027'),
    )

    expect(confirmError.value).toBe('De wachtwoorden zijn niet gelijk.')
  })

  it('wachtwoord voldoet aan de eisen geeft geen melding', () => {
    const { passwordError } = useRegisterValidation(
      ref(''),
      ref(''),
      ref('!Test2026'),
      ref('!Test2026'),
    )

    expect(passwordError.value).toBe('')
  })

  it('wachtwoord voldoet niet aan de eisen hoofdletter geeft een melding', () => {
    const { passwordError, passwordRules } = useRegisterValidation(
      ref(''),
      ref(''),
      ref('!test2026'),
      ref('!Test2026'),
    )

    expect(passwordError.value).toBe('Je wachtwoord voldoet nog niet aan alle eisen.')
    expect(passwordRules.value.find((r) => r.label === 'Een hoofdletter').met).toBe(false)
  })

  it('wachtwoord voldoet niet aan de eisen niet drie keer hetzelfde teken geeft een melding', () => {
    const { passwordError, passwordRules } = useRegisterValidation(
      ref(''),
      ref(''),
      ref('!Teeest2026'),
      ref('!Teeest2026'),
    )

    expect(passwordError.value).toBe('Je wachtwoord voldoet nog niet aan alle eisen.')
    expect(
      passwordRules.value.find((r) => r.label === 'Niet drie keer hetzelfde teken achter elkaar')
        .met,
    ).toBe(false)
  })

  it('alles goed ingevuld maakt het formulier geldig', () => {
    const { isFormValid } = useRegisterValidation(
      ref('111222333'),
      ref('test1@test.nl'),
      ref('!Test2026'),
      ref('!Test2026'),
    )

    expect(isFormValid.value).toBe(true)
  })

  it('alles goed ingevuld behalve bsn maakt het formulier ongeldig', () => {
    const { isFormValid } = useRegisterValidation(
      ref('11122233'),
      ref('test1@test.nl'),
      ref('!Test2026'),
      ref('!Test2026'),
    )

    expect(isFormValid.value).toBe(false)
  })
})
