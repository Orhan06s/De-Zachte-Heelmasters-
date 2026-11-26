import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/services/api.js'
import { errorMessage } from '@/utils/errorMessage.js'

export const useAuthStore = defineStore('auth', () => {
  const loading = ref(false)
  const error = ref(null)
  const errors = ref([])
  const roles = ref([])
  const accessMessage = ref(null)

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

  async function login(email, password) {
    loading.value = true
    error.value = null
    errors.value = []
    try {
      const response = await api.post('/api/auth/login', { email, password })
      roles.value = response.data.roles
      return true
    } catch (e) {
      roles.value = []
      error.value = errorMessage(e)
      return false
    } finally {
      loading.value = false
    }
  }

  async function fetchCurrentUser() {
    try {
      const response = await api.get('/api/auth/me')
      roles.value = response.data.roles
    } catch {
      roles.value = []
    }
  }

  return { loading, error, errors, roles, accessMessage, register, login, fetchCurrentUser }
})