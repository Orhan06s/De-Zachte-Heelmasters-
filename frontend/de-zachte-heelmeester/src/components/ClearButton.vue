<script setup>
defineProps({
  label: { type: String, required: true },
})

defineEmits(['clear'])
</script>

<template>
  <button type="button" class="clear-button" :aria-label="`${label} leegmaken`" @click="$emit('clear')">✕</button>
</template>

<style scoped>
.clear-button {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  min-height: 40px;
  padding: 0;
  border: none;
  background: none;
  color: #4b5563;
  font-size: 1.25rem;
  cursor: pointer;
}

.clear-button:focus-visible {
  outline: 3px solid #1d4ed8;
  outline-offset: 2px;
}
</style>