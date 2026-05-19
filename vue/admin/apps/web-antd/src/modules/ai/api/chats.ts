import { requestClient } from '#/api/request';

export namespace ChatsApi {
  export interface Chats {
    [key: string]: any;
    id?: string;
    aiChatsName?: string;
    title?: string;
    aiAgentName?: string;
    lastMessage?: string;
    isTop?: boolean;
    userId?: string;
    creator?: string;
    creationTime?: string;
    modifier?: string;
    modificationTime?: string;
  }
  export interface ChatsHistory {
    [key: string]: any;
    id: string;
    role: 'user' | 'assistant';
    content: string;
  }
  export interface ChatsAgent {
    [key: string]: any;
    id?: string;
    name?: string;
  }
  export interface ChatsModel {
    [key: string]: any;
    id: string;
    configName: string;
    keyName: string;
  }
}

/**
 * 获取列表数据
 */
export async function getChatsList() {
  return requestClient.post<Array<ChatsApi.Chats>>(
    '/ai/chats/list'
  );
}

/**
 * 获取对话历史消息
 * @param id ID
 */
export async function getChatsHistory(id: string) {
  return requestClient.post<Array<ChatsApi.ChatsHistory>>(`/ai/chats/history/${id}`);
}

/**
 * 获取对话可调用模�? */
export async function getModels() {
  return requestClient.post<Array<ChatsApi.ChatsModel>>(`/ai/chats/models`);
}

/** 语音转文�?*/
export async function speechToText(file: File | Blob) {
  const formData = new FormData()
  formData.append('file', file, 'recording.webm')
  return requestClient.post('/ai/chats/speech-to-text', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}

/**
 * 创建
 * @param data 数据
 */
export async function createChats(
  data: ChatsApi.Chats,
) {
  return requestClient.post('/ai/chats/create', data);
}

/**
 * 更新
 * @param data 数据
 */
export async function updateChats(
  data: ChatsApi.Chats,
) {
  return requestClient.put(`/ai/chats/update`, data);
}

/**
 * 更新标题
 * @param data 数据
 */
export async function updateTitleChats(
  data: ChatsApi.Chats,
) {
  return requestClient.put(`/ai/chats/updateTitle`, data);
}

/**
 * 更新置顶信息
 * @param id ID
 * @param data 数据
 */
export async function updateTopChats(id: string) {
  return requestClient.put(`/ai/chats/updateTop/${id}`);
}

/**
 * 删除
 * @param id ID
 */
export async function deleteChats(id: string) {
  return requestClient.delete(`/ai/chats/delete/${id}`);
}
