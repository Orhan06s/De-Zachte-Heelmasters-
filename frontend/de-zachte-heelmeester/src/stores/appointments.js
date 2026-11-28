import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api.js'
import { errorMessage } from '@/utils/errorMessage.js'

const REFERRAL_NEEDED = 'Je hebt eerst een doorverwijzing van je huisarts nodig.'

export const useAppointmentStore = defineStore('appointments', () => {
  const referrals = ref([])
  const slots = ref([])
  const loading = ref(false)
  const error = ref(null)
  const bookedSlot = ref(null)

  // The referrals of the logged-in patient (AFS-01 AC4: none means a message)
  async function fetchReferrals() {
    loading.value = true
    error.value = null
    try {
      const response = await api.get('/api/appointments/referrals')
      referrals.value = response.data
      if (referrals.value.length === 0) {
        error.value = REFERRAL_NEEDED
      }
    } catch (e) {
      referrals.value = []
      error.value = errorMessage(e)
    } finally {
      loading.value = false
    }
  }

  // The free moments for a referral, seven days from the given date (AFS-01 AC1)
  async function fetchSlots(referralId, from) {
    loading.value = true
    error.value = null
    bookedSlot.value = null
    try {
      const response = await api.get('/api/appointments/available', { params: { referralId, from } })
      slots.value = response.data
    } catch (e) {
      slots.value = []
      error.value = errorMessage(e)
    } finally {
      loading.value = false
    }
  }

  // Saves the chosen moment for the referral (AFS-01 AC2)
  async function book(referralId, slot, from) {
    loading.value = true
    error.value = null
    bookedSlot.value = null
    try {
      await api.post('/api/appointments', {
        referralId,
        specialistId: slot.specialistId,
        date: slot.date,
        startTime: slot.startTime,
      })
      bookedSlot.value = slot
      return true
    } catch (e) {
      const message = errorMessage(e)
      // AFS-02 AC3: someone else was first, back to the free moments without that moment
      if (e.response?.status === 409) {
        await fetchSlots(referralId, from)
      }
      error.value = message
      return false
    } finally {
      loading.value = false
    }
  }

  return { referrals, slots, loading, error, bookedSlot, fetchReferrals, fetchSlots, book }
})
