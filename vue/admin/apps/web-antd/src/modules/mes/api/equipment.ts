import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace EquipmentApi {
  export interface EquipmentItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    equipmentType?: number;
    equipmentTypeName?: string;
    spec?: string;
    brand?: string;
    supplier?: string;
    purchaseDate?: string;
    warrantyEndDate?: string;
    status?: number;
    statusName?: string;
    lineId?: number;
    lineName?: string;
    location?: string;
    responsibleId?: number;
    responsibleName?: string;
    photos?: string;
    parameters?: string;
    totalRunningHours?: number;
    totalRepairCount?: number;
    remark?: string;
    createTime?: string;
    updateTime?: string;
  }
}

/**
 * 分页查询设备
 */
async function getEquipmentPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<EquipmentApi.EquipmentItem>>(
    '/mes/equipment/page',
    params,
  );
}

/**
 * 获取设备详情
 */
async function getEquipmentDetail(id: number) {
  return requestClient.get<EquipmentApi.EquipmentItem>(`/mes/equipment/${id}`);
}

/**
 * 创建设备
 */
async function createEquipment(data: EquipmentApi.EquipmentItem) {
  return requestClient.post('/mes/equipment/create', data);
}

/**
 * 更新设备
 */
async function updateEquipment(data: EquipmentApi.EquipmentItem) {
  return requestClient.put('/mes/equipment/update', data);
}

/**
 * 删除设备
 */
async function deleteEquipment(id: number) {
  return requestClient.delete(`/mes/equipment/delete/${id}`);
}

/**
 * 更新设备状态
 */
async function updateEquipmentStatus(data: { id: number; status: number }) {
  return requestClient.post('/mes/equipment/update-status', data);
}

/**
 * 设备统计
 */
async function getEquipmentStatistics() {
  return requestClient.get('/mes/equipment/statistics');
}

/**
 * 获取设备列表（用于下拉选择）
 */
async function getEquipmentList() {
  return requestClient.post('/mes/equipment/page', { pageNum: 1, pageSize: 1000 });
}

export {
  createEquipment,
  deleteEquipment,
  getEquipmentDetail,
  getEquipmentList,
  getEquipmentPage,
  getEquipmentStatistics,
  updateEquipment,
  updateEquipmentStatus,
};
