import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace LineApi {
  export interface LineItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    workshopId?: number;
    workshopName?: string;
    status?: number;
  }
}

/**
 * 分页查询产线
 */
async function getLinePage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<LineApi.LineItem>>(
    '/mes/line/page',
    params,
  );
}

/**
 * 按车间获取产线列表(联动下拉)
 */
async function getLineListByWorkshop(workshopId: number) {
  return requestClient.post<Array<LineApi.LineItem>>(
    `/mes/line/list-by-workshop/${workshopId}`,
  );
}

/**
 * 创建产线
 */
async function createLine(data: LineApi.LineItem) {
  return requestClient.post('/mes/line/create', data);
}

/**
 * 更新产线
 */
async function updateLine(data: LineApi.LineItem) {
  return requestClient.put('/mes/line/update', data);
}

/**
 * 删除产线
 */
async function deleteLine(id: number) {
  return requestClient.delete(`/mes/line/delete/${id}`);
}

/**
 * 批量删除产线
 */
async function batchDeleteLine(ids: number[]) {
  return requestClient.post('/mes/line/batch-delete', ids);
}

export {
  batchDeleteLine,
  createLine,
  deleteLine,
  getLineListByWorkshop,
  getLinePage,
  updateLine,
};
