<script lang="ts" setup>
import { Card } from 'ant-design-vue';
import { providers } from '../data'

const props = defineProps<{
  modelValue?: number;
  disabled?: boolean;
}>();
const emit = defineEmits<{
  (e: 'update:modelValue', value: number): void;
}>();

function handleSelect(value: number) {
  emit('update:modelValue', value);
}
</script>

<template>
  <div class="provider-selector">
    <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4">
      <Card v-for="provider in providers" :key="provider.value" size="small" :bordered="true"
        class="provider-card cursor-pointer transition-all p-2 text-center" :class="{
          'provider-card-active': modelValue === provider.value,
          'provider-card-disabled': disabled,
        }" @click="!disabled && handleSelect(provider.value)">
        <div class="flex flex-col items-center gap-1">
          <!-- 图标区域：有 icon 显示图片，无则显示首字母色块 -->
          <div v-if="!provider.icon"
            class="w-10 h-10 flex items-center justify-center rounded-lg text-white text-base font-bold"
            :style="{ backgroundColor: provider.value === 1 ? '#0960bd' : provider.value === 2 ? '#10a37f' : '#ff6a00' }">
            {{ provider.label.charAt(0) }}
          </div>
          <img v-else :src="provider.icon" :alt="provider.label"
            class="w-10 h-10 rounded-lg object-contain bg-gray-100" />
          <!-- 厂商名称（允许两行） -->
          <span
            class="text-xs font-medium text-gray-700 dark:text-gray-200 line-clamp-2 break-words w-full leading-tight">
            {{ provider.label }}
          </span>
        </div>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.provider-card {
  border: 1px solid var(--border-color, #e5e7eb);
  border-radius: 12px;
}

.provider-card:hover {
  border-color: var(--accent, #0960bd);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.06);
}

.provider-card-active {
  border-color: var(--accent, #0960bd);
  background-color: rgba(9, 96, 189, 0.08);
  box-shadow: 0 0 0 1px var(--accent, #0960bd);
}

:root.dark .provider-card-active {
  background-color: rgba(9, 96, 189, 0.2);
}

/* 确保 line-clamp 生效 */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.provider-card-disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
