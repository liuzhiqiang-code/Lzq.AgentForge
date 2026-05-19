import { requestClient } from '#/api/request';

/**
 * AI 技能管理 API
 */
export namespace AgentSkillApi {
  /** 技能列表项 */
  export interface SkillItem {
    skillName: string;
    skillDescription: string;
    type?: 'Internal' | 'External'; // 新增：技能类型
    tools: ToolItem[];
  }

  /** 工具项 */
  export interface ToolItem {
    toolName: string;
    description: string;
    parameters: ParameterItem[];
  }

  /** 工具参数项 */
  export interface ParameterItem {
    name: string;
    parameterType: string;
    description: string;
  }

  /** 执行技能请求 */
  export interface ExecuteSkillRequest {
    skillName: string;
    toolName: string;
    arguments: Record<string, any>;
  }
}

/**
 * 获取技能列表
 */
export async function getAgentSkillList() {
  return requestClient.post<AgentSkillApi.SkillItem[]>('/ai/agentSkill/list');
}

/**
 * 执行技能工具
 * @param data 执行请求
 */
export async function executeAgentSkill(data: AgentSkillApi.ExecuteSkillRequest) {
  return requestClient.post<{ success: boolean; data: any }>(
    '/ai/agentSkill/execute',
    data,
  );
}

/** 上传技能程序集 (DLL) */
export function uploadPlugin(file: File) {
  const fd = new FormData();
  fd.append('file', file);
  return requestClient.post('/ai/agentSkill/upload-plugin', fd, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

/** 上传外部技能压缩包 (ZIP) */
export function uploadExternalZip(file: File) {
  const fd = new FormData();
  fd.append('file', file);
  return requestClient.post('/ai/agentSkill/upload-external', fd, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}
