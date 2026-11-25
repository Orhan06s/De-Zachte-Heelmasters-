<script setup>
defineProps({
  message: { type: String, default: '' },
  messages: { type: Array, default: () => [] },
})
</script>

<template>
  <div v-if="message || messages.length > 0" class="error-summary" role="alert">
    <p class="error-title">Let op:</p>
    <ul v-if="messages.length > 1">
      <li v-for="item in messages" :key="item">{{ item }}</li>
    </ul>
    <p v-else>{{ message }}</p>
  </div>
</template>

<style scoped>
.error-summary {
  padding: 1rem;
  border-left: 6px solid #b91c1c;
  border-radius: 14px;
  background: #fef2f2;
  color: #991b1b;
}

.error-title {
  margin: 0 0 0.25rem;
  font-weight: 700;
}

.error-summary ul,
.error-summary p {
  margin: 0;
}

.error-summary ul {
  padding-left: 1.25rem;
}
</style>