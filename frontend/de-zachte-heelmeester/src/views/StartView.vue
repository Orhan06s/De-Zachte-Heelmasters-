<script setup>
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
    <main class="start-page">
        <h1>Startpagina {{ route.meta.title }}</h1>
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
</style>