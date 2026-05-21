<script setup lang="ts">
import { Input, Dropdown, Menu, MenuItem } from 'ant-design-vue'
import type { HistoryItem } from '../types'

defineProps<{
  groupedHistory: { label: string; items: HistoryItem[] }[]
  currentTitle: string
  editingItemId?: string
  editingTitle: string
  currentAgentName?: string
}>()

const emit = defineEmits<{
  'history-click': [item: HistoryItem]
  'new-chat': []
  'agent-click': []
  'rename': [item: HistoryItem]
  'save-rename': [item: HistoryItem]
  'update:editingTitle': [value: string]
  'pin': [item: HistoryItem]
  'delete': [item: HistoryItem]
}>()
</script>

<template>
  <aside class="w-64 hidden lg:flex flex-col flex-shrink-0 border-r"
    style="background-color: var(--bg-sidebar); border-color: var(--border-color)">
    
    <div class="p-6">
      <button @click="emit('new-chat')"
        class="w-full flex items-center justify-center gap-2 px-4 py-3 rounded-xl transition-all border shadow-sm active:scale-[0.98] text-[13px] font-medium"
        style="background-color: var(--bg-secondary); color: var(--text-secondary); border-color: var(--border-color)">
        <span class="text-xl text-[var(--accent)]">+</span>
        <span>新建对话</span>
      </button>
    </div>

    <div class="flex-1 overflow-y-auto custom-scrollbar p-3">
      <div class="space-y-1">
        <template v-for="group in groupedHistory" :key="group.label">
          <div class="text-[10px] mt-3 mb-1 px-4 py-1 uppercase font-bold tracking-widest rounded-lg"
            style="background: var(--group-title-bg); color: var(--text-muted)">
            {{ group.label }}
          </div>
          <div v-for="item in group.items" :key="item.id"
            class="history-item group flex items-center justify-between px-4 py-3 text-[13px] cursor-pointer transition-all rounded-xl relative"
            :class="currentTitle === item.title ? 'history-item-active bg-[var(--accent)]/10 text-[var(--accent)]' : 'hover:bg-[var(--bg-hover)]'"
            :style="{ color: currentTitle === item.title ? 'var(--accent)' : 'var(--text-tertiary)' }"
            @click="emit('history-click', item)">
            
            <template v-if="editingItemId === item.id">
              <Input :value="editingTitle" @update:value="emit('update:editingTitle', $event)"
                :data-edit-id="item.id" size="small" class="flex-1 min-w-0"
                style="background: var(--bg-input); color: var(--text-primary); border-color: var(--accent)"
                @blur="emit('save-rename', item)"
                @keydown.enter.prevent="($event.target as HTMLInputElement).blur()"
                @click.stop />
            </template>
            <template v-else>
              <span class="flex items-center gap-1.5 truncate flex-1 min-w-0">
                <svg v-if="item.isTop" class="w-3.5 h-3.5 flex-shrink-0" style="color: var(--accent)" fill="currentColor" viewBox="0 0 16 16">
                  <path d="M4.5 0a.5.5 0 0 1 .5.5V2h5V.5a.5.5 0 0 1 1 0V2h1a2 2 0 0 1 2 2v2a2 2 0 0 1-2 2h-1v6.5a.5.5 0 0 1-1 0V8h-5v6.5a.5.5 0 0 1-1 0V8h-1a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h1V.5A.5.5 0 0 1 4.5 0z"/>
                </svg>
                {{ item.title }}
              </span>
            </template>

            <Dropdown :trigger="['click']">
              <button class="ml-2 p-1 rounded-md opacity-0 group-hover:opacity-100 transition-opacity hover:bg-[var(--bg-hover)] flex-shrink-0"
                style="color: var(--text-muted)" title="更多操作" @click.stop>
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 16 16">
                  <circle cx="8" cy="3" r="1.5" /><circle cx="8" cy="8" r="1.5" /><circle cx="8" cy="13" r="1.5" />
                </svg>
              </button>
              <template #overlay>
                <Menu>
                  <MenuItem @click="emit('rename', item)">重命名</MenuItem>
                  <MenuItem @click="emit('pin', item)">{{ item.isTop ? '取消置顶' : '置顶' }}</MenuItem>
                  <MenuItem @click="emit('delete', item)" danger>删除对话</MenuItem>
                </Menu>
              </template>
            </Dropdown>
          </div>
        </template>
      </div>
    </div>

    <div class="p-4 border-t" style="border-color: var(--border-color)">
      <button @click="emit('agent-click')"
        class="w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all text-[13px]"
        style="color: var(--text-tertiary)">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 0 0 2.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 0 0 1.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 0 0-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 0 0-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 0 0-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 0 0-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 0 0 1.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
        </svg>
        <span>{{ currentAgentName || '设置智能体' }}</span>
      </button>
    </div>
  </aside>
</template>

<style scoped>
.history-item {
  border-left: 2px solid transparent;
  transition: transform 0.2s, box-shadow 0.2s, background 0.2s;
}
.history-item:hover {
  transform: translateX(4px);
  box-shadow: 0 2px 8px var(--accent-shadow);
}
.history-item-active {
  border-left-color: var(--accent);
  box-shadow: 0 0 0 1px var(--accent-shadow);
}
.group-hover\:opacity-100 {
  opacity: 1;
}
</style>
