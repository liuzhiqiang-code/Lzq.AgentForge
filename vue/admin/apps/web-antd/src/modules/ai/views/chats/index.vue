<script setup lang="ts">
import { ref, computed, nextTick, watch, onMounted, onUnmounted, toRaw } from 'vue'
import { useAppConfig } from '@vben/hooks'
import { useAccessStore } from '@vben/stores'
import { getChatsList, getChatsHistory, getModels, deleteChats, updateTitleChats, updateTopChats } from '#/modules/ai/api/chats'
import type { ChatsApi }  from '#/modules/ai/api/chats'
import { useVbenModal } from '@vben/common-ui'
import { message } from 'ant-design-vue'
import { preferences } from '@vben/preferences'
import AgentModal from './modules/agentModal.vue'
import VoiceInputButton from './modules/voice-input-button.vue'
import ChatSidebar from './modules/chatSidebar.vue'
import ChatMessage from './modules/chatMessage.vue'
import ModelSelector from './modules/modelSelector.vue'
import type { ExtendedMessage, HistoryItem } from './types'

// ========== 全局配置 ==========
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const accessStore = useAccessStore()

// ========== 模态框 ==========
const [Modal, modalApi] = useVbenModal({ connectedComponent: AgentModal })

// ========== 流式状态 ==========
const isStreaming = ref(false)
const abortController = ref<AbortController | null>(null)

// ========== 主题 ==========
const isDark = ref(true)
watch(() => preferences.theme.mode, (newMode) => {
  isDark.value = newMode === 'dark'
}, { immediate: true })

// ========== 对话状态 ==========
const currentChats = ref<ChatsApi.Chats>()
const currentAgent = ref<ChatsApi.ChatsAgent>()
const currentTitle = ref('新对话')
const messages = ref<ExtendedMessage[]>([])
const history = ref<HistoryItem[]>([])

// ========== 模型状态 ==========
const models = ref<ChatsApi.ChatsModel[]>([])
const selectedModelId = ref('')
const isModelOpen = ref(false)

// ========== 输入状态 ==========
const inputText = ref('')
const scrollContainer = ref<HTMLElement | null>(null)
const textareaRef = ref<HTMLTextAreaElement | null>(null)

// ========== 历史编辑状态 ==========
const editingItemId = ref<string>()
const editingTitle = ref('')

// ========== 计算属性 ==========
const selectedModelDisplay = computed(() => {
  const found = models.value.find(m => m.id === selectedModelId.value)
  return found ? `${found.keyName}_${found.configName}` : selectedModelId.value
})

const groupedHistory = computed(() => {
  const now = new Date()
  const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const yesterdayStart = new Date(todayStart.getTime() - 86400000)
  const sevenDaysAgo = new Date(todayStart.getTime() - 7 * 86400000)
  const thirtyDaysAgo = new Date(todayStart.getTime() - 30 * 86400000)

  const pinned = history.value.filter(h => !!h.isTop)
  const unpinned = history.value.filter(h => !h.isTop)

  const today: HistoryItem[] = []
  const yesterday: HistoryItem[] = []
  const last7Days: HistoryItem[] = []
  const last30Days: HistoryItem[] = []
  const older: HistoryItem[] = []

  for (const item of unpinned) {
    const time = item.modificationTime ? new Date(item.modificationTime) : null
    if (!time) { older.push(item); continue }
    if (time >= todayStart) today.push(item)
    else if (time >= yesterdayStart) yesterday.push(item)
    else if (time >= sevenDaysAgo) last7Days.push(item)
    else if (time >= thirtyDaysAgo) last30Days.push(item)
    else older.push(item)
  }

  return [
    { label: '置顶', items: pinned },
    { label: '今天', items: today },
    { label: '昨天', items: yesterday },
    { label: '7 天内', items: last7Days },
    { label: '30 天内', items: last30Days },
    { label: '更早', items: older },
  ].filter(g => g.items.length > 0)
})

// ========== 生命周期 ==========
onMounted(async () => {
  window.addEventListener('click', handleOutsideClick)
  await getHistory()
  await loadModels()
})

onUnmounted(() => window.removeEventListener('click', handleOutsideClick))

// ========== 监听器 ==========
watch(inputText, () => nextTick(adjustHeight))
watch(messages, () => {
  if (scrollContainer.value) {
    const { scrollTop, scrollHeight, clientHeight } = scrollContainer.value
    if (scrollHeight - scrollTop - clientHeight < 100) scrollToBottom()
  }
}, { deep: true })

// ========== UI 方法 ==========
const scrollToBottom = async () => {
  await nextTick()
  scrollContainer.value?.scrollTo({ top: scrollContainer.value.scrollHeight, behavior: 'smooth' })
}

const adjustHeight = () => {
  if (!textareaRef.value) return
  textareaRef.value.style.height = 'auto'
  textareaRef.value.style.height = `${Math.min(textareaRef.value.scrollHeight, 180)}px`
}

const handleOutsideClick = (e: MouseEvent) => {
  if (!(e.target as HTMLElement).closest('.model-selector')) isModelOpen.value = false
}

// ========== 数据加载 ==========
async function loadModels() {
  try {
    const data = await getModels()
    if (data?.length) {
      models.value = data
      selectedModelId.value = data[0]!.id
    }
  } catch { message.error('获取模型列表失败') }
}

const getHistory = async () => {
  const data = await getChatsList()
  history.value = data.map(a => ({
    id: a.id, aiAgentName: a.aiAgentName ?? '',
    title: a.title ?? '新对话', modificationTime: a.modificationTime, isTop: a.isTop,
  }))
}

// ========== 历史操作 ==========
const historyClickHandle = async (item: HistoryItem) => {
  currentChats.value = item
  currentTitle.value = item.title
  currentAgent.value = { name: item.aiAgentName }
  messages.value = await getChatsHistory(item.id ?? '') as ExtendedMessage[]
  await nextTick()
}

const newChatsHandle = () => {
  messages.value = []
  currentChats.value = { id: '0', title: '新对话', aiAgentName: '' }
  currentTitle.value = '新对话'
  currentAgent.value = { name: '' }
}

const renameHistory = (item: HistoryItem) => {
  editingItemId.value = item.id
  editingTitle.value = item.title
  nextTick(() => {
    document.querySelector<HTMLInputElement>(`input[data-edit-id="${item.id}"]`)?.focus()
  })
}

const saveRename = async (item: HistoryItem) => {
  const newTitle = editingTitle.value.trim()
  if (!newTitle || newTitle === item.title) { editingItemId.value = undefined; return }
  try {
    await updateTitleChats({ id: item.id!, title: newTitle }) // item.id 必传
    item.title = newTitle
    if (currentChats.value?.id === item.id) { 
      currentTitle.value = newTitle
      currentChats.value!.title = newTitle // ← 修复：非空断言
    }
  } catch { message.error('重命名失败') }
  editingItemId.value = undefined
}

const pinHistory = async (item: HistoryItem) => {
  if (item.id) { await updateTopChats(item.id); await getHistory() }
}

const deleteHistory = async (item: HistoryItem) => {
  if (item.id) { await deleteChats(item.id); await getHistory() }
}

// ========== 智能体 ==========
const agentClickHandle = async () => {
  if (messages.value.length > 0 && currentChats.value?.id !== '0') {
    message.error('当前对话已有上下文，无法切换智能体。请新建对话后重试。')
    return
  }
  currentAgent.value ? modalApi.setData(toRaw(currentAgent.value)).open() : modalApi.open()
}

const agentSuccessHandle = (data: ChatsApi.ChatsAgent) => { currentAgent.value = data }

// ========== 语音 ==========
const onVoiceText = (text: string) => { inputText.value = text }

// ========== 终止流式响应 ==========
const stopStreaming = () => {
  abortController.value?.abort()
  abortController.value = null
  isStreaming.value = false
  const aiMsg = messages.value[messages.value.length - 1]
  if (aiMsg && aiMsg.role === 'assistant' && !aiMsg.content && (!aiMsg.segments || aiMsg.segments.length === 0))
    messages.value.pop()
}

// ========== 流式发送 ==========
const sendMessage = async () => {
  const prompt = inputText.value.trim()
  if (!prompt) return

  // 检查是否正在流式响应
  if (isStreaming.value) {
    message.warning('请等待当前回答完成后再发送')
    return
  }
  // 加锁
  isStreaming.value = true

  const userMsgId = Date.now().toString()
  messages.value.push({ id: userMsgId, role: 'user', content: prompt })
  inputText.value = ''
  nextTick(() => { if (textareaRef.value) textareaRef.value.style.height = 'auto' })

  const aiMsgId = userMsgId + 1
  const aiMsg: ExtendedMessage = { id: aiMsgId, role: 'assistant', content: '', segments: [] }
  messages.value.push(aiMsg)

  abortController.value = new AbortController()
  const signal = abortController.value.signal

  try {
    const response = await fetch(`${apiURL}/ai/chats/completion`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${accessStore.accessToken}` },
      body: JSON.stringify({
        AIAgentName: currentAgent.value?.name ?? '',
        Prompt: prompt,
        AIModelConfigId: selectedModelId.value,
        ChatsId: currentChats.value?.id ?? '0',
      }),
      signal,
    })

    if (!response.body) return
    const reader = response.body.getReader()
    const decoder = new TextDecoder()
    let buffer = ''

    while (true) {
      const { value, done } = await reader.read()
      if (done) break
      buffer += decoder.decode(value, { stream: true })
      const lines = buffer.split('\n')
      buffer = lines.pop() || ''

      for (const line of lines) {
        const trimmedLine = line.trim()
        if (!trimmedLine) continue
        const colonIndex = trimmedLine.indexOf(':')
        if (colonIndex === -1) continue
        const type = trimmedLine.substring(0, colonIndex).trim()
        const jsonStr = trimmedLine.substring(colonIndex + 1).trim()
        let data: any
        try { data = JSON.parse(jsonStr) } catch { continue }

        const msg = messages.value.find(m => m.id === aiMsgId) as ExtendedMessage
        if (!msg) continue
        if (!msg.segments) msg.segments = []

        switch (type) {
          case 'thinking': {
            const content = data.v || ''
            if (!content) break
            const last = msg.segments[msg.segments.length - 1]
            if (last?.type === 'thinking') last.content += content
            else msg.segments.push({ type: 'thinking', content, collapsed: true })
            break
          }
          case 'message': {
            if (data.p === 'response/status') break
            const content = data.v || ''
            if (!content) break
            const last = msg.segments[msg.segments.length - 1]
            if (last?.type === 'message') last.content += content
            else msg.segments.push({ type: 'message', content })
            break
          }
          case 'tool_start': {
            msg.segments.push({ type: 'tool', callId: data.callId || '', toolName: data.toolName || 'unknown', arguments: data.toolArgs || '', status: 'running', collapsed: true })
            break
          }
          case 'tool_end': {
            const targetCallId = data.callId
            for (let i = msg.segments.length - 1; i >= 0; i--) {
              const seg = msg.segments[i]
              if (seg?.type === 'tool' && seg.callId === targetCallId && seg.status === 'running') {
                seg.result = data.toolResult || ''
                seg.status = 'done'
                break
              }
            }
            break
          }
          // ========== 新增图表事件处理 ==========
          case 'echarts_start': {
            let title = '图表';
            let chartType = '';
            // 尝试从 toolArgs 解析实际参数
            if ((data as any).toolArgs) {
              try {
                const argsObj = JSON.parse((data as any).toolArgs);
                const args = argsObj?.arguments;
                if (args) {
                  title = args.title || title;
                  chartType = args.chartType || '';
                }
              } catch {}
            } else if (data.v) {
              // 备用从 v 解析
              try {
                const vObj = JSON.parse(data.v);
                title = vObj?.arguments?.title || vObj?.title || title;
                chartType = vObj?.arguments?.chartType || '';
              } catch {}
            }
            msg.segments.push({
              type: 'echarts',
              status: 'loading',
              title,
              chartType,
              collapsed: false,
              callId: data.callId,
            });
            break;
          }
          case 'echarts_end': {
            const targetCallId = data.callId;
            let option: Record<string, unknown> = {};

            // 优先使用后端直接给的 chartOption 对象
            if (data.chartOption && typeof data.chartOption === 'object') {
              option = data.chartOption as Record<string, unknown>;
            } else {
              // 兼容旧格式：从 toolResult 或 v 中解析
              const rawOption = (data as any).toolResult || data.v;
              if (typeof rawOption === 'string') {
                try { option = JSON.parse(rawOption) as Record<string, unknown>; } catch {}
              } else if (typeof rawOption === 'object' && rawOption !== null) {
                option = rawOption as Record<string, unknown>;
              }
            }

            const segments = msg.segments ?? [];
            const loadingSegment = segments.find(
              seg => seg.type === 'echarts' && seg.status === 'loading' && seg.callId === targetCallId
            ) as any;

            if (loadingSegment) {
              loadingSegment.status = 'done';
              loadingSegment.chartOption = option;
              loadingSegment.title = (option.title as any)?.text || loadingSegment.title;
              loadingSegment.collapsed = loadingSegment.collapsed ?? false;
            } else {
              msg.segments = [...segments, {
                type: 'echarts' as const,
                status: 'done',
                chartOption: option,
                title: (option.title as any)?.text || '图表',
                collapsed: false,
                callId: targetCallId,
              }];
            }
            break;
          }
          case 'error': {
            if (data.v) msg.errorContent = (msg.errorContent || '') + data.v
            break
          }
          case 'aiChats': {
            if (data.content) {
              currentTitle.value = data.content
              if (currentChats.value?.id !== data.chatId)//id对不上，说明是新对话
                currentChats.value = { id: data.chatId, title: data.content }
              const histItem = history.value.find(h => h.id == data.chatId)
              if (histItem) histItem.title = data.content
              else history.value.unshift({ id: data.chatId, aiAgentName: data.aiAgentName, title: data.content, modificationTime: new Date().toISOString() })
            }
            break
          }
          case 'close': {
            isStreaming.value = false
            break
          }
        }
      }
    }
  } catch (err: any) {
    if (err?.name === 'AbortError') {
      return
    }
    const aiMsg = messages.value.find(m => m.id === aiMsgId)
    if (aiMsg) aiMsg.content = '抱歉，服务遇到了一点问题，请稍后再试。'
  }
  finally {
    abortController.value = null
    isStreaming.value = false
  }
}
</script>

<template>
  <div :class="['flex h-screen w-full overflow-hidden antialiased font-sans', isDark ? 'dark' : 'light']"
    class="bg-transparent text-[var(--text-primary)]">
    <ChatSidebar
      :grouped-history="groupedHistory"
      :current-title="currentTitle"
      :editing-item-id="editingItemId"
      :editing-title="editingTitle"
      :current-agent-name="currentAgent?.name"
      @history-click="historyClickHandle"
      @new-chat="newChatsHandle"
      @agent-click="agentClickHandle"
      @rename="renameHistory"
      @save-rename="saveRename"
      @update:editing-title="editingTitle = $event"
      @pin="pinHistory"
      @delete="deleteHistory"
    />

    <main class="flex-1 flex flex-col min-w-0 h-full overflow-hidden bg-[var(--bg-primary)]">
      <header class="h-14 flex-shrink-0 flex items-center justify-center px-8 border-b"
        style="border-color: var(--border-light)">
        <div class="flex items-center gap-3 px-5 py-1.5 rounded-full border shadow-xl"
          style="background: var(--header-bg); border-color: var(--border-light)">
          <div class="w-1.5 h-1.5 bg-[#52c41a] rounded-full animate-pulse shadow-[0_0_8px_#52c41a]"></div>
          <h1 class="text-[12px] font-medium tracking-wide max-w-[260px] truncate" style="color: var(--text-secondary)">{{ currentTitle }}</h1>
          <div class="h-3 w-[1px] mx-1" style="background-color: var(--text-muted)"></div>
          <span class="text-[9px] uppercase font-black" style="color: var(--text-muted)">Online</span>
        </div>
      </header>

      <section ref="scrollContainer" class="flex-1 overflow-y-auto custom-scrollbar scroll-smooth">
        <div class="max-w-4xl mx-auto py-8 px-6">
          <ChatMessage
            v-for="msg in messages"
            :key="msg.id"
            :msg="msg"
            :selected-model-display="selectedModelDisplay"
          />
        </div>
      </section>

      <footer class="flex-shrink-0 p-6" style="background: linear-gradient(to top, var(--bg-primary), var(--bg-primary), transparent)">
        <div class="max-w-4xl mx-auto">
          <div class="input-wrapper rounded-2xl border transition-all duration-300"
            style="background-color: var(--bg-input); border-color: var(--border-color);">
            <textarea ref="textareaRef" v-model="inputText"
              @keydown.enter.exact.prevent="sendMessage" placeholder="发送指令..."
              class="w-full bg-transparent border-none px-6 py-4 resize-none text-[14px] placeholder:text-[var(--text-muted)] custom-scrollbar-textarea focus:ring-1 focus:ring-[var(--accent)]"
              style="color: var(--text-primary); min-height: 100px; max-height: 160px; outline: none;" />

            <div class="flex items-center justify-between px-4 py-3 border-t"
              style="border-color: var(--border-light); background-color: var(--bg-input-footer)">
              <div class="text-[11px] font-medium" style="color: var(--text-tertiary)">可按 Enter 发送</div>
              <div class="flex items-center gap-3">
                <ModelSelector
                  :models="models"
                  :selected-model-id="selectedModelId"
                  :selected-model-display="selectedModelDisplay"
                  :is-open="isModelOpen"
                  @select="selectedModelId = $event; isModelOpen = false"
                  @toggle="isModelOpen = !isModelOpen"
                />
                <button v-if="isStreaming" @click="stopStreaming"
                  class="px-5 py-1.5 rounded-xl font-bold text-[12px] flex items-center gap-2 text-white"
                  style="background: #ff4d4f;">
                  <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><rect x="6" y="6" width="12" height="12" rx="2"/></svg>
                  <span>停止</span>
                </button>
                <button v-else @click="sendMessage" :disabled="!inputText.trim()"
                  :class="['px-5 py-1.5 rounded-xl transition-all font-bold text-[12px] flex items-center gap-2', inputText.trim() ? 'text-white shadow-lg' : 'cursor-not-allowed']"
                  :style="inputText.trim() ? { backgroundColor: 'var(--accent)', boxShadow: '0 10px 15px -3px var(--accent-shadow)' } : { backgroundColor: 'var(--bg-disabled)', color: 'var(--text-muted)' }">
                  <span>发送</span>
                  <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/></svg>
                </button>
                <VoiceInputButton @text-recognized="onVoiceText" :disabled="isStreaming" />
              </div>
            </div>
          </div>
          <div class="mt-3 text-center">
            <p class="text-[10px]" style="color: var(--text-muted)">AI 可能会产生错误，请核实重要信息。</p>
          </div>
        </div>
      </footer>
    </main>
    <Modal @success="agentSuccessHandle" />
  </div>
</template>

<style scoped>

.dark {
  --bg-primary: #0f0f0f;
  --bg-secondary: #1a1a1a;
  --bg-sidebar: #141414;
  --bg-hover: #252525;
  --bg-input: #2a2a2a;
  --bg-input-footer: rgba(255, 255, 255, 0.04);
  --bg-bubble-ai: #252525;
  --bg-avatar: #2a2a2a;
  --bg-selector: #1f1f1f;
  --bg-dropdown: #252525;
  --bg-disabled: #2a2a2a;
  --text-primary: #e5e7eb;
  --text-secondary: #d1d5db;
  --text-tertiary: #9ca3af;
  --text-muted: #6b7280;
  --text-bubble-ai: #e5e7eb;
  --border-color: #2e2e2e;
  --border-light: rgba(255, 255, 255, 0.06);
  --accent: #0960bd;
  --accent-shadow: rgba(9, 96, 189, 0.25);
  --header-bg: rgba(20, 20, 20, 0.8);
  --group-title-bg: rgba(255, 255, 255, 0.04);
  --error-bg: #2d1617;
  --error-border: #5c2527;
  --error-text: #e8a4a4;
  --error-icon: #ff6b6b;
  --user-msg-bg: #4a3a6e;   /* 深紫灰，柔和且与暗黑背景协调 */
  --user-msg-text: #f0eef7; 
  --user-msg-shadow: rgba(74, 58, 110, 0.3);
}

.light {
  --bg-primary: #f8f9fa;
  --bg-secondary: #ffffff;
  --bg-sidebar: #ffffff;
  --bg-hover: #f0f2f5;
  --bg-input: #ffffff;
  --bg-input-footer: #f9fafb;
  --bg-bubble-ai: #f3f4f6;
  --bg-avatar: #e5e7eb;
  --bg-selector: #ffffff;
  --bg-dropdown: #ffffff;
  --bg-disabled: #e5e7eb;
  --text-primary: #111827;
  --text-secondary: #374151;
  --text-tertiary: #6b7280;
  --text-muted: #9ca3af;
  --text-bubble-ai: #1f2937;
  --border-color: #e5e7eb;
  --border-light: #f3f4f6;
  --accent: #0960bd;
  --accent-shadow: rgba(9, 96, 189, 0.15);
  --header-bg: rgba(255, 255, 255, 0.8);
  --group-title-bg: rgba(0, 0, 0, 0.02);
  --error-bg: #fff2f0;
  --error-border: #ffccc7;
  --error-text: #cf1322;
  --error-icon: #ff4d4f;
  --user-msg-bg: #e5f0ff;
  --user-msg-text: #1e2a3a;
  --user-msg-shadow: rgba(0, 0, 0, 0.1);
}
</style>

<style scoped>
.h-screen { height: 90vh; }
.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: var(--border-color); border-radius: 10px; }
.custom-scrollbar-textarea::-webkit-scrollbar { display: none; }
.animate-fade-in { animation: fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}
.vben-pop-enter-active, .vben-pop-leave-active { transition: all 0.2s ease; }
.vben-pop-enter-from, .vben-pop-leave-to { opacity: 0; transform: translateY(8px); }
textarea { scrollbar-width: none; }
.input-wrapper { box-shadow: 0 0 0 0 transparent; }
.input-wrapper:focus-within { border-color: var(--accent); box-shadow: 0 0 0 3px var(--accent-shadow); }
</style>
