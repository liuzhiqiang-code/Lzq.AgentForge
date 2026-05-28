import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace MaintenancePlanApi {
  export interface MaintenancePlanItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    equipmentId?: number;
    equipmentCode?: string;
    equipmentName?: string;
    maintenanceType?: number;
    maintenanceTypeName?: string;
    cycleDays?: number;
    planDate?: string;
    actualDate?: string;
    status?: number;
    statusName?: string;
    responsibleId?: number;
    responsibleName?: string;
    content?: string;
    standard?: string;
    durationMinutes?: number;
    remark?: string;
    createTime?: string;
  }
}

export namespace MaintenanceRecordApi {
  export interface MaintenanceRecordItem {
    [key: string]: any;
    id?: number;
    code?: string;
    planId?: number;
    equipmentId?: number;
    equipmentCode?: string;
    equipmentName?: string;
    maintenanceType?: number;
    maintenanceTypeName?: string;
    maintenanceDate?: string;
    maintainerId?: number;
    maintainerName?: string;
    durationMinutes?: number;
    content?: string;
    result?: string;
    images?: string;
    problemsFound?: string;
    nextReminderDate?: string;
    remark?: string;
    createTime?: string;
  }
}

// ====== 保养计划 ======

/**
 * 分页查询保养计划
 */
async function getMaintenancePlanPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<MaintenancePlanApi.MaintenancePlanItem>>(
    '/mes/maintenance/plan/page',
    params,
  );
}

/**
 * 获取保养计划详情
 */
async function getMaintenancePlanDetail(id: number) {
  return requestClient.get<MaintenancePlanApi.MaintenancePlanItem>(
    `/mes/maintenance/plan/${id}`,
  );
}

/**
 * 创建保养计划
 */
async function createMaintenancePlan(data: MaintenancePlanApi.MaintenancePlanItem) {
  return requestClient.post('/mes/maintenance/plan/create', data);
}

/**
 * 删除保养计划
 */
async function deleteMaintenancePlan(id: number) {
  return requestClient.delete(`/mes/maintenance/plan/delete/${id}`);
}

// ====== 保养记录 ======

/**
 * 分页查询保养记录
 */
async function getMaintenanceRecordPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<MaintenanceRecordApi.MaintenanceRecordItem>>(
    '/mes/maintenance/record/page',
    params,
  );
}

/**
 * 创建保养记录
 */
async function createMaintenanceRecord(data: MaintenanceRecordApi.MaintenanceRecordItem) {
  return requestClient.post('/mes/maintenance/record/create', data);
}

export {
  createMaintenancePlan,
  createMaintenanceRecord,
  deleteMaintenancePlan,
  getMaintenancePlanDetail,
  getMaintenancePlanPage,
  getMaintenanceRecordPage,
};
