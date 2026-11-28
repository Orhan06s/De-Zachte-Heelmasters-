<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAppointmentStore } from '@/stores/appointments.js'
import ErrorSummary from '@/components/ErrorSummary.vue'
import { toIsoDate, addDays, canGoToPreviousWeek, groupByDay, formatDay, slotLabel } from '@/utils/appointmentSlots.js'

const store = useAppointmentStore()
const today = toIsoDate(new Date())

const referralId = ref(null)
const from = ref(today)
const selectedKey = ref(null)

const days = computed(() => groupByDay(store.slots))
const weekEnd = computed(() => addDays(from.value, 6))

// One moment is one specialist on one day at one time
function slotKey(slot) {
  return `${slot.specialistId}-${slot.date}-${slot.startTime}`
}

const selectedSlot = computed(() => store.slots.find((slot) => slotKey(slot) === selectedKey.value) ?? null)

onMounted(async () => {
  store.bookedSlot = null
  await store.fetchReferrals()

  // With one referral the patient does not have to choose
  if (store.referrals.length === 1) {
    await chooseReferral(store.referrals[0].referralId)
  }
})

async function chooseReferral(id) {
  referralId.value = id
  from.value = today
  await loadWeek()
}

async function loadWeek() {
  selectedKey.value = null
  await store.fetchSlots(referralId.value, from.value)
}

async function previousWeek() {
  from.value = addDays(from.value, -7)
  await loadWeek()
}

async function nextWeek() {
  from.value = addDays(from.value, 7)
  await loadWeek()
}

async function saveAppointment() {
  // When it works the confirmation is shown, when the moment is taken the free moments stay (AFS-02 AC3)
  await store.book(referralId.value, selectedSlot.value, from.value)
  selectedKey.value = null
}

// Loads the free moments again, the booked moment is no longer free
async function makeAnotherAppointment() {
  await loadWeek()
}
</script>

<template>
  <main class="appointment-page">
    <section class="appointment-card">
      <RouterLink :to="{ name: 'patient-start' }" class="back-link">Terug naar de startpagina</RouterLink>

      <h1>Afspraak maken</h1>

      <!-- After saving only the confirmation is shown -->
      <template v-if="store.bookedSlot">
        <div class="confirmation" role="status">
          <p class="confirmation-title">Gelukt, je afspraak staat vast!</p>
          <p>
            Je bent welkom op {{ formatDay(store.bookedSlot.date) }} van {{ store.bookedSlot.startTime }} tot
            {{ store.bookedSlot.endTime }} bij {{ store.bookedSlot.specialistName }}.
          </p>
        </div>
        <div class="actions">
          <button type="button" class="primary-button" @click="makeAnotherAppointment">Nog een afspraak maken</button>
        </div>
      </template>

      <template v-else>
        <ErrorSummary :message="store.error" />

        <!-- More than one referral: the patient chooses for which treatment -->
        <fieldset v-if="store.referrals.length > 1" class="referrals">
          <legend>Voor welke doorverwijzing?</legend>
          <label v-for="referral in store.referrals" :key="referral.referralId" class="option">
            <input
              type="radio"
              name="referral"
              :value="referral.referralId"
              :checked="referralId === referral.referralId"
              @change="chooseReferral(referral.referralId)"
            />
            {{ referral.careCode }} {{ referral.description }}
          </label>
        </fieldset>

        <template v-if="referralId">
          <p v-if="store.referrals.length === 1" class="referral">
            Doorverwijzing: {{ store.referrals[0].careCode }} {{ store.referrals[0].description }}
          </p>

          <div class="week-nav">
            <button type="button" class="secondary-button" :disabled="!canGoToPreviousWeek(from, today) || store.loading" @click="previousWeek">
              Vorige week
            </button>
            <span class="week">{{ formatDay(from) }} tot en met {{ formatDay(weekEnd) }}</span>
            <button type="button" class="secondary-button" :disabled="store.loading" @click="nextWeek">Volgende week</button>
          </div>

          <form class="slots" @submit.prevent="saveAppointment">
            <p v-if="store.loading">Bezig met laden...</p>
            <p v-else-if="days.length === 0">Er zijn deze week geen vrije momenten. Kijk bij de volgende week.</p>

            <fieldset v-for="day in days" :key="day.date" class="day">
              <legend>{{ formatDay(day.date) }}</legend>
              <label v-for="slot in day.slots" :key="slotKey(slot)" class="option">
                <input v-model="selectedKey" type="radio" name="slot" :value="slotKey(slot)" />
                {{ slotLabel(slot) }}
              </label>
            </fieldset>

            <button type="submit" class="primary-button" :disabled="!selectedSlot || store.loading">Afspraak opslaan</button>
          </form>
        </template>
      </template>
    </section>
  </main>
</template>

<style scoped>
.appointment-page {
  display: flex;
  justify-content: center;
  padding: 3rem 1rem;
}

.appointment-card {
  width: 100%;
  max-width: 720px;
  padding: 2rem;
  border-top: 6px solid #b91c1c;
  border-radius: 24px;
  background: #e5e5e5;
  color: #1f2937;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

h1 {
  margin: 0;
  font-size: 1.75rem;
  color: #b91c1c;
}

.back-link {
  align-self: flex-start;
  color: #1f2937;
  font-weight: 700;
}

.actions {
  display: flex;
  justify-content: flex-end;
}

.actions .primary-button {
  position: static;
}

.confirmation {
  padding: 1rem;
  border-left: 6px solid #15803d;
  border-radius: 14px;
  background: #f0fdf4;
  color: #14532d;
}

.confirmation p {
  margin: 0;
}

.confirmation .confirmation-title {
  margin-bottom: 0.25rem;
  font-size: 1.25rem;
  font-weight: 700;
}

.referral {
  margin: 0;
  font-weight: 700;
}

fieldset {
  margin: 0;
  padding: 0;
  border: none;
}

legend {
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.week-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.week {
  font-weight: 700;
  text-align: center;
}

.slots {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  max-height: 60vh;
  overflow-y: auto;
}

.day {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 0.25rem 1rem;
}

.day legend::first-letter {
  text-transform: uppercase;
}

.option {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-height: 32px;
  cursor: pointer;
}

input[type='radio'] {
  width: 1.25rem;
  height: 1.25rem;
  accent-color: #b91c1c;
}

input:focus-visible,
button:focus-visible,
a:focus-visible {
  outline: 3px solid #1d4ed8;
  outline-offset: 2px;
}

.primary-button,
.secondary-button {
  min-height: 48px;
  padding: 0.75rem 1.75rem;
  border-radius: 999px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
}

.primary-button {
  position: sticky;
  bottom: 0;
  align-self: flex-end;
  border: none;
  background: #b91c1c;
  color: #ffffff;
}

.primary-button:hover {
  background: #991b1b;
}

.secondary-button {
  border: 2px solid #b91c1c;
  background: #ffffff;
  color: #b91c1c;
}

.primary-button:disabled,
.secondary-button:disabled {
  border-color: #4b5563;
  background: #4b5563;
  color: #ffffff;
  cursor: not-allowed;
}

@media (max-width: 480px) {
  .appointment-card {
    padding: 1.25rem;
  }

  .primary-button {
    align-self: stretch;
  }
}
</style>
