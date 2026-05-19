import { requestClient } from '#/api/request';

/**
 * 产量统计汇总（新路径：/dashboard/kanban/production/summary）
 * 参数：lineId (可选)
 * 后端返回格式：{ code: 0, data: ProductionOutputSummaryDto }
 */
export async function getProductionOutput(params: { lineId?: number }) {
  const queryString = new URLSearchParams();
  if (params.lineId !== undefined) {
    queryString.append('lineId', params.lineId.toString());
  }
  return requestClient.get<any>(`/dashboard/kanban/production/summary?${queryString.toString()}`);
}

/**
 * 工单完成率汇总（新路径：/dashboard/kanban/workorder/summary）
 * 参数：lineId (可选)
 * 后端返回格式：{ code: 0, data: WorkOrderCompletionSummaryDto }
 */
export async function getCompletionRate(params: { lineId?: number }) {
  const queryString = new URLSearchParams();
  if (params.lineId !== undefined) {
    queryString.append('lineId', params.lineId.toString());
  }
  return requestClient.get<any>(`/dashboard/kanban/workorder/summary?${queryString.toString()}`);
}

/**
 * 不良率趋势汇总（新路径：/dashboard/kanban/defect/summary）
 * 参数：lineId (可选)
 * 后端返回格式：{ code: 0, data: DefectRateSummaryDto }
 */
export async function getDefectRateStatistics(params: { lineId?: number }) {
  const queryString = new URLSearchParams();
  if (params.lineId !== undefined) {
    queryString.append('lineId', params.lineId.toString());
  }
  return requestClient.get<any>(`/dashboard/kanban/defect/summary?${queryString.toString()}`);
}

/**
 * 设备状态概览（新路径：/dashboard/kanban/equipment/overview）
 * 参数：无
 * 后端返回格式：{ code: 0, data: EquipmentStatusOverviewDto }
 */
export async function getEquipmentStatusOverview() {
  return requestClient.get<any>('/dashboard/kanban/equipment/overview');
}

/**
 * 各产线设备状态（保留旧路径或需要新接口）
 * TODO: 确认是否有对应的新接口路径
 */
export async function getEquipmentStatusByLine(params: { start: string; end: string }) {
  const queryString = new URLSearchParams();
  queryString.append('start', params.start);
  queryString.append('end', params.end);
  return requestClient.get<any>(`/mes/equipment/statistics/GetStatusByLine?${queryString.toString()}`);
}
