<script setup lang="ts">
import type { ChatsApi } from '#/modules/ai/api/chats'

defineProps<{
  models: ChatsApi.ChatsModel[]
  selectedModelId: string
  selectedModelDisplay: string
  isOpen: boolean
}>()

const emit = defineEmits<{
  select: [id: string]
  toggle: []
}>()
</script>

<template>
  <div class="relative model-selector">
    <button @click.stop="emit('toggle')"
      class="flex items-center gap-2 border text-[10px] font-bold px-3 py-1.5 rounded-lg transition-colors"
      style="background-color: var(--bg-selector); border-color: var(--border-color); color: var(--text-tertiary)">
      <span>{{ selectedModelDisplay }}</span>
      <svg :class="['w-3 h-3 transition-transform', isOpen ? 'rotate-180' : '']" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path d="M19 9l-7 7-7-7" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />
      </svg>
    </button>

    <transition name="vben-pop">
      <div v-if="isOpen"
        class="absolute bottom-full mb-2 right-0 w-55 border rounded-xl shadow-2xl p-1.5 z-[100] custom-scrollbar"
        style="max-height: 260px; overflow-y: auto; background-color: var(--bg-dropdown); border-color: var(--border-color)">
        <div v-for="model in models" :key="model.id"
          @click="emit('select', model.id)"
          :class="['px-3 py-2 text-[11px] font-medium rounded-lg cursor-pointer transition-all mb-0.5',
            selectedModelId === model.id ? 'bg-[var(--accent)]/20 text-[var(--accent)]' : 'hover:bg-[var(--bg-hover)]']"
          :style="{ color: selectedModelId === model.id ? 'var(--accent)' : 'var(--text-secondary)' }">
          {{ model.keyName }}_{{ model.configName }}
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.model-selector {
  position: relative;
}
.vben-pop-enter-active,
.vben-pop-leave-active {
  transition: all 0.2s ease;
}
.vben-pop-enter-from,
.vben-pop-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>
