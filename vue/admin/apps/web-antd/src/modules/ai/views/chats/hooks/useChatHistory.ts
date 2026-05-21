import { ref, computed } from 'vue'
import { getChatsList, getChatsHistory, deleteChats, updateTitleChats, updateTopChats } from '#/modules/ai/api/chats'
import { message } from 'ant-design-vue'
import type { ExtendedMessage, HistoryItem } from '../types'

export function useChatHistory() {
  const history = ref<HistoryItem[]>([])
  const currentChats = ref<ChatsApi.Chats>()
  const currentAgent = ref<ChatsApi.ChatsAgent>()
  const currentTitle = ref('新对话')
  const messages = ref<ExtendedMessage[]>([])

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

    const groups = [
      { label: '置顶', items: [] as HistoryItem[] },
      { label: '今天', items: [] as HistoryItem[] },
      { label: '昨天', items: [] as HistoryItem[] },
      { label: '7 天内', items: [] as HistoryItem[] },
      { label: '30 天内', items: [] as HistoryItem[] },
      { label: '更早', items: [] as HistoryItem[] },
    ]

    const pinned: HistoryItem[] = []
    const unpinned: HistoryItem[] = []

    for (const item of history.value) {
      if (item.isTop) pinned.push(item)
      else unpinned.push(item)
    }
    groups[0].items = pinned

    for (const item of unpinned) {
      const time = item.modificationTime ? new Date(item.modificationTime) : null
      if (!time) { groups[5].items.push(item); continue }
      if (time >= todayStart) groups[1].items.push(item)
      else if (time >= yesterdayStart) groups[2].items.push(item)
      else if (time >= sevenDaysAgo) groups[3].items.push(item)
      else if (time >= thirtyDaysAgo) groups[4].items.push(item)
      else groups[5].items.push(item)
    }

    return groups.filter(g => g.items.length > 0)
  })

  const historyClickHandle = async (item: HistoryItem) => {
    currentChats.value = item
    currentTitle.value = item.title
    currentAgent.value = { name: item.aiAgentName }
    messages.value = await getChatsHistory(item.id ?? '') as ExtendedMessage[]
  }

  const newChatsHandle = () => {
    messages.value = []
    currentChats.value = { id: '0', title: '新对话', aiAgentName: '' }
    currentTitle.value = '新对话'
    currentAgent.value = { name: '' }
  }

  const deleteHistory = async (item: HistoryItem) => {
    if (item.id) {
      await deleteChats(item.id)
      await getHistory()
    }
  }

  const pinHistory = async (item: HistoryItem) => {
    if (item.id) {
      await updateTopChats(item.id)
      await getHistory()
    }
  }

  const renameHistory = async (item: HistoryItem, newTitle: string) => {
    if (!newTitle.trim() || newTitle.trim() === item.title) return
    await updateTitleChats({ id: item.id, title: newTitle })
    item.title = newTitle
    if (currentChats.value?.id === item.id) {
      currentTitle.value = newTitle
      if (currentChats.value) currentChats.value.title = newTitle
    }
  }

  return {
    history, groupedHistory, currentChats, currentAgent, currentTitle, messages,
    getHistory, historyClickHandle, newChatsHandle,
    deleteHistory, pinHistory, renameHistory,
  }
}
