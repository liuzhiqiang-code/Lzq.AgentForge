import { requestClient } from '#/api/request';
import type { Recordable } from '@vben/types';

export namespace ApiKeyApi {
  export interface ApiKey {
    [key: string]: any;
    id:string;
    provider?:string;
    keyName?:string;
    keyValue?:string;
    baseUrl?:string;
    isEnabled?:string;
    creator:string;
    creationTime:string;
    modifier:string;
    modificationTime:string;
  }
}

/**
 * 获取列表数据分页
 */
export async function getApiKeyPage(data: Recordable<any>) {
  return requestClient.post<Array<ApiKeyApi.ApiKey>>(
    '/ai/apiKey/page',data
  );
}

/**
 * 获取列表数据
 */
export async function getApiKeyList() {
  return requestClient.post<Array<ApiKeyApi.ApiKey>>(
    '/ai/apiKey/list',
  );
}

/**
 * 获取明细数据
 */
export async function getDetail(id: string) {
  return requestClient.get<ApiKeyApi.ApiKey>(
    `/ai/apiKey/detail/${id}`,
  );
}

/** 获取可用模型（根据厂商和 Key�?*/
export async function getAvailableModels(provider: number, keyValue: string) {
  return requestClient.post<string[]>('/ai/apiKey/getAvailableModels', { provider, keyValue });
}

/**
 * 创建
 * @param data 数据
 */
export async function createApiKey(
  data: ApiKeyApi.ApiKey,
) {
  return requestClient.post('/ai/apiKey/create', data);
}

/**
 * 更新
 * @param data 数据
 */
export async function updateApiKey(
  data: ApiKeyApi.ApiKey,
) {
  return requestClient.put(`/ai/apiKey/update`, data);
}

/**
 * 删除
 * @param id ID
 */
export async function deleteApiKey(id: string) {
  return requestClient.delete(`/ai/apiKey/delete/${id}`);
}
