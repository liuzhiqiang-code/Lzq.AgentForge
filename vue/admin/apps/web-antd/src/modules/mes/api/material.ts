import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types';
import type { Recordable } from '@vben/types';

export namespace MaterialApi {
  export interface MaterialItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    spec?: string;
    materialTypeId?: number;
    materialTypeName?: string;
    unitId?: number;
    unitName?: string;
    weight?: number;
    volume?: number;
    status?: number;
    creationTime?: string;
  }

  export interface MaterialTypeItem {
    [key: string]: any;
    id?: number;
    code?: string;
    name?: string;
    parentId?: number | null;
    level?: number;
    children?: MaterialTypeItem[];
  }

  export interface UnitOfMeasureItem {
    [key: string]: any;
    id?: number;
    name?: string;
    code?: string;
  }
}

async function getMaterialPage(params: Recordable<any>) {
  return requestClient.post<PagedResponse<MaterialApi.MaterialItem>>(
    '/mes/material/page',
    params,
  );
}

async function getMaterialDetail(id: number) {
  return requestClient.get<MaterialApi.MaterialItem>(
    `/mes/material/get/${id}`,
  );
}

async function getMaterialSelectList() {
  return requestClient.get<MaterialApi.MaterialItem[]>(
    '/mes/material/select-list',
  );
}

async function createMaterial(data: Recordable<any>) {
  return requestClient.post<number>('/mes/material/create', data);
}

async function updateMaterial(data: Recordable<any>) {
  return requestClient.put<boolean>('/mes/material/update', data);
}

async function deleteMaterial(id: number) {
  return requestClient.delete<boolean>(`/mes/material/delete/${id}`);
}

async function batchDeleteMaterial(ids: number[]) {
  return requestClient.post<number>('/mes/material/batch-delete', ids);
}

async function getMaterialTypeTree() {
  return requestClient.post<MaterialApi.MaterialTypeItem[]>(
    '/mes/material-type/tree',
  );
}

async function getUnitOfMeasureSelectList() {
  return requestClient.get<MaterialApi.UnitOfMeasureItem[]>(
    '/mes/unit-of-measure/select-list',
  );
}

async function createMaterialType(data: Recordable<any>) {
  return requestClient.post<number>('/mes/material-type/create', data);
}

async function deleteMaterialType(id: number) {
  return requestClient.delete<boolean>(`/mes/material-type/delete/${id}`);
}

export {
  batchDeleteMaterial,
  createMaterial,
  createMaterialType,
  deleteMaterial,
  deleteMaterialType,
  getMaterialDetail,
  getMaterialPage,
  getMaterialSelectList,
  getMaterialTypeTree,
  getUnitOfMeasureSelectList,
  updateMaterial,
};
