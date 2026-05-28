import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace DefectApi {
  export interface DefectItem {
    [key: string]: any;
    id?: number;
    qcOrderId?: number;
    qcOrderCode?: string;
    workOrderId?: number;
    workOrderCode?: string;
    productId?: number;
    productName?: string;
    productSpec?: string;
    batchNo?: string;
    defectQty?: number;
    defectCode?: string;
    defectDesc?: string;
    defectImages?: string;
    status?: number;
    statusName?: string;
    handlingType?: number;
    handlingTypeName?: string;
    handlingRemark?: string;
    handlerId?: number;
    handlerName?: string;
    handlingTime?: string;
    needReview?: boolean;
    reviewResult?: string;
    reviewer?: string;
    reviewTime?: string;
    remark?: string;
    createTime?: string;
    updateTime?: string;
  }
}

/**
 * 分页查询缺陷记录
 */
async function getDefectPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<DefectApi.DefectItem>>(
    '/mes/defect/page',
    params,
  );
}

/**
 * 获取缺陷详情
 */
async function getDefectDetail(id: number) {
  return requestClient.get<DefectApi.DefectItem>(`/mes/defect/${id}`);
}

/**
 * 按质检单查询缺陷列表
 */
async function getDefectListByQCOrder(qcOrderId: number) {
  return requestClient.get<DefectApi.DefectItem[]>(
    `/mes/defect/by-qc-order/${qcOrderId}`,
  );
}

/**
 * 创建缺陷记录
 */
async function createDefect(data: DefectApi.DefectItem) {
  return requestClient.post('/mes/defect/create', data);
}

/**
 * 删除缺陷记录
 */
async function deleteDefect(id: number) {
  return requestClient.delete(`/mes/defect/delete/${id}`);
}

/**
 * 处理缺陷
 */
async function handleDefect(data: {
  id: number;
  handlingType: number;
  handlingRemark?: string;
  handlerId?: number;
}) {
  return requestClient.post('/mes/defect/handle', data);
}

/**
 * 评审缺陷
 */
async function reviewDefect(data: {
  id: number;
  reviewResult: string;
  reviewer?: string;
}) {
  return requestClient.post('/mes/defect/review', data);
}

/**
 * 缺陷统计
 */
async function getDefectStatistics(params?: Recordable<any>) {
  return requestClient.get('/mes/defect/statistics', { params });
}

export {
  createDefect,
  deleteDefect,
  getDefectDetail,
  getDefectListByQCOrder,
  getDefectPage,
  getDefectStatistics,
  handleDefect,
  reviewDefect,
};
