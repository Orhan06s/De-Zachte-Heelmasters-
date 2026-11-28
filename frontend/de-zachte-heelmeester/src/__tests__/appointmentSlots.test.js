import { describe, it, expect } from 'vitest'
import { toIsoDate, addDays, canGoToPreviousWeek, groupByDay, formatDay, slotLabel } from '@/utils/appointmentSlots.js'

describe('toIsoDate', () => {
  it('writes a date as year-month-day, the way the API expects it', () => {
    expect(toIsoDate(new Date(2026, 10, 30))).toBe('2026-11-30')
  })
})

// AFS-01 AC3: no limit on how far ahead, the patient can keep going to the next week
describe('addDays', () => {
  it('next week is seven days later', () => {
    expect(addDays('2026-11-30', 7)).toBe('2026-12-07')
  })

  it('goes into the next year', () => {
    expect(addDays('2026-12-28', 7)).toBe('2027-01-04')
  })

  it('six months ahead is possible', () => {
    expect(addDays('2026-11-30', 182)).toBe('2027-05-31')
  })
})

// AFS-01 AC1: only free moments, so no weeks in the past
describe('canGoToPreviousWeek', () => {
  it('not from the first week', () => {
    expect(canGoToPreviousWeek('2026-11-28', '2026-11-28')).toBe(false)
  })

  it('yes from a later week', () => {
    expect(canGoToPreviousWeek('2026-12-05', '2026-11-28')).toBe(true)
  })
})

// AFS-01 AC1: the free moments are shown per day
describe('groupByDay', () => {
  const slots = [
    { specialistId: 1, specialistName: 'A', date: '2026-12-01', startTime: '09:00', endTime: '09:45' },
    { specialistId: 1, specialistName: 'A', date: '2026-11-30', startTime: '10:00', endTime: '10:45' },
    { specialistId: 1, specialistName: 'A', date: '2026-11-30', startTime: '09:15', endTime: '10:00' },
  ]

  it('puts the days in order', () => {
    expect(groupByDay(slots).map((day) => day.date)).toEqual(['2026-11-30', '2026-12-01'])
  })

  it('puts the moments of a day in order of time', () => {
    expect(groupByDay(slots)[0].slots.map((slot) => slot.startTime)).toEqual(['09:15', '10:00'])
  })

  it('no moments gives no days', () => {
    expect(groupByDay([])).toEqual([])
  })
})

describe('formatDay', () => {
  it('shows the day in Dutch', () => {
    expect(formatDay('2026-11-30')).toBe('maandag 30 november')
  })
})

describe('slotLabel', () => {
  it('shows the time and the specialist', () => {
    const slot = { specialistId: 1, specialistName: 'Test Cardioloog', date: '2026-11-30', startTime: '09:00', endTime: '09:45' }
    expect(slotLabel(slot)).toBe('09:00 tot 09:45, Test Cardioloog')
  })
})
