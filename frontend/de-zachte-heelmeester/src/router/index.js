import { createRouter, createWebHistory } from 'vue-router'
import RegisterView from '@/views/RegisterView.vue'
import LoginView from '@/views/LoginView.vue'
import StartView from '@/views/StartView.vue'
import MakeAppointmentView from '@/views/MakeAppointmentView.vue'
import { useAuthStore } from '@/stores/auth.js'
import { startPageFor, hasAccess } from '@/utils/startPage.js'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/inloggen' },
    { path: '/registreren', name: 'register', component: RegisterView },
    { path: '/inloggen', name: 'login', component: LoginView },
    { path: '/patient', name: 'patient-start', component: StartView, meta: { title: 'patiënt', roles: ['Patient'] } },
    { path: '/patient/afspraak-maken', name: 'make-appointment', component: MakeAppointmentView, meta: { roles: ['Patient'] } },
    { path: '/huisarts', name: 'general-practitioner-start', component: StartView, meta: { title: 'huisarts', roles: ['Huisarts'] } },
    { path: '/specialist', name: 'specialist-start', component: StartView, meta: { title: 'specialist', roles: ['Specialist'] } },
    { path: '/medewerker', name: 'employee-start', component: StartView, meta: { title: 'medewerker', roles: ['Medewerker'] } },
    { path: '/beheerder', name: 'admin-start', component: StartView, meta: { title: 'beheerder', roles: ['Beheerder'] } },
  ],
})

// Runs before every page change
router.beforeEach(async (to) => {
  const allowedRoles = to.meta.roles
  if (!allowedRoles) {
    return true // page for everyone
  }

  const auth = useAuthStore()
  if (auth.roles.length === 0) {
    await auth.fetchCurrentUser() // after a page refresh the store is empty
  }

  if (auth.roles.length === 0) {
    return { name: 'login' } // not logged in
  }

  if (!hasAccess(allowedRoles, auth.roles)) {
    // Message, stays logged in, back to the start page of his own role
    auth.accessMessage = 'Je hebt geen toegang tot deze pagina.'
    return { name: startPageFor(auth.roles) }
  }

  return true
})

export default router