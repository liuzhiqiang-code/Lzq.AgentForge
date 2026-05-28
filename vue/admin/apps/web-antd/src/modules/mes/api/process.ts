import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace ProcessApi {
  export interface ProcessItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    lineId?: number;
    lineName?: string;
    sequence?: number;
    standardHours?: number;
    status?: number;
  }
}

/**
 * 分页查询工序
 */
async function getProcessPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<ProcessApi.ProcessItem>>(
    '/mes/process/page',
    params,
  );
}

/**
 * 按产线获取工序列表(联动下拉)
 */
async function getProcessListByLine(lineId: number) {
  return requestClient.post<Array<ProcessApi.ProcessItem>>(
    `/mes/process/list-by-line/${lineId}`,
  );
}

/**
 * 创建工序
 */
async function createProcess(data: ProcessApi.ProcessItem) {
  return requestClient.post('/mes/process/create', data);
}

/**
 * 更新工序
 */
async function updateProcess(data: ProcessApi.ProcessItem) {
  return requestClient.put('/mes/process/update', data);
}

/**
 * 删除工序
 */
async function deleteProcess(id: number) {
  return requestClient.delete(`/mes/process/delete/${id}`);
}

/**
 * 批量删除工序
 */
async function batchDeleteProcess(ids: number[]) {
  return requestClient.post('/mes/process/batch-delete', ids);
}

export {
  batchDeleteProcess,
  createProcess,
  deleteProcess,
  getProcessListByLine,
  getProcessPage,
  updateProcess,
};
