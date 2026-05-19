import { requestClient } from '#/api/request';
import type { SelectViewDto } from '#/api/types';

export namespace ModelingApi {
  export interface Modeling {
    [key: string]: any;
    keyword?: string;
    modelingName?: string;
    dataId?: string
  }
}

/**
 * 获取模型下拉列表
 */
async function getModelingSelectList(data: ModelingApi.Modeling) {
  return requestClient.post<Array<SelectViewDto>>(
    '/rbac/modeling/selectlist',
    data
  );
}

const ModelingApiObj = {
  getSelectList: (data: ModelingApi.Modeling) => {
    return requestClient.post<Array<SelectViewDto>>(
      `/rbac/modeling/${data.modelingName}/selectlist`,
      data
    );
  },
  getData: (data: ModelingApi.Modeling) => {
    return requestClient.get<any>(
      `/rbac/modeling/${data.modelingName}/data?id=${data.dataId}`
    );
  }
}

export { getModelingSelectList, ModelingApiObj };
