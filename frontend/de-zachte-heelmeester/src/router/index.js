import { createRouter, createWebHistory } from 'vue-router'
import RegisterView from '@/views/RegisterView.vue'
import LoginView from '@/views/LoginView.vue'
import StartView from '@/views/StartView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/inloggen' },
    { path: '/registreren', name: 'register', component: RegisterView },
    { path: '/inloggen', name: 'login', component: LoginView },
    { path: '/patient', name: 'patient-start', component: StartView, meta: { title: 'patiënt'} },
    { path: '/huisarts', name: 'general-practitioner-start', component: StartView, meta: { title: 'huisarts'} },
    { path: '/specialist', name: 'specialist-start', component: StartView, meta: { title: 'specialist'} },
    { path: '/medewerker', name: 'employee-start', component: StartView, meta: { title: 'medewerker'} },
    { path: '/beheerder', name: 'admin-start', component: StartView, meta: { title: 'beheerder'} },
  ],
})

export default router