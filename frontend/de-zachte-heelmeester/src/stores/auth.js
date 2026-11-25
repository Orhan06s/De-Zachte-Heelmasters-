import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api.js'
import { errorMessage } from '@/utils/errorMessage.js'

export const useAuthStore = defineStore('auth', () => {
  const loading = ref(false)
  const error = ref(null)
  const errors = ref([])

  async function register(bsn, email, password) {
    loading.value = true
    error.value = null
    errors.value = []
    try {
      await api.post('/api/registration', { bsn, email, password })
      return true
    } catch (e) {
      error.value = errorMessage(e)
      errors.value = e.response?.data?.errors ?? []
      return false
    } finally {
      loading.value = false
    }
  }

  return { loading, error, errors, register }
})