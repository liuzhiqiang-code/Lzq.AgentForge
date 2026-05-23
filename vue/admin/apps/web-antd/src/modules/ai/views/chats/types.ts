import type { ChatsApi } from '#/modules/ai/api/chats'

export type StreamingEventArgs =
  | { type: 'thinking'; content: string; collapsed: boolean }
  | { type: 'message'; content: string }
  | { type: 'tool'; callId: string; toolName: string; arguments?: string; result?: string; status: 'running' | 'done'; collapsed: boolean }
  | {
      type: 'echarts'
      /** 图表状态：loading 为生成中，done 为已完成 */
      status: 'loading' | 'done'
      /** 图表标题（可选，用于折叠按钮） */
      title?: string
      /** 图表类型（可选，如 line、bar） */
      chartType?: string
      /** ECharts 配置项（仅在 status 为 done 时有效） */
      chartOption?: any
      /** 用于匹配 echarts_start 与 echarts_end */
      callId?: string
      /** 是否折叠（初始可折叠） */
      collapsed?: boolean,
    }

export interface ExtendedMessage extends ChatsApi.ChatsHistory {
  errorContent?: string
  segments?: StreamingEventArgs[]
}

export type HistoryItem = {
  id?: string
  aiAgentName: string
  title: string
  modificationTime?: string
  isTop?: boolean
}
