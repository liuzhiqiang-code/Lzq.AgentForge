import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace InspectionPlanApi {
  export interface InspectionPlanItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    equipmentId?: number;
    equipmentCode?: string;
    equipmentName?: string;
    cycleType?: number;
    cycleTypeName?: string;
    cycleValue?: number;
    nextInspectDate?: string;
    itemCount?: number;
    executorId?: number;
    executorName?: string;
    isEnabled?: boolean;
    remark?: string;
    createTime?: string;
    updateTime?: string;
  }
}

export namespace InspectionRecordApi {
  export interface InspectionRecordItem {
    [key: string]: any;
    id?: number;
    code?: string;
    planId?: number;
    equipmentId?: number;
    equipmentCode?: string;
    equipmentName?: string;
    inspectDate?: string;
    result?: number;
    resultName?: string;
    inspectorId?: number;
    inspectorName?: string;
    completedTime?: string;
    durationMinutes?: number;
    abnormalDesc?: string;
    createRepairOrder?: boolean;
    repairOrderId?: number;
    remark?: string;
    createTime?: string;
  }
}

// ====== 点检计划 ======

/**
 * 分页查询点检计划
 */
async function getInspectionPlanPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<InspectionPlanApi.InspectionPlanItem>>(
    '/mes/inspection/plan/page',
    params,
  );
}

/**
 * 获取点检计划详情
 */
async function getInspectionPlanDetail(id: number) {
  return requestClient.get<InspectionPlanApi.InspectionPlanItem>(
    `/mes/inspection/plan/${id}`,
  );
}

/**
 * 获取点检计划项目
 */
async function getInspectionPlanItems(planId: number) {
  return requestClient.get('/mes/inspection/plan/items', {
    params: { planId },
  });
}

/**
 * 创建点检计划
 */
async function createInspectionPlan(data: InspectionPlanApi.InspectionPlanItem) {
  return requestClient.post('/mes/inspection/plan/create', data);
}

/**
 * 更新点检计划
 */
async function updateInspectionPlan(data: InspectionPlanApi.InspectionPlanItem) {
  return requestClient.put('/mes/inspection/plan/update', data);
}

/**
 * 删除点检计划
 */
async function deleteInspectionPlan(id: number) {
  return requestClient.delete(`/mes/inspection/plan/delete/${id}`);
}

/**
 * 获取待执行点检计划
 */
async function getPendingInspectionPlans() {
  return requestClient.get('/mes/inspection/plan/pending');
}

// ====== 点检记录 ======

/**
 * 分页查询点检记录
 */
async function getInspectionRecordPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<InspectionRecordApi.InspectionRecordItem>>(
    '/mes/inspection/record/page',
    params,
  );
}

/**
 * 获取今日点检记录
 */
async function getTodayInspectionRecords() {
  return requestClient.get('/mes/inspection/record/today');
}

/**
 * 执行点检
 */
async function executeInspection(data: Recordable<any>) {
  return requestClient.post('/mes/inspection/execute', data);
}

export {
  createInspectionPlan,
  deleteInspectionPlan,
  executeInspection,
  getInspectionPlanDetail,
  getInspectionPlanItems,
  getInspectionPlanPage,
  getInspectionRecordPage,
  getPendingInspectionPlans,
  getTodayInspectionRecords,
  updateInspectionPlan,
};
