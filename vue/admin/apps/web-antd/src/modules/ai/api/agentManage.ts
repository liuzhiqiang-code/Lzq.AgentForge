import { requestClient } from '#/api/request';
import type { PagedResponse } from '#/api/types'

export namespace AgentManageApi {
  export interface AgentManage {
    [key: string]: any;
    id?: string;
    name?: string;
    description?: string;
    instructions?: string;
    temperature?: number;
    maxOutputTokens?: number;
    topP?: number;
    frequencyPenalty?: number;
    presencePenalty?: number;
    selectedSkills?: SkillMethodEntry[];
    creator?: string;
    creationTime?: string;
    modifier?: string;
  }
  export interface SkillMethodEntry {
    skillName: string;
  }
}

/**
 * 获取列表数据
 */
export async function getAgentManageList() {
  return requestClient.post<Array<AgentManageApi.AgentManage>>(
    '/ai/agentManage/list',
  );
}

/**
 * 获取分页数据
 */
export async function getAgentManagePage(data: AgentManageApi.AgentManage) {
  return requestClient.post<PagedResponse<AgentManageApi.AgentManage>>(
    '/ai/agentManage/page',data,
  );
}

/**
 * 创建
 * @param data 数据
 */
export async function createAgentManage(
  data: AgentManageApi.AgentManage,
) {
  return requestClient.post('/ai/agentManage/create', data);
}

/**
 * 更新
 * @param data 数据
 */
export async function updateAgentManage(
  data: AgentManageApi.AgentManage,
) {
  return requestClient.put(`/ai/agentManage/update`, data);
}

/**
 * 删除
 * @param id ID
 */
export async function deleteAgentManage(id: string) {
  return requestClient.delete(`/ai/agentManage/delete/${id}`);
}
