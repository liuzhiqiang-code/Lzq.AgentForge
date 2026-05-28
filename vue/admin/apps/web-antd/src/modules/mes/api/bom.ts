import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace BomApi {
  export interface BomItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    productId?: number;
    productName?: string;
    productCode?: string;
    version?: string;
    status?: number;
    statusText?: string;
    effDate?: string;
    expDate?: string;
    remark?: string;
    creationTime?: string;
  }

  export interface BomDetail {
    header: BomItem;
    items: BomItemDetail[];
  }

  export interface BomItemDetail {
    [key: string]: any;
    id?: number;
    bomId?: number;
    itemId?: number;
    itemCode?: string;
    itemName?: string;
    itemSpec?: string;
    qty?: number;
    scrapRate?: number;
    sort?: number;
    substituteIds?: string;
    remark?: string;
  }

  export interface VersionHistoryItem {
    [key: string]: any;
    id?: number;
    bomId?: number;
    version?: string;
    changeDescription?: string;
    creationTime?: string;
  }

  export interface BomDiffResult {
    oldVersion: string;
    newVersion: string;
    headerDiff: string;
    itemChanges: BomItemDiff[];
  }

  export interface BomItemDiff {
    changeType: string;
    itemId: number;
    itemCode?: string;
    itemName?: string;
    oldQty?: number;
    newQty?: number;
    oldScrapRate?: number;
    newScrapRate?: number;
  }
}

async function getBomPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<BomApi.BomItem>>(
    '/mes/bom/page',
    params,
  );
}

async function getBomDetail(id: number) {
  return requestClient.get<BomApi.BomDetail>(
    `/mes/bom/get/${id}`,
  );
}

async function createBom(data: Recordable<any>) {
  return requestClient.post<number>('/mes/bom/create', data);
}

async function updateBom(data: Recordable<any>) {
  return requestClient.put<boolean>('/mes/bom/update', data);
}

async function deleteBom(id: number) {
  return requestClient.delete<boolean>(`/mes/bom/delete/${id}`);
}

async function copyBom(id: number) {
  // backend RoutePattern has `true` (POST), but uses GET in swagger
  // Use POST to be safe
  return requestClient.post<number>(`/mes/bom/copy/${id}`);
}

async function releaseBom(id: number, changeDescription?: string) {
  return requestClient.post<boolean>(`/mes/bom/release/${id}`, { changeDescription });
}

async function rollbackBom(bomId: number, historyId: number) {
  return requestClient.post<boolean>(`/mes/bom/rollback/${bomId}/${historyId}`);
}

async function getBomItems(bomId: number) {
  return requestClient.get<BomApi.BomItemDetail[]>(
    `/mes/bom/items/${bomId}`,
  );
}

async function createBomItem(data: Recordable<any>) {
  return requestClient.post<number>('/mes/bom/item/create', data);
}

async function updateBomItem(data: Recordable<any>) {
  return requestClient.put<boolean>('/mes/bom/item/update', data);
}

async function deleteBomItem(id: number) {
  return requestClient.delete<boolean>(`/mes/bom/item/delete/${id}`);
}

async function getVersionHistory(bomId: number) {
  return requestClient.get<BomApi.VersionHistoryItem[]>(
    `/mes/bom/version-history/${bomId}`,
  );
}

async function getBomDiff(historyId1: number, historyId2: number) {
  return requestClient.get<BomApi.BomDiffResult>(
    `/mes/bom/diff/${historyId1}/${historyId2}`,
  );
}

async function previewVersion(historyId: number) {
  return requestClient.get<BomApi.BomDetail>(
    `/mes/bom/preview-version/${historyId}`,
  );
}

export {
  copyBom,
  createBom,
  createBomItem,
  deleteBom,
  deleteBomItem,
  getBomDetail,
  getBomDiff,
  getBomItems,
  getBomPage,
  getVersionHistory,
  previewVersion,
  releaseBom,
  rollbackBom,
  updateBom,
  updateBomItem,
};
