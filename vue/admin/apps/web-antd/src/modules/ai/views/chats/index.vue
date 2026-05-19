<script setup lang="ts">
// ==================== 导入 ====================
import { ref, nextTick, watch, onMounted, onUnmounted, toRaw, computed } from 'vue'
import { useAppConfig } from '@vben/hooks'
import { useAccessStore } from '@vben/stores'
import MarkdownIt from 'markdown-it'
import type { ChatsApi } from '#/modules/ai/api/chats'
import { getChatsList, getChatsHistory, getModels, deleteChats, updateTitleChats, updateTopChats } from '#/modules/ai/api/chats'
import { useVbenModal } from '@vben/common-ui'
import AgentModal from './modules/agentModal.vue'
import VoiceInputButton from './modules/voice-input-button.vue'
import { Dropdown, Menu, MenuItem, Input, message } from 'ant-design-vue'
import { preferences } from '@vben/preferences'

// ==================== 扩展消息类型 ====================
interface ExtendedMessage extends ChatsApi.ChatsHistory {
  errorContent?: string
}

// ==================== 全局配置和工具 ====================
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const accessStore = useAccessStore()
const md = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true,
})

// ==================== 模态框 ====================
const [Modal, modalApi] = useVbenModal({
  connectedComponent: AgentModal,
})

// ==================== 主题状态与同步 ====================
const isDark = ref(true)

watch(
  () => preferences.theme.mode,
  (newMode) => {
    if (newMode) {
      if (newMode === 'dark') {
        isDark.value = true
      } else {
        isDark.value = false
      }
    }
  },
  { immediate: true }
)

// ==================== 对话和智能体状态 ====================
const currentChats = ref<ChatsApi.Chats>()
const currentAgent = ref<ChatsApi.ChatsAgent>()
const currentTitle = ref('新对话')

// ==================== 模型选择状态 ====================
const models = ref<ChatsApi.ChatsModel[]>([])
const selectedModelId = ref<string>('')
const selectedModelDisplay = computed(() => {
  const found = models.value.find(m => m.id === selectedModelId.value)
  return found ? `${found.keyName}_${found.configName}` : selectedModelId.value
})
const isModelOpen = ref(false)

async function loadModels() {
  try {
    const data = await getModels()
    if (data && data.length > 0) {
      models.value = data
      selectedModelId.value = data[0]!.id
    }
  } catch (e) {
    message.error('获取模型列表失败')
  }
}

// ==================== 消息和历史状态 ====================
const messages = ref<ExtendedMessage[]>([])

type historyItem = {
  id?: string
  aiAgentName: string
  title: string
  modificationTime?: string
  isTop?: boolean
}
const history = ref<historyItem[]>([])

// ==================== UI 引用 ====================
const scrollContainer = ref<HTMLElement | null>(null)
const textareaRef = ref<HTMLTextAreaElement | null>(null)

// ==================== 输入状态 ====================
const inputText = ref('')

// ==================== 方法：历史记录 ====================
const getHistory = async () => {
  const data = await getChatsList()
  history.value = data.map((a) => ({
    id: a.id,
    aiAgentName: a.aiAgentName ?? '',
    title: a.title ?? '新对话',
    modificationTime: a.modificationTime,
    isTop: a.isTop,
  }))
}

const groupedHistory = computed(() => {
  const now = new Date()
  const todayStart = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const yesterdayStart = new Date(todayStart.getTime() - 86400000)
  const sevenDaysAgo = new Date(todayStart.getTime() - 7 * 86400000)
  const thirtyDaysAgo = new Date(todayStart.getTime() - 30 * 86400000)
  const oneYearAgo = new Date(todayStart.getTime() - 365 * 86400000)

  const groups: [
    { label: string; items: historyItem[] },
    { label: string; items: historyItem[] },
    { label: string; items: historyItem[] },
    { label: string; items: historyItem[] },
    { label: string; items: historyItem[] },
    { label: string; items: historyItem[] }
  ] = [
    { label: '置顶', items: [] },
    { label: '今天', items: [] },
    { label: '昨天', items: [] },
    { label: '7 天内', items: [] },
    { label: '30 天内', items: [] },
    { label: '1 年内', items: [] },
  ]

  const pinned: historyItem[] = []
  const unpinned: historyItem[] = []

  for (const item of history.value) {
    if (item.isTop) pinned.push(item)
    else unpinned.push(item)
  }
  groups[0].items = pinned

  for (const item of unpinned) {
    const time = item.modificationTime ? new Date(item.modificationTime) : null
    if (!time) {
      groups[5].items.push(item)
      continue
    }
    if (time >= todayStart) {
      groups[1].items.push(item)
    } else if (time >= yesterdayStart) {
      groups[2].items.push(item)
    } else if (time >= sevenDaysAgo) {
      groups[3].items.push(item)
    } else if (time >= thirtyDaysAgo) {
      groups[4].items.push(item)
    } else {
      groups[5].items.push(item)
    }
  }

  return groups.filter(g => g.items.length > 0)
})

const historyClickHandle = async (item: historyItem) => {
  currentChats.value = item
  currentTitle.value = item.title
  currentAgent.value = { name: item.aiAgentName }
  messages.value = (await getChatsHistory(item.id ?? '')) as ExtendedMessage[]
}

// ==================== 方法：智能体 ====================
const agentClickHandle = async () => {
  if (messages.value.length > 0 && currentChats.value?.id !== '0') {
    message.error('当前对话已有上下文，无法切换智能体。请新建对话后重试。')
    return
  }
  if (currentAgent.value) modalApi.setData(toRaw(currentAgent.value)).open()
  else modalApi.open()
}

const agentSuccessHandle = async (data: ChatsApi.ChatsAgent) => {
  currentAgent.value = data
}

// ==================== 方法：新建对话 ====================
const newChatsHandle = async () => {
  messages.value = []
  currentChats.value = { id: '0', title: '新对话', aiAgentName: '' }
  currentTitle.value = '新对话'
  currentAgent.value = { name: '' }
}

// ==================== 方法：UI 调整 ====================
const adjustHeight = () => {
  const textarea = textareaRef.value
  if (!textarea) return
  textarea.style.height = 'auto'
  textarea.style.height = `${Math.min(textarea.scrollHeight, 180)}px`
}

const scrollToBottom = async () => {
  await nextTick()
  if (scrollContainer.value) {
    scrollContainer.value.scrollTo({
      top: scrollContainer.value.scrollHeight,
      behavior: 'smooth',
    })
  }
}

// ==================== 监听器 ====================
watch(inputText, () => {
  nextTick(() => adjustHeight())
})

watch(messages, () => scrollToBottom(), { deep: true })

// ==================== 方法：发送消息与流处理 ====================
const sendMessage = async () => {
  const prompt = inputText.value.trim()
  if (!prompt) return

  const userMsgId = Date.now().toString()
  messages.value.push({ id: userMsgId, role: 'user', content: prompt } as ExtendedMessage)
  inputText.value = ''

  nextTick(() => {
    if (textareaRef.value) textareaRef.value.style.height = 'auto'
  })

  const aiMsgId = userMsgId + 1
  messages.value.push({ id: aiMsgId, role: 'assistant', content: '' } as ExtendedMessage)

  try {
    const response = await fetch(`${apiURL}/ai/chats/completion`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessStore.accessToken as string}`,
      },
      body: JSON.stringify({
        AIAgentName: currentAgent.value?.name ?? '',
        Prompt: prompt,
        AIModelConfigId: selectedModelId.value,
        ChatsId: currentChats.value?.id ?? '0',
      }),
    })

    if (!response.body) return

    const reader = response.body.getReader()
    const decoder = new TextDecoder()

    while (true) {
      const { value, done } = await reader.read()
      if (done) break

      const chunk = decoder.decode(value, { stream: true })
      const lines = chunk.split('\n\n')

      for (const line of lines) {
        if (!line.trim()) continue

        if (line.startsWith('message:')) {
          const jsonStr = line.replace('message:', '').trim()
          try {
            const data = JSON.parse(jsonStr)
            if (data.v && typeof data.v === 'string') {
              const aiMsg = messages.value.find((m) => m.id === aiMsgId)
              if (aiMsg) aiMsg.content += data.v
            }
          } catch (e) {
            console.error('解析消息失败', e)
          }
        }

        if (line.startsWith('aiChats:')) {
          const jsonStr = line.replace('aiChats:', '').trim()
          try {
            const data = JSON.parse(jsonStr)
            if (data.content) {
              currentTitle.value = data.content
              if (currentChats.value)
                currentChats.value = {
                  id: data.chatId,
                  title: data.content,
                }

              const histItem = history.value.find((h) => h.id == data.chatId)
              if (histItem) {
                histItem.title = data.content
              } else {
                history.value.unshift({
                  id: data.chatId,
                  aiAgentName: data.aiAgentName,
                  title: data.content,
                  modificationTime: new Date().toISOString(),
                })
              }
            }
          } catch (e) {}
        }

        // 改动点：error 流式内容存储到 errorContent 字段
        if (line.startsWith('error:')) {
          const jsonStr = line.replace('error:', '').trim()
          try {
            const data = JSON.parse(jsonStr)
            if (data.v && typeof data.v === 'string') {
              const aiMsg = messages.value.find((m) => m.id === aiMsgId)
              if (aiMsg) {
                aiMsg.errorContent = (aiMsg.errorContent || '') + data.v
              }
            }
          } catch (e) {
            console.error('解析错误消息失败', e)
          }
        }

        if (line.startsWith('close:')) {
          console.log('AI 响应结束')
        }
      }
    }
  } catch (error) {
    console.error('请求出错:', error)
    const aiMsg = messages.value.find((m) => m.id === aiMsgId)
    if (aiMsg) aiMsg.content = '抱歉，服务遇到了一点问题，请稍后再试。'
  }
}

// ==================== 外部点击处理 ====================
const handleOutsideClick = (e: MouseEvent) => {
  const target = e.target as HTMLElement
  if (!target.closest('.model-selector')) {
    isModelOpen.value = false
  }
}

// ==================== 历史记录操作 ====================

// 内联编辑状态
const editingItemId = ref<string | undefined>(undefined)
const editingTitle = ref('')
const renameHistory = (item: historyItem) => {
  editingItemId.value = item.id
  editingTitle.value = item.title
  nextTick(() => {
    const inputEl = document.querySelector(`input[data-edit-id="${item.id}"]`) as HTMLInputElement
    inputEl?.focus()
  })
}

const saveRename = async (item: historyItem) => {
  const newTitle = editingTitle.value.trim()
  if (!newTitle || newTitle === item.title) {
    editingItemId.value = undefined
    return
  }
  try {
    await updateTitleChats({ id: item.id, title: newTitle })
    item.title = newTitle
    if (currentChats.value?.id === item.id) {
      currentTitle.value = newTitle
      if (currentChats.value) {
        currentChats.value.title = newTitle
      }
    }
  } catch (e) {
    message.error('重命名失败')
  } finally {
    editingItemId.value = undefined
  }
}

const pinHistory = async (item: historyItem) => {
  if (item.id) {
    console.log('置顶', item)
    await updateTopChats(item.id)
    await getHistory()
  }
}

const deleteHistory = async (item: historyItem) => {
  if (item.id) {
    console.log('删除对话', item)
    await deleteChats(item.id)
    await getHistory()
  }
}

const onVoiceText = (text: string) => {
  inputText.value = text
}

// ==================== 生命周期 ====================
onMounted(() => {
  window.addEventListener('click', handleOutsideClick)
  getHistory()
  loadModels()
})

onUnmounted(() => window.removeEventListener('click', handleOutsideClick))
</script>

<template>
  <div
    :class="[
      'flex h-screen w-full overflow-hidden antialiased font-sans',
      isDark ? 'dark' : 'light',
    ]"
    class="bg-[var(--bg-primary)] text-[var(--text-primary)]"
  >
    <!-- 侧边栏 -->
    <aside
      class="w-64 hidden lg:flex flex-col flex-shrink-0 border-r"
      style="background-color: var(--bg-sidebar); border-color: var(--border-color)"
    >
      <div class="p-6">
        <button
          @click="newChatsHandle"
          class="w-full flex items-center justify-center gap-2 px-4 py-3 rounded-xl transition-all border shadow-sm active:scale-[0.98] text-[13px] font-medium"
          style="background-color: var(--bg-secondary); color: var(--text-secondary); border-color: var(--border-color)"
        >
          <span class="text-xl text-[var(--accent)]">+</span>
          <span>新建对话</span>
        </button>
      </div>

      <div class="flex-1 overflow-y-auto custom-scrollbar p-3">
        <div class="space-y-1">
          <template v-for="group in groupedHistory" :key="group.label">
            <div
              class="text-[10px] mt-3 mb-1 px-4 py-1 uppercase font-bold tracking-widest rounded-lg"
              style="background: var(--group-title-bg); color: var(--text-muted)"
            >
              {{ group.label }}
            </div>
            <div
              v-for="item in group.items"
              :key="item.id"
              class="history-item group flex items-center justify-between px-4 py-3 text-[13px] cursor-pointer transition-all rounded-xl relative"
              :class="currentTitle === item.title ? 'history-item-active bg-[var(--accent)]/10 text-[var(--accent)]' : 'hover:bg-[var(--bg-hover)]'"
              :style="{ color: currentTitle === item.title ? 'var(--accent)' : 'var(--text-tertiary)' }"
              @click="historyClickHandle(item)"
            >
              <template v-if="editingItemId === item.id">
                <Input
                  v-model:value="editingTitle"
                  :data-edit-id="item.id"
                  size="small"
                  class="flex-1 min-w-0"
                  style="background: var(--bg-input); color: var(--text-primary); border-color: var(--accent)"
                  @blur="saveRename(item)"
                  @keydown.enter.prevent="($event.target as HTMLInputElement).blur()"
                  @click.stop
                />
              </template>
              <template v-else>
                <span class="flex items-center gap-1.5 truncate flex-1 min-w-0">
                  <svg
                    v-if="item.isTop"
                    class="w-3.5 h-3.5 flex-shrink-0"
                    style="color: var(--accent)"
                    fill="currentColor"
                    viewBox="0 0 16 16"
                  >
                    <path d="M4.5 0a.5.5 0 0 1 .5.5V2h5V.5a.5.5 0 0 1 1 0V2h1a2 2 0 0 1 2 2v2a2 2 0 0 1-2 2h-1v6.5a.5.5 0 0 1-1 0V8h-5v6.5a.5.5 0 0 1-1 0V8h-1a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h1V.5A.5.5 0 0 1 4.5 0z"/>
                  </svg>
                  {{ item.title }}
                </span>
              </template>

              <Dropdown :trigger="['click']">
                <button
                  class="ml-2 p-1 rounded-md opacity-0 group-hover:opacity-100 transition-opacity hover:bg-[var(--bg-hover)] flex-shrink-0"
                  style="color: var(--text-muted)"
                  title="更多操作"
                  @click.stop
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 16 16">
                    <circle cx="8" cy="3" r="1.5" />
                    <circle cx="8" cy="8" r="1.5" />
                    <circle cx="8" cy="13" r="1.5" />
                  </svg>
                </button>
                <template #overlay>
                  <Menu>
                    <MenuItem @click="renameHistory(item)">重命名</MenuItem>
                    <MenuItem @click="pinHistory(item)"> {{ item.isTop ? '取消置顶' : '置顶' }}</MenuItem>
                    <MenuItem @click="deleteHistory(item)" danger>删除对话</MenuItem>
                  </Menu>
                </template>
              </Dropdown>
            </div>
          </template>
        </div>
      </div>

      <div class="p-4 border-t" style="border-color: var(--border-color)">
        <button
          @click="agentClickHandle()"
          class="w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all text-[13px]"
          style="color: var(--text-tertiary)"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 0 0 2.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 0 0 1.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 0 0-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 0 0-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 0 0-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 0 0-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 0 0 1.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
          </svg>
          <div v-if="currentAgent?.name">
            <span>{{ currentAgent.name }}</span>
          </div>
          <div v-else>
            <span>设置智能体</span>
          </div>
        </button>
      </div>
    </aside>

    <!-- 主内容区 -->
    <main class="flex-1 flex flex-col min-w-0 h-full overflow-hidden">
      <header
        class="h-14 flex-shrink-0 flex items-center justify-center px-8 border-b"
        style="border-color: var(--border-light)"
      >
        <div
          class="flex items-center gap-3 px-5 py-1.5 rounded-full border shadow-xl"
          style="background: var(--header-bg); border-color: var(--border-light)"
        >
          <div class="w-1.5 h-1.5 bg-[#52c41a] rounded-full animate-pulse shadow-[0_0_8px_#52c41a]"></div>
          <h1 class="text-[12px] font-medium tracking-wide max-w-[260px] truncate" style="color: var(--text-secondary)">
            {{ currentTitle }}
          </h1>
          <div class="h-3 w-[1px] mx-1" style="background-color: var(--text-muted)"></div>
          <span class="text-[9px] uppercase font-black" style="color: var(--text-muted)">Online</span>
        </div>
      </header>

      <!-- 消息滚动区 -->
      <section ref="scrollContainer" class="flex-1 overflow-y-auto custom-scrollbar scroll-smooth">
        <div class="max-w-4xl mx-auto py-8 px-6">
          <div
            v-for="msg in messages"
            :key="msg.id"
            :class="[
              'flex w-full mb-8 animate-fade-in',
              msg.role === 'user' ? 'justify-end' : 'justify-start',
            ]"
          >
            <div
              :class="[
                'flex max-w-[85%] items-start gap-4',
                msg.role === 'user' ? 'flex-row-reverse' : 'flex-row',
              ]"
            >
              <!-- AI 头像 -->
              <div
                v-if="msg.role === 'assistant'"
                class="w-9 h-9 rounded-full flex-shrink-0 flex items-center justify-center border-2"
                style="background-color: var(--bg-avatar); border-color: var(--accent)"
              >
                <svg class="w-5 h-5" style="color: var(--accent)" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                  stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M12 8V4H8"></path>
                  <rect width="16" height="12" x="4" y="8" rx="2"></rect>
                  <circle cx="9" cy="13" r="1"></circle>
                  <circle cx="15" cy="13" r="1"></circle>
                </svg>
              </div>

              <!-- 气泡内容 -->
              <div
                :class="[
                  'flex flex-col min-w-0',
                  msg.role === 'user' ? 'items-end' : 'items-start',
                ]"
              >
                <div
                  :class="[
                    'text-[14px] leading-[1.7] py-3 px-5 transition-all relative',
                    msg.role === 'user'
                      ? 'text-white rounded-2xl rounded-tr-none shadow-lg'
                      : 'rounded-2xl rounded-tl-none border',
                  ]"
                  :style="msg.role === 'user'
                    ? { backgroundColor: 'var(--accent)', boxShadow: '0 10px 15px -3px var(--accent-shadow)' }
                    : { backgroundColor: 'var(--bg-bubble-ai)', borderColor: 'var(--border-color)', color: 'var(--text-bubble-ai)' }"
                >
                  <!-- 区分普通消息与错误消息 -->
                  <div v-if="msg.role === 'assistant'">
                    <!-- 正常内容 -->
                    <div v-if="msg.content" class="markdown-body" v-html="md.render(msg.content)"></div>
                    <!-- 加载状态（无内容且无错误） -->
                    <span v-else-if="!msg.errorContent" class="animate-pulse">...</span>
                    <!-- 错误内容块 -->
                    <div v-if="msg.errorContent" class="error-block">
                      <svg class="error-icon" viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
                        <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
                      </svg>
                      <span>{{ msg.errorContent }}</span>
                    </div>
                  </div>

                  <div v-else class="whitespace-pre-wrap break-words">
                    {{ msg.content }}
                  </div>
                </div>

                <div v-if="msg.role === 'assistant'" class="mt-2 px-1">
                  <span class="text-[9px] font-bold uppercase tracking-widest" style="color: var(--text-muted)">
                    {{ selectedModelDisplay }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- 底部输入区 -->
      <footer
        class="flex-shrink-0 p-6"
        style="background: linear-gradient(to top, var(--bg-primary), var(--bg-primary), transparent)"
      >
        <div class="max-w-4xl mx-auto">
          <div
            class="input-wrapper rounded-2xl border transition-all duration-300"
            style="background-color: var(--bg-input); border-color: var(--border-color);"
          >
            <textarea
              ref="textareaRef"
              v-model="inputText"
              @keydown.enter.exact.prevent="sendMessage"
              placeholder="发送指令..."
              class="w-full bg-transparent border-none px-6 py-4 resize-none text-[14px] placeholder-gray-600 custom-scrollbar-textarea focus:ring-1 focus:ring-[var(--accent)]"
              style="color: var(--text-primary); min-height: 100px; max-height: 160px; outline: none;"
            ></textarea>

            <div
              class="flex items-center justify-between px-4 py-3 border-t"
              style="border-color: var(--border-light); background-color: var(--bg-input-footer)"
            >
              <div class="flex items-center gap-2 px-2 text-[11px] font-medium" style="color: var(--text-tertiary)">
                可按 Enter 发送              </div>

              <div class="flex items-center gap-3">
                <div class="relative model-selector">
                  <button
                    @click.stop="isModelOpen = !isModelOpen"
                    class="flex items-center gap-2 border text-[10px] font-bold px-3 py-1.5 rounded-lg transition-colors"
                    style="background-color: var(--bg-selector); border-color: var(--border-color); color: var(--text-tertiary)"
                  >
                    <span>{{ selectedModelDisplay }}</span>
                    <svg
                      :class="['w-3 h-3 transition-transform', isModelOpen ? 'rotate-180' : '']"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path d="M19 9l-7 7-7-7" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />
                    </svg>
                  </button>

                  <transition name="vben-pop">
                  <div v-if="isModelOpen"
                    class="absolute bottom-full mb-2 right-0 w-55 border rounded-xl shadow-2xl p-1.5 z-[100] custom-scrollbar"
                    style="max-height: 260px; overflow-y: auto; background-color: var(--bg-dropdown); border-color: var(--border-color)">
                      <div
                        v-for="model in models"
                        :key="model.id"
                        @click="selectedModelId = model.id; isModelOpen = false"
                        :class="[
                          'px-3 py-2 text-[11px] font-medium rounded-lg cursor-pointer transition-all mb-0.5',
                          selectedModelId === model.id
                            ? 'bg-[var(--accent)]/20 text-[var(--accent)]'
                            : 'hover:bg-[var(--bg-hover)]'
                        ]"
                        :style="{ color: selectedModelId === model.id ? 'var(--accent)' : 'var(--text-secondary)' }"
                      >
                        {{ model.keyName }}_{{ model.configName }}
                      </div>
                    </div>
                  </transition>
                </div>

                <button
                  @click="sendMessage"
                  :disabled="!inputText.trim()"
                  :class="[
                    'px-5 py-1.5 rounded-xl transition-all font-bold text-[12px] flex items-center gap-2',
                    inputText.trim()
                      ? 'text-white shadow-lg'
                      : 'cursor-not-allowed'
                  ]"
                  :style="inputText.trim()
                    ? { backgroundColor: 'var(--accent)', boxShadow: '0 10px 15px -3px var(--accent-shadow)' }
                    : { backgroundColor: 'var(--bg-disabled)', color: 'var(--text-muted)' }"
                >
                  <span>发送</span>
                  <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
                  </svg>
                </button>
                <VoiceInputButton @text-recognized="onVoiceText" />
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

<style>
/* 暗黑模式 */
.dark {
  --bg-primary: #050505;
  --bg-secondary: #161616;
  --bg-sidebar: #0d0d0d;
  --bg-hover: #1a1a1a;
  --bg-input: #111;
  --bg-input-footer: rgba(255, 255, 255, 0.01);
  --bg-bubble-ai: #1a1a1a;
  --bg-avatar: #161616;
  --bg-selector: #1a1a1a;
  --bg-dropdown: #1a1a1a;
  --bg-disabled: #1a1a1a;

  --text-primary: #e5e7eb;
  --text-secondary: #d1d5db;
  --text-tertiary: #9ca3af;
  --text-muted: #6b7280;
  --text-bubble-ai: #cbd5e1;

  --border-color: #262626;
  --border-light: rgba(255, 255, 255, 0.02);

  --accent: #0960bd;
  --accent-shadow: rgba(9, 96, 189, 0.2);

  --header-bg: rgba(17, 17, 17, 0.6);
  --group-title-bg: rgba(255, 255, 255, 0.03);

  /* 错误颜色变量 (暗色) */
  --error-bg: #2d1617;
  --error-border: #5c2527;
  --error-text: #e8a4a4;
  --error-icon: #ff6b6b;
}

/* 白天模式 */
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

  /* 错误颜色变量 (亮色) */
  --error-bg: #fff2f0;
  --error-border: #ffccc7;
  --error-text: #cf1322;
  --error-icon: #ff4d4f;
}
</style>

<style scoped>
.h-screen {
  height: 90vh;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 10px;
}

.custom-scrollbar-textarea::-webkit-scrollbar {
  display: none;
}

.animate-fade-in {
  animation: fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
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

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

textarea {
  scrollbar-width: none;
}

/* Markdown 主体 */
.markdown-body {
  word-break: break-word;
  line-height: 1.75;
  color: inherit;
}

.markdown-body :deep(> *:last-child) {
  margin-bottom: 0 !important;
}

.markdown-body :deep(h1),
.markdown-body :deep(h2),
.markdown-body :deep(h3) {
  color: inherit;
  font-weight: 600;
  margin-top: 1.5rem;
  margin-bottom: 0.75rem;
  line-height: 1.3;
}

.markdown-body :deep(h1) {
  font-size: 1.5rem;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 0.3rem;
}

.markdown-body :deep(h2) {
  font-size: 1.25rem;
}

.markdown-body :deep(h3) {
  font-size: 1.1rem;
}

.markdown-body :deep(p) {
  margin-bottom: 1rem;
}

.markdown-body :deep(ul),
.markdown-body :deep(ol) {
  padding-left: 1.5rem;
  margin-bottom: 1rem;
}

.markdown-body :deep(li) {
  margin-bottom: 0.25rem;
}

.markdown-body :deep(li > ul),
.markdown-body :deep(li > ol) {
  margin-top: 0.25rem;
  margin-bottom: 0;
}

.markdown-body :deep(blockquote) {
  margin: 1rem 0;
  padding: 0 1rem;
  color: var(--text-muted);
  border-left: 0.25rem solid var(--accent);
  background: rgba(9, 96, 189, 0.05);
}

.markdown-body :deep(code:not(pre code)) {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  background-color: var(--bg-hover);
  padding: 0.2rem 0.4rem;
  border-radius: 6px;
  font-size: 88%;
  color: inherit;
}

.markdown-body :deep(pre) {
  background-color: var(--bg-secondary);
  padding: 1rem;
  border-radius: 12px;
  overflow-x: auto;
  margin: 1.2rem 0;
  border: 1px solid var(--border-color);
}

.markdown-body :deep(pre code) {
  background-color: transparent;
  padding: 0;
  font-size: 13px;
  line-height: 1.5;
  color: inherit;
  font-family: 'Fira Code', 'Cascadia Code', monospace;
}

.markdown-body :deep(table) {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 1rem;
  font-size: 13px;
}

.markdown-body :deep(th),
.markdown-body :deep(td) {
  padding: 8px 12px;
  border: 1px solid var(--border-color);
}

.markdown-body :deep(th) {
  background-color: var(--bg-hover);
  text-align: left;
}

.markdown-body :deep(a) {
  color: var(--accent);
  text-decoration: none;
  border-bottom: 1px solid transparent;
  transition: border-color 0.2s;
}

.markdown-body :deep(a:hover) {
  border-bottom-color: var(--accent);
}

.markdown-body :deep(pre)::-webkit-scrollbar {
  height: 6px;
}

.markdown-body :deep(pre)::-webkit-scrollbar-thumb {
  background: var(--border-color);
  border-radius: 10px;
}

.markdown-body :deep(pre)::-webkit-scrollbar-thumb:hover {
  background: var(--text-muted);
}

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

.input-wrapper {
  box-shadow: 0 0 0 0 transparent;
}
.input-wrapper:focus-within {
  border-color: var(--accent);
  box-shadow: 0 0 0 3px var(--accent-shadow);
}

/* 错误信息样式（使用主题变量） */
.error-block {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  margin-top: 12px;
  padding: 10px 12px;
  background: var(--error-bg);
  border: 1px solid var(--error-border);
  border-radius: 8px;
  color: var(--error-text);
  font-size: 13px;
  line-height: 1.5;
}
.error-icon {
  flex-shrink: 0;
  margin-top: 2px;
  color: var(--error-icon);
}
</style>
