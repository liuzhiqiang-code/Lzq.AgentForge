import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace WorkshopApi {
  export interface WorkshopItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    factoryId?: number;
    factoryName?: string;
    manager?: string;
    status?: number;
  }
}

/**
 * 分页查询车间
 */
async function getWorkshopPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<WorkshopApi.WorkshopItem>>(
    '/mes/workshop/page',
    params,
  );
}

/**
 * 按工厂获取车间列表(联动下拉)
 */
async function getWorkshopListByFactory(factoryId: number) {
  return requestClient.post<Array<WorkshopApi.WorkshopItem>>(
    `/mes/workshop/list-by-factory/${factoryId}`,
  );
}

/**
 * 创建车间
 */
async function createWorkshop(data: WorkshopApi.WorkshopItem) {
  return requestClient.post('/mes/workshop/create', data);
}

/**
 * 更新车间
 */
async function updateWorkshop(data: WorkshopApi.WorkshopItem) {
  return requestClient.put('/mes/workshop/update', data);
}

/**
 * 删除车间
 */
async function deleteWorkshop(id: number) {
  return requestClient.delete(`/mes/workshop/delete/${id}`);
}

/**
 * 批量删除车间
 */
async function batchDeleteWorkshop(ids: number[]) {
  return requestClient.post('/mes/workshop/batch-delete', ids);
}

export {
  batchDeleteWorkshop,
  createWorkshop,
  deleteWorkshop,
  getWorkshopListByFactory,
  getWorkshopPage,
  updateWorkshop,
};
