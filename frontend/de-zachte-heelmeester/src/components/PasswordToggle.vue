<script setup>
import eyeOffIcon from '../images/oogje_uit.png'
import eyeIcon from '../images/oogje_aan.png'

const visible = defineModel({ type: Boolean, default: false })
</script>

<template>
  <button type="button" class="password-toggle" :aria-label="visible ? 'Wachtwoord verbergen' : 'Wachtwoord tonen'" :aria-pressed="visible" @click="visible = !visible">
    <img :src="visible ? eyeOffIcon : eyeIcon" alt="" />
  </button>
</template>

<style scoped>
.password-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  min-height: 40px;
  padding: 0;
  border: none;
  background: none;
  cursor: pointer;
}

.password-toggle img {
  width: 40px;
  height: 40px;
}

.password-toggle:focus-visible {
  outline: 3px solid #1d4ed8;
  outline-offset: 2px;
}
</style>