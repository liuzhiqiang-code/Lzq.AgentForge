import { requestClient } from '#/api/request';

// ==================== 顶部卡片数据 ====================
export interface TopCardData {
  totalConversations: number;
  todayConversations: number;
  activeAgents: number;
  totalAgents: number;
  todayApiCalls: number;
  totalApiCalls: number;
  todaySkillCalls: number;
  totalSkillCalls: number;
}

export async function getTopCard() {
  return requestClient.get<TopCardData>('/ai/analytics/topCard');
}

// ==================== 对话请求趋势（按天） ====================
export interface ConversationTrendsData {
  dates: string[];            // ['05-02', '05-03', ...]
  conversations: number[];    // 对话数
  apiCalls: number[];         // API 调用数
}

export async function getConversationTrends(params?: { days?: number }) {
  return requestClient.get<ConversationTrendsData>('/ai/analytics/conversation-trends', { params });
}

// ==================== 模型调用量月度统计 ====================
export interface ModelUsageMonthlyData {
  months: string[];   // ['1月', '2月', ...]
  values: number[];   // 调用次数（或 token 消耗）
}

export async function getModelUsageMonthly(params?: { months?: number }) {
  return requestClient.get<ModelUsageMonthlyData>('/ai/analytics/model-usage-monthly', { params });
}

// ==================== 智能体使用排行（用于下方的柱状图/条形图） ====================
export interface AgentUsageItem {
  name: string;       // 智能体名称
  count: number;      // 调用次数
}

export async function getAgentUsageRanking(params?: { top?: number }) {
  return requestClient.get<AgentUsageItem[]>('/ai/analytics/agent-usage-ranking', { params });
}

// ==================== 模型调用分布（用于饼图） ====================
export interface ModelDistributionItem {
  name: string;       // 模型名称
  value: number;      // 调用占比或次数
}

export async function getModelDistribution() {
  return requestClient.get<ModelDistributionItem[]>('/ai/analytics/model-distribution');
}

// ==================== 技能使用统计（用于玫瑰图/条形图） ====================
export interface SkillStatsItem {
  name: string;       // 技能名称
  count: number;      // 调用次数
}

export async function getSkillStats() {
  return requestClient.get<SkillStatsItem[]>('/ai/analytics/skill-stats');
}
