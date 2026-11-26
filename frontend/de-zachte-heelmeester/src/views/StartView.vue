<script setup>
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth.js';

const route = useRoute()
const auth = useAuthStore()

// Show the message once, then remove it from the store
const accessMessage = auth.accessMessage
auth.accessMessage = null
</script>

<template>
    <main class="start-page">
        <h1>Startpagina {{ route.meta.title }}</h1>
        <p v-if="accessMessage" class="access-message" role="alert">{{ accessMessage }}</p>
        <p>Je bent ingelogd.</p>
    </main>
</template>

<style scoped>
.start-page {
    padding: 3rem 1rem;
    max-width: 560px;
    margin: 0 auto;
}

h1 {
    color: #b91c1c;
}

.access-message {
    padding: 1rem;
    border-left: 6px solid #b91c1c;
    border-radius: 14px;
    background: #fef2f2;
    color: #991b1b
}
</style>