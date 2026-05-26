<script lang="ts" setup>
import { onMounted, ref, markRaw } from 'vue';
import type { AnalysisOverviewItem } from '@vben/common-ui';
import type { TabOption } from '@vben/types';

import {
  AnalysisChartCard,
  AnalysisChartsTabs,
  AnalysisOverview,
} from '@vben/common-ui';
import {
  MdiMessageOutline,
  MdiRobotOutline,
  MdiApi,
  MdiPuzzleOutline,
} from '@vben/icons';

import { getTopCard } from '#/api/dashboard/analytics';

import AnalyticsConversationTrends from './analytics-conversation-trends.vue';
import AnalyticsModelMonthlyUsage from './analytics-model-monthly-usage.vue';
import AnalyticsAgentRanking from './analytics-agent-ranking.vue';
import AnalyticsModelDistribution from './analytics-model-distribution.vue';
import AnalyticsSkillStats from './analytics-skill-stats.vue';

const overviewItems = ref<AnalysisOverviewItem[]>([
  {
    icon: markRaw(MdiMessageOutline),
    title: '总对话数',
    totalTitle: '累计对话',
    totalValue: 0,
    value: 0,
  },
  {
    icon: markRaw(MdiRobotOutline),
    title: '活跃智能体',
    totalTitle: '智能体总数',
    totalValue: 0,
    value: 0,
  },
  {
    icon: markRaw(MdiApi),
    title: '模型调用量',
    totalTitle: '累计调用',
    totalValue: 0,
    value: 0,
  },
  {
    icon: markRaw(MdiPuzzleOutline),
    title: '技能使用量',
    totalTitle: '今日调用次数',
    totalValue: 0,
    value: 0,
  },
]);

async function fetchTopCard() {
  try {
    const data = await getTopCard();
    // 如果 requestClient 返回的是包装对象 { code, data }，请解构：const { data } = await getTopCard();
    overviewItems.value[0]!.value = data.todayConversations;
    overviewItems.value[0]!.totalValue = data.totalConversations;

    overviewItems.value[1]!.value = data.activeAgents;
    overviewItems.value[1]!.totalValue = data.totalAgents;

    overviewItems.value[2]!.value = data.todayApiCalls;
    overviewItems.value[2]!.totalValue = data.totalApiCalls;

    overviewItems.value[3]!.value = data.todaySkillCalls;
    overviewItems.value[3]!.totalValue = data.totalSkillCalls;
  } catch (error) {
    console.error('获取顶部卡片数据失败', error);
  }
}

const chartTabs: TabOption[] = [
  {
    label: '对话趋势',
    value: 'conversation-trends',
  },
  {
    label: '模型调用量',
    value: 'model-usage',
  },
];

onMounted(() => {
  fetchTopCard();
});

</script>

<template>
  <div class="p-5">
    <AnalysisOverview :items="overviewItems" />
    <AnalysisChartsTabs :tabs="chartTabs" class="mt-5">
      <template #conversation-trends>
        <AnalyticsConversationTrends />
      </template>
      <template #model-usage>
        <AnalyticsModelMonthlyUsage />
      </template>
    </AnalysisChartsTabs>

    <div class="mt-5 w-full md:flex">
      <!-- 替换原本的访问数量为：智能体排行 -->
      <AnalysisChartCard class="mt-5 md:mr-4 md:mt-0 md:w-1/3" title="智能体活跃排行">
        <AnalyticsAgentRanking />
      </AnalysisChartCard>

      <!-- 替换原本的访问来源为：模型分布 -->
      <AnalysisChartCard class="mt-5 md:mr-4 md:mt-0 md:w-1/3" title="模型调用分布">
        <AnalyticsModelDistribution />
      </AnalysisChartCard>

      <!-- 替换原本的访问来源为：技能使用统计 -->
      <AnalysisChartCard class="mt-5 md:mt-0 md:w-1/3" title="技能调用排行">
        <AnalyticsSkillStats />
      </AnalysisChartCard>
    </div>
  </div>
</template>
