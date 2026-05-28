import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace QCOrderApi {
  export interface QCOrderItem {
    [key: string]: any;
    id?: number;
    code?: string;
    qcType?: number;
    qcTypeName?: string;
    refCode?: string;
    refId?: number;
    supplierId?: number;
    supplierName?: string;
    productId?: number;
    productName?: string;
    productSpec?: string;
    batchNo?: string;
    submitQty?: number;
    qualifiedQty?: number;
    unqualifiedQty?: number;
    status?: number;
    statusName?: string;
    inspectorId?: number;
    inspectorName?: string;
    inspectDate?: string;
    completedTime?: string;
    qcStandard?: string;
    conclusion?: string;
    remark?: string;
    createTime?: string;
    updateTime?: string;
    items?: QCOrderItemViewDto[];
  }

  export interface QCOrderItemViewDto {
    id?: number;
    qcOrderId?: number;
    itemName?: string;
    itemType?: number;
    itemTypeName?: string;
    standard?: string;
    method?: string;
    sampleQty?: number;
    qualifiedQty?: number;
    unqualifiedQty?: number;
    result?: number;
    resultName?: string;
    defectDesc?: string;
    defectCode?: string;
    remark?: string;
  }
}

/**
 * 分页查询质检单
 */
async function getQCOrderPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<QCOrderApi.QCOrderItem>>(
    '/mes/qc-order/page',
    params,
  );
}

/**
 * 获取质检单详情
 */
async function getQCOrderDetail(id: number) {
  return requestClient.get<QCOrderApi.QCOrderItem>(`/mes/qc-order/${id}`);
}

/**
 * 获取检验明细
 */
async function getQCOrderItems(qcOrderId: number) {
  return requestClient.get<QCOrderApi.QCOrderItemViewDto[]>(
    '/mes/qc-order/items',
    { params: { qcOrderId } },
  );
}

/**
 * 创建质检单
 */
async function createQCOrder(data: QCOrderApi.QCOrderItem) {
  return requestClient.post('/mes/qc-order/create', data);
}

/**
 * 更新质检单
 */
async function updateQCOrder(data: QCOrderApi.QCOrderItem) {
  return requestClient.put('/mes/qc-order/update', data);
}

/**
 * 删除质检单
 */
async function deleteQCOrder(id: number) {
  return requestClient.delete(`/mes/qc-order/delete/${id}`);
}

/**
 * 提交检验
 */
async function submitInspectQCOrder(data: { id: number }) {
  return requestClient.post('/mes/qc-order/submit-inspect', data);
}

/**
 * 判定质检单（合格/不合格）  判定结果：2-合格 3-不合格
 */
async function judgeQCOrder(data: { id: number; conclusion: string; result: number }) {
  return requestClient.post('/mes/qc-order/judge', data);
}

/**
 * 取消质检单
 */
async function cancelQCOrder(data: { id: number }) {
  return requestClient.post('/mes/qc-order/cancel', data);
}

export {
  cancelQCOrder,
  createQCOrder,
  deleteQCOrder,
  getQCOrderDetail,
  getQCOrderItems,
  getQCOrderPage,
  judgeQCOrder,
  submitInspectQCOrder,
  updateQCOrder,
};
