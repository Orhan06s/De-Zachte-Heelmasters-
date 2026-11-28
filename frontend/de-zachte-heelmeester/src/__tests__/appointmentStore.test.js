import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import api from '@/services/api.js'
import { useAppointmentStore } from '@/stores/appointments.js'

// The tests do not call the real API, they decide themselves what the API answers
vi.mock('@/services/api.js', () => ({
  default: { get: vi.fn(), post: vi.fn() },
}))

const slot = { specialistId: 1, specialistName: 'Test Cardioloog', date: '2026-11-30', startTime: '09:00', endTime: '09:45' }
const otherSlot = { ...slot, startTime: '09:15', endTime: '10:00' }

function apiError(status, message) {
  return Object.assign(new Error(message), { response: { status, data: { message } } })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.resetAllMocks()
})

// AFS-01 AC4: without a referral the patient sees a message and no moments
describe('fetchReferrals', () => {
  it('without a referral there is a message', async () => {
    api.get.mockResolvedValue({ data: [] })
    const store = useAppointmentStore()

    await store.fetchReferrals()

    expect(store.referrals).toEqual([])
    expect(store.error).toBe('Je hebt eerst een doorverwijzing van je huisarts nodig.')
  })

  it('with a referral there is no message', async () => {
    api.get.mockResolvedValue({ data: [{ referralId: 1, careCode: 'Z0001', description: 'Consult hart' }] })
    const store = useAppointmentStore()

    await store.fetchReferrals()

    expect(store.referrals).toHaveLength(1)
    expect(store.error).toBeNull()
  })
})

// AFS-01 AC1: the free moments of seven days from a date
describe('fetchSlots', () => {
  it('asks the moments for the referral and the date', async () => {
    api.get.mockResolvedValue({ data: [slot] })
    const store = useAppointmentStore()

    await store.fetchSlots(1, '2026-11-30')

    expect(api.get).toHaveBeenCalledWith('/api/appointments/available', { params: { referralId: 1, from: '2026-11-30' } })
    expect(store.slots).toEqual([slot])
  })

  // After a booking the patient can make another appointment, then the old confirmation is gone
  it('removes the confirmation of the previous booking', async () => {
    api.post.mockResolvedValue({ status: 201 })
    api.get.mockResolvedValue({ data: [otherSlot] })
    const store = useAppointmentStore()
    await store.book(1, slot, '2026-11-30')

    await store.fetchSlots(1, '2026-11-30')

    expect(store.bookedSlot).toBeNull()
  })
})

// AFS-01 AC2: confirming saves the appointment for the referral
describe('book', () => {
  it('sends the chosen moment with the referral', async () => {
    api.post.mockResolvedValue({ status: 201 })
    const store = useAppointmentStore()

    const booked = await store.book(1, slot, '2026-11-30')

    expect(booked).toBe(true)
    expect(api.post).toHaveBeenCalledWith('/api/appointments', {
      referralId: 1,
      specialistId: 1,
      date: '2026-11-30',
      startTime: '09:00',
    })
    expect(store.bookedSlot).toEqual(slot)
  })

  // AFS-02 AC3: the moment is taken, message and back to the free moments without that moment
  it('a taken moment gives the message and new free moments', async () => {
    api.post.mockRejectedValue(apiError(409, 'Dit moment is niet meer vrij.'))
    api.get.mockResolvedValue({ data: [otherSlot] })
    const store = useAppointmentStore()

    const booked = await store.book(1, slot, '2026-11-30')

    expect(booked).toBe(false)
    expect(store.error).toBe('Dit moment is niet meer vrij.')
    expect(store.slots).toEqual([otherSlot])
    expect(store.bookedSlot).toBeNull()
  })
})
