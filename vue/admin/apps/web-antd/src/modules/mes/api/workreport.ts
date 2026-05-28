import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace WorkReportApi {
  export interface WorkReportItem {
    [key: string]: any;
    id?: number;
    workOrderId?: number;
    workOrderCode?: string;
    operatorId?: string;
    qualifiedQty?: number;
    defectQty?: number;
    workHours?: number;
    reportTime?: string;
    remark?: string;
  }
}

/**
 * 锟斤拷页锟斤拷询锟斤拷锟斤拷锟斤拷录
 */
async function getWorkReportPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<WorkReportApi.WorkReportItem>>(
    '/mes/work-report/page',
    params,
  );
}

/**
 * 锟斤拷锟斤拷锟斤拷锟斤拷询锟斤拷锟斤拷锟叫憋拷
 */
async function getWorkReportListByWorkOrder(workOrderId: number) {
  return requestClient.post<Array<WorkReportApi.WorkReportItem>>(
    `/mes/work-report/list/${workOrderId}`,
  );
}

/**
 * 锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷录
 */
async function createWorkReport(data: WorkReportApi.WorkReportItem) {
  return requestClient.post('/mes/work-report/create', data);
}

/**
 * 删锟斤拷锟斤拷锟斤拷锟斤拷录
 */
async function deleteWorkReport(id: number) {
  return requestClient.delete(`/mes/work-report/delete/${id}`);
}

export {
  createWorkReport,
  deleteWorkReport,
  getWorkReportListByWorkOrder,
  getWorkReportPage,
};
