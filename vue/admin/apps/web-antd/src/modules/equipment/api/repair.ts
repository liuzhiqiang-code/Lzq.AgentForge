import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace RepairApi {
  export interface RepairItem {
    [key: string]: any;
    id?: number;
    code?: string;
    equipmentId?: number;
    equipmentCode?: string;
    equipmentName?: string;
    repairType?: number;
    repairTypeName?: string;
    description?: string;
    images?: string;
    reporterId?: number;
    reporterName?: string;
    reportTime?: string;
    priority?: number;
    priorityName?: string;
    status?: number;
    statusName?: string;
    repairUserId?: number;
    repairUserName?: string;
    repairStartTime?: string;
    repairEndTime?: string;
    faultReason?: string;
    repairProcess?: string;
    partsUsed?: string;
    workHours?: number;
    cost?: number;
    acceptorId?: number;
    acceptorName?: string;
    acceptTime?: string;
    acceptComment?: string;
    remark?: string;
    createTime?: string;
  }
}

/**
 * 分页查询维修单
 */
async function getRepairPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<RepairApi.RepairItem>>(
    '/mes/repair/page',
    params,
  );
}

/**
 * 获取维修单详情
 */
async function getRepairDetail(id: number) {
  return requestClient.get<RepairApi.RepairItem>(`/mes/repair/${id}`);
}

/**
 * 创建维修单
 */
async function createRepair(data: RepairApi.RepairItem) {
  return requestClient.post('/mes/repair/create', data);
}

/**
 * 派工
 */
async function assignRepair(data: { id: number; repairUserId: number; repairUserName?: string }) {
  return requestClient.post('/mes/repair/assign', data);
}

/**
 * 开始维修
 */
async function startRepair(data: { id: number }) {
  return requestClient.post('/mes/repair/start', data);
}

/**
 * 完成维修
 */
async function completeRepair(data: {
  id: number;
  faultReason?: string;
  repairProcess?: string;
  partsUsed?: string;
  workHours?: number;
  cost?: number;
}) {
  return requestClient.post('/mes/repair/complete', data);
}

/**
 * 验收
 */
async function acceptRepair(data: { id: number; acceptComment?: string }) {
  return requestClient.post('/mes/repair/accept', data);
}

/**
 * 取消维修单
 */
async function cancelRepair(id: number) {
  return requestClient.post(`/mes/repair/cancel/${id}`);
}

/**
 * 维修统计
 */
async function getRepairStatistics() {
  return requestClient.get('/mes/repair/statistics');
}

export {
  acceptRepair,
  assignRepair,
  cancelRepair,
  completeRepair,
  createRepair,
  getRepairDetail,
  getRepairPage,
  getRepairStatistics,
  startRepair,
};
