import type { ChatsApi } from '#/modules/ai/api/chats'

export type StreamingEventArgs =
  | { type: 'thinking'; content: string; collapsed: boolean }
  | { type: 'message'; content: string }
  | { type: 'tool'; callId: string; toolName: string; arguments?: string; result?: string; status: 'running' | 'done'; collapsed: boolean }
  | { type: 'chart'; content: string; height?: string }

export interface ExtendedMessage extends ChatsApi.ChatsHistory {
  errorContent?: string
  segments?: StreamingEventArgs []
}

export type HistoryItem = {
  id?: string
  aiAgentName: string
  title: string
  modificationTime?: string
  isTop?: boolean
}
