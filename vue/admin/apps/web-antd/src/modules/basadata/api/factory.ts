import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace FactoryApi {
  export interface FactoryItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    address?: string;
    status?: number;
  }
}

/**
 * 分页查询工厂
 */
async function getFactoryPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<FactoryApi.FactoryItem>>(
    '/mes/factory/page',
    params,
  );
}

/**
 * 获取工厂树
 */
async function getFactoryTree() {
  return requestClient.post<Array<FactoryApi.FactoryItem>>(
    '/mes/factory/tree',
  );
}

/**
 * 创建工厂
 */
async function createFactory(data: FactoryApi.FactoryItem) {
  return requestClient.post('/mes/factory/create', data);
}

/**
 * 更新工厂
 */
async function updateFactory(data: FactoryApi.FactoryItem) {
  return requestClient.put('/mes/factory/update', data);
}

/**
 * 删除工厂
 */
async function deleteFactory(id: number) {
  return requestClient.delete(`/mes/factory/delete/${id}`);
}

/**
 * 批量删除工厂
 */
async function batchDeleteFactory(ids: number[]) {
  return requestClient.post('/mes/factory/batch-delete', ids);
}

export {
  batchDeleteFactory,
  createFactory,
  deleteFactory,
  getFactoryPage,
  getFactoryTree,
  updateFactory,
};
