import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types'
import type { Recordable } from '@vben/types';

export namespace ModelConfigApi {
  export interface ModelConfig {
    [key: string]: any;
    id:string;
    apiKeyId?:string;
    configName?:string;
    displayModelName?:string;
    contextLength?:string;
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
export async function getModelConfigPage(data: Recordable<any>) {
  return requestClient.post<PagedResponse<ModelConfigApi.ModelConfig>>(
    '/ai/modelConfig/page',data
  );
}

/**
 * 获取列表数据
 */
export async function getModelConfigList() {
  return requestClient.post<Array<ModelConfigApi.ModelConfig>>(
    '/ai/modelConfig/list',
  );
}

/**
 * 创建
 * @param data 数据
 */
export async function createModelConfig(
  data: ModelConfigApi.ModelConfig,
) {
  return requestClient.post('/ai/modelConfig/create', data);
}

/**
 * 更新
 * @param data 数据
 */
export async function updateModelConfig(
  data: ModelConfigApi.ModelConfig,
) {
  return requestClient.put(`/ai/modelConfig/update`, data);
}

/**
 * 删除
 * @param id ID
 */
export async function deleteModelConfig(id: string) {
  return requestClient.delete(`/ai/modelConfig/delete/${id}`);
}
