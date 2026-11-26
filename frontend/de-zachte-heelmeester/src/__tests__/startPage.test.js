import { describe, it, expect } from "vitest";
import { startPageFor, hasAccess } from "@/utils/startPage.js";

// LOG-02 AC1: After logging in the user goes to the start page of his role
describe('startPageFor', () => {
    it('patient goes to the patient start page', () => {
        expect(startPageFor(['Patient'])).toBe('patient-start')
    })

    it('huisarts goes to the general practitioner start page', () => {
        expect(startPageFor(['Huisarts'])).toBe('general-practitioner-start')
    })

    it('user without a role goes back to login', () => {
        expect(startPageFor([])).toBe('login')
    })

    it('unknown role goes back to login', () => {
        expect(startPageFor(['Unknown'])).toBe('login')
    })
})

// LOG-02 AC2: a patient has no access to a page that is only for huisartsen
describe('hasAccess', () => {
  it('patient has no access to a huisarts page', () => {
    expect(hasAccess(['Huisarts'], ['Patient'])).toBe(false)
  })

  it('huisarts has access to a huisarts page', () => {
    expect(hasAccess(['Huisarts'], ['Huisarts'])).toBe(true)
  })

  it('user without roles has no access', () => {
    expect(hasAccess(['Patient'], [])).toBe(false)
  })
})