// Helpers for the screen where a patient chooses a free moment (AFS-01)

// A date as 'YYYY-MM-DD', the format the API uses. Uses the local date, so not shifted by the time zone.
export function toIsoDate(date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// Reads 'YYYY-MM-DD' as a local date. Without a time JavaScript reads it as UTC, with a time as local time.
function fromIsoDate(isoDate) {
  return new Date(`${isoDate}T00:00`)
}

export function addDays(isoDate, days) {
  const date = fromIsoDate(isoDate)
  date.setDate(date.getDate() + days)
  return toIsoDate(date)
}

// The first week starts today. Going back further would only show moments in the past.
export function canGoToPreviousWeek(from, today) {
  return from > today
}

// Groups the free moments per day, days and times in order
export function groupByDay(slots) {
  const sorted = [...slots].sort(
    (a, b) => a.date.localeCompare(b.date) || a.startTime.localeCompare(b.startTime),
  )

  const days = []
  for (const slot of sorted) {
    const lastDay = days[days.length - 1]
    if (lastDay && lastDay.date === slot.date) {
      lastDay.slots.push(slot)
    } else {
      days.push({ date: slot.date, slots: [slot] })
    }
  }
  return days
}

// For example 'maandag 30 november'
export function formatDay(isoDate) {
  return fromIsoDate(isoDate).toLocaleDateString('nl-NL', { weekday: 'long', day: 'numeric', month: 'long' })
}

// For example '09:00 tot 09:45, Test Cardioloog'
export function slotLabel(slot) {
  return `${slot.startTime} tot ${slot.endTime}, ${slot.specialistName}`
}
