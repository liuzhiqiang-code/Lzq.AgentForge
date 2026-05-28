import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace WorkOrderApi {
  export interface WorkOrderItem {
    [key: string]: any;
    id?: number;
    code?: string;
    productName?: string;
    lineId?: number;
    lineName?: string;
    processId?: number;
    processName?: string;
    plannedQty?: number;
    completedQty?: number;
    defectQty?: number;
    status?: number;
    plannedStart?: string;
    plannedEnd?: string;
    actualStart?: string;
    actualEnd?: string;
  }
}

/**
 * 分页查询工单
 */
async function getWorkOrderPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<WorkOrderApi.WorkOrderItem>>(
    '/mes/work-order/page',
    params,
  );
}

/**
 * 获取工单详情
 */
async function getWorkOrderDetail(id: number) {
  return requestClient.get<WorkOrderApi.WorkOrderItem>(`/mes/work-order/${id}`);
}

/**
 * 创建工单
 */
async function createWorkOrder(data: WorkOrderApi.WorkOrderItem) {
  return requestClient.post('/mes/work-order/create', data);
}

/**
 * 更新工单
 */
async function updateWorkOrder(data: WorkOrderApi.WorkOrderItem) {
  return requestClient.put('/mes/work-order/update', data);
}

/**
 * 删除工单
 */
async function deleteWorkOrder(id: number) {
  return requestClient.delete(`/mes/work-order/delete/${id}`);
}

/**
 * 批量删除工单
 */
async function batchDeleteWorkOrder(ids: number[]) {
  return requestClient.post('/mes/work-order/batch-delete', ids);
}

/**
 * 派发工单
 */
async function dispatchWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/dispatch', data);
}

/**
 * 开始工单
 */
async function startWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/start', data);
}

/**
 * 完工工单
 */
async function completeWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/complete', data);
}

/**
 * 关闭工单
 */
async function closeWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/close', data);
}

/**
 * 取消工单
 */
async function cancelWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/cancel', data);
}

/**
 * 暂停工单
 */
async function pauseWorkOrder(data: { id: number }) {
  return requestClient.post('/mes/work-order/pause', data);
}

export {
  batchDeleteWorkOrder,
  cancelWorkOrder,
  closeWorkOrder,
  completeWorkOrder,
  createWorkOrder,
  deleteWorkOrder,
  dispatchWorkOrder,
  getWorkOrderDetail,
  getWorkOrderPage,
  pauseWorkOrder,
  startWorkOrder,
  updateWorkOrder,
};
